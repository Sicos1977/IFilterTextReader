//
// FilterReader.cs
//
// Author: Kees van Spelde <sicos2002@hotmail.com>
//
// Copyright (c) 2013-2021 Magic-Sessions. (www.magic-sessions.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using IFilterTextReader.Exceptions;
// ReSharper disable LocalizableElement
// ReSharper disable FunctionComplexityOverflow
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable EventNeverSubscribedTo.Global

namespace IFilterTextReader
{
    #region FilterReaderTimeout
    /// <summary>
    /// A method to control the parsing of very large files
    /// </summary>
    public enum FilterReaderTimeout
    {
        /// <summary>
        /// The reader does not timeout and reads to the end of the file
        /// </summary>
        NoTimeout,

        /// <summary>
        /// The reader times out and returns like it has succesfully parsed the complete file
        /// </summary>
        TimeoutOnly,

        /// <summary>
        /// The reader times out and throws the exception <see cref="IFFilterTimeout"/>
        /// </summary>
        TimeoutWithException
    }
    #endregion

    /// <summary>
    /// This class is a <see cref="TextReader"/> wrapper around an <see cref="NativeMethods.IFilter"/>. This way a file can be processed 
    /// like if it is a dead normal text file
    /// </summary>
    public class FilterReader : TextReader
    {
        #region Delegates
        /// <summary>
        /// Raised when an unmapped property has been retrieved with the <see cref="NativeMethods.IFilter.GetValue"/> method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"><see cref="UnmappedPropertyEventArgs"/></param>
        public delegate void UnmappedPropertyEventHandler(Object sender, UnmappedPropertyEventArgs e);
        #endregion

        #region Events
        /// <summary>
        /// Raised when an unmapped property has been retrieved with the <see cref="NativeMethods.IFilter.GetValue"/> method
        /// </summary>
        public event UnmappedPropertyEventHandler UnmappedPropertyEvent;
        #endregion

        #region Fields
        /// <summary>
        /// The file to read with an IFilter
        /// </summary>
        private readonly string _fileName;

        /// <summary>
        /// The <see cref="_fileName"/> as a <see cref="Stream"/>
        /// </summary>
        private readonly Stream _fileStream;

        /// <summary>
        /// The IFilter interface
        /// </summary>
        private readonly NativeMethods.IFilter _filter;

        /// <summary>
        /// Contains the chunk that we are reading from the <see cref="NativeMethods.IFilter"/> interface
        /// </summary>
        private NativeMethods.STAT_CHUNK _chunk;

        /// <summary>
        /// The guidPropSet of the last chunk attributes for which text was extracted
        /// </summary>
        private Guid _chunkAttributesGuidPropSetOfLastText;

        /// <summary>
        /// Contains pointer to non-movable memory block that holds the STAT_CHUNK structure
        /// </summary>
        private IntPtr _chunkBuffer;

        /// <summary>
        /// Indicates if the <see cref="_chunk"/> is valid
        /// </summary>
        private bool _chunkValid;

        /// <summary>
        /// Indicates that we are done with processing all the text that has been returned by the 
        /// <see cref="_chunk"/>
        /// </summary>
        private bool _done;

        /// <summary>
        /// Indicates that we are done with reading <see cref="_chunk">chunks</see>
        /// </summary>
        private bool _endOfChunks;

        /// <summary>
        /// Holds the chars that are left from the last chunck read
        /// </summary>
        private char[] _charsLeftFromLastRead;

        /// <summary>
        /// <see cref="FilterReaderOptions"/>
        /// </summary>
        private readonly FilterReaderOptions _options = new FilterReaderOptions();

        /// <summary>
        /// Used in conjuction with <see cref="_options"/>
        /// </summary>
        private Stopwatch _stopwatch;

        /// <summary>
        /// Indicates when true that a carriage return was found on the previous line
        /// </summary>
        private bool _carriageReturnFound;

        /// <summary>
        /// Collection of metadata properties extracted from file
        /// </summary>
        public readonly Dictionary<string, List<object>> MetaDataProperties = new Dictionary<string, List<object>>();
        #endregion

        #region Constructor en Destructor
        /// <summary>
        /// Creates an TextReader object for the given <paramref name="fileName"/>
        /// </summary>
        /// <param name="fileName">The file to read</param>
        /// <param name="extension">Overrides the file extension of the <paramref name="fileName"/>, 
        /// the extension is used to determine the <see cref="NativeMethods.IFilter"/> that needs to
        /// be used to read the <paramref name="fileName"/></param>
        /// <param name="filterReaderOptions"><see cref="FilterReaderOptions"/></param>
        public FilterReader(string fileName,
                            string extension = "",
                            FilterReaderOptions filterReaderOptions = null)
        {
            try
            {
                if (filterReaderOptions != null)
                    _options = filterReaderOptions;

                _fileName = fileName;
                _fileStream = File.OpenRead(fileName);

                if (string.IsNullOrWhiteSpace(extension))
                {
                    extension = Path.GetExtension(fileName);
                    if (string.IsNullOrWhiteSpace(extension))
                    {
                        // Try to detect the extension if the file does not have one
                        var fileInfo = FileTypeSelector.GetFileTypeFileInfo(fileName);
                        if (fileInfo != null)
                            extension = fileInfo.Extension;
                    }
                }

                _filter = FilterLoader.LoadAndInitIFilter(
                    _fileStream,
                    extension,
                    _options.DisableEmbeddedContent,
                    fileName,
                    _options.ReadIntoMemory);

                if (_filter == null)
                {
                    if (string.IsNullOrWhiteSpace(extension))
                        throw new IFFilterNotFound(
                            $"There is no {(Environment.Is64BitProcess ? "64" : "32")} bits IFilter installed for the file '{Path.GetFileName(fileName)}'");

                    throw new IFFilterNotFound(
                        $"There is no {(Environment.Is64BitProcess ? "64" : "32")} bits IFilter installed for the extension '{extension}'");
                }

                if (_options.ReaderTimeout != FilterReaderTimeout.NoTimeout && _options.Timeout < 0)
                    throw new ArgumentException($"Needs to be larger then 0", nameof(_options.Timeout));
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// Creates an TextReader object for the given <see cref="Stream"/>
        /// </summary>
        /// <param name="stream">The file stream to read</param>
        /// <param name="extension">The extension for the <paramref name="stream"/></param>
        /// <param name="filterReaderOptions"><see cref="FilterReaderOptions"/></param>
        public FilterReader(Stream stream,
                            string extension,
                            FilterReaderOptions filterReaderOptions)
        {
            if (string.IsNullOrWhiteSpace(extension))
                throw new ArgumentException("The extension cannot be empty", nameof(extension));

            if (filterReaderOptions != null)
                _options = filterReaderOptions;

            _filter = FilterLoader.LoadAndInitIFilter(
                stream,
                extension,
                _options.DisableEmbeddedContent, string.Empty,
                _options.ReadIntoMemory);

            if (_filter == null)
                throw new IFFilterNotFound(
                    $"There is no {(Environment.Is64BitProcess ? "64" : "32")} bits IFilter installed for the stream with the extension '{extension}'");

            if (_options.ReaderTimeout != FilterReaderTimeout.NoTimeout && _options.Timeout < 0)
                throw new ArgumentException($"Needs to be larger then 0", nameof(_options.Timeout));
        }

        /// <summary>
        /// Disposes this object
        /// </summary>
        ~FilterReader()
        {
            _stopwatch?.Stop();

            Dispose(false);
        }
        #endregion

        #region Timeout
        /// <summary>
        /// Validates if the <see cref="_stopwatch"/> has passed the <see cref="FilterReaderOptions.Timeout"/> value when
        /// <see cref="FilterReaderOptions.ReaderTimeout"/> is set to anything but <see cref="FilterReaderTimeout.NoTimeout"/> and
        /// takes the correct action
        /// </summary>
        private bool Timeout()
        {
            if (_options.ReaderTimeout == FilterReaderTimeout.NoTimeout) return false;

            if (_stopwatch == null)
            {
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
                return false;
            }

            if (_stopwatch.ElapsedMilliseconds >= _options.Timeout)
            {
                switch (_options.ReaderTimeout)
                {
                    case FilterReaderTimeout.TimeoutOnly:
                        return true;

                    case FilterReaderTimeout.TimeoutWithException:
                        throw new IFFilterTimeout("The IFilter has timed out");
                }
            }

            return false;
        }
        #endregion

        #region ReadLine
        /// <summary>
        /// Reads a line of characters from the text reader and returns the data as a string.
        /// </summary>
        /// <returns>The next line from the reader, or null if all characters have been read.</returns>
        /// <exception cref="IFFileIsPasswordProtected">Raised when a file is password protected</exception>
        /// <exception cref="IFAccessFailure">Raised when the <see cref="_fileName"/> or IFilter file can not be accessed</exception>
        /// <exception cref="OutOfMemoryException">Raised when there is not enough memory to process the file</exception>
        /// <exception cref="IFUnknownFormat">Raised when the <see cref="_fileName"/> is not in the format the IFilter expect it to be 
        /// <exception cref="IFOldFilterFormat">Raised when an old <see cref="NativeMethods.IFilter"/> format is used and no filename is supplied</exception>
        /// (e.g. files with a wrong extension)</exception>
        public override string ReadLine()
        {
            var buffer = new char[2048];
            var stringBuilder = new StringBuilder();
            var done = false;

            while (!done)
            {
                var charsRead = Read(buffer, 0, 1024);

                if (charsRead <= 0)
                {
                    if (stringBuilder.Length != 0)
                        done = true;
                    else
                        return null;
                }

                for (var i = 0; i < charsRead; i++)
                {
                    var chr = buffer[i];

                    switch (chr)
                    {
                        case '\r':
                            _carriageReturnFound = true;
                            done = true;
                            break;

                        case '\n':
                            if (!_carriageReturnFound)
                                done = true;

                            break;

                        default:
                            _carriageReturnFound = false;
                            stringBuilder.Append(buffer[i]);
                            break;
                    }

                    if (done)
                    {
                        i++;
                        if (i < charsRead)
                        {
                            if (_charsLeftFromLastRead == null)
                            {
                                _charsLeftFromLastRead = new char[charsRead - i];
                                Array.Copy(buffer, i, _charsLeftFromLastRead, 0, charsRead - i);
                            }
                            else
                            {
                                var charsLeft = charsRead - i;
                                var temp = new char[_charsLeftFromLastRead.Length + charsLeft];
                                Array.Copy(buffer, i, temp, 0, charsLeft);
                                _charsLeftFromLastRead.CopyTo(temp, charsLeft);
                                _charsLeftFromLastRead = temp;
                            }
                        }

                        _done = false;
                        break;
                    }
                }
            }

            return stringBuilder.Length == 0 ? string.Empty : stringBuilder.ToString();
        }
        #endregion

        #region Read
        /// <summary>
        /// Reads the next character from the text reader and advances the character position by one character.
        /// </summary>
        /// <returns>The next character from the text reader, or -1 if no more characters are available.</returns>
        /// <exception cref="IFFileIsPasswordProtected">Raised when a file is password protected</exception>
        /// <exception cref="IFAccessFailure">Raised when the <see cref="_fileName"/> or IFilter file can not be accessed</exception>
        /// <exception cref="OutOfMemoryException">Raised when there is not enough memory to process the file</exception>
        /// <exception cref="IFUnknownFormat">Raised when the <see cref="_fileName"/> is not in the format the IFilter expect it to be 
        /// <exception cref="IFOldFilterFormat">Raised when an old <see cref="NativeMethods.IFilter"/> format is used and no filename is supplied</exception>
        /// (e.g. files with a wrong extension)</exception>
        public override int Read()
        {
            if (Timeout()) return -1;

            var chr = new char[0];
            var read = Read(chr, 0, 1);
            if (read == 1)
                return chr[0];

            return -1;
        }

        /// <summary>
        /// Reads a specified maximum number of characters from the current reader and writes the data to a buffer, 
        /// beginning at the specified index
        /// </summary>
        /// <param name="buffer">When this method returns, contains the specified character array with the values 
        /// between index and (index + count - 1) replaced by the characters read from the current source. </param>
        /// <param name="index">The position in buffer at which to begin writing. </param>
        /// <param name="count">The maximum number of characters to read. If the end of the reader is reached before 
        /// the specified number of characters is read into the buffer, the method returns. </param>
        /// <returns>The number of characters that have been read. The number will be less than or equal to count, 
        /// depending on whether the data is available within the reader.  This method returns 0 (zero) if it is called
        /// when no more characters are left to read</returns>
        /// <exception cref="IFFileIsPasswordProtected">Raised when a file is password protected</exception>
        /// <exception cref="IFAccessFailure">Raised when the <see cref="_fileName"/> or IFilter file can not be accessed</exception>
        /// <exception cref="OutOfMemoryException">Raised when there is not enough memory to process the file</exception>
        /// <exception cref="IFUnknownFormat">Raised when the <see cref="_fileName"/> is not in the format the IFilter expect it to be 
        /// <exception cref="IFOldFilterFormat">Raised when an old <see cref="NativeMethods.IFilter"/> format is used and no filename is supplied</exception>
        /// (e.g. files with a wrong extension)</exception>
        public override int Read(char[] buffer, int index, int count)
        {
            var breakChar = string.Empty;

            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (buffer.Length - index < count)
                throw new ArgumentException("The buffer is to small");

            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            var charsRead = 0;

            if (Timeout())
                return 0;

            while (!_done && charsRead < count)
            {
                if (_charsLeftFromLastRead != null)
                {
                    var charsToCopy = _charsLeftFromLastRead.Length < count - charsRead
                        ? _charsLeftFromLastRead.Length
                        : count - charsRead;

                    Array.Copy(_charsLeftFromLastRead, 0, buffer, index + charsRead, charsToCopy);

                    charsRead += charsToCopy;

                    if (charsToCopy < _charsLeftFromLastRead.Length)
                    {
                        var temp = new char[_charsLeftFromLastRead.Length - charsToCopy];
                        Array.Copy(_charsLeftFromLastRead, charsToCopy, temp, 0, temp.Length);
                        _charsLeftFromLastRead = temp;
                    }
                    else
                        _charsLeftFromLastRead = null;

                    if (_charsLeftFromLastRead == null && _endOfChunks)
                        _done = true;

                    continue;
                }

                // If we don't have a valid chunck anymore then read a new chunck
                if (!_chunkValid)
                {
                    if (_chunkBuffer == IntPtr.Zero)
                    {
                        // Keep the buffer allocated in a non-movable memory block. Some filters
                        // (offfilt.dll) expect the buffer passed to GetChunk(buffer) call to
                        // be valid even during the later call to GetText/GetValue methods.
                        //
                        // This means that we either have to pin a managed memory or allocate
                        // an unmanaged one. Since the buffer lifetime is not bound to single
                        // method call pinning is potentially expensive operation (until pinned
                        // heap becomes available in .NET 5). Use the unmanaged allocator instead.
                        _chunkBuffer = Marshal.AllocCoTaskMem(Marshal.SizeOf<NativeMethods.STAT_CHUNK>());
                    }

                    var result = _filter.GetChunk(_chunkBuffer);
                    _chunkValid = result == NativeMethods.IFilterReturnCode.S_OK;

                    if (_chunkValid)
                        _chunk = Marshal.PtrToStructure<NativeMethods.STAT_CHUNK>(_chunkBuffer);

                    switch (result)
                    {
                        case NativeMethods.IFilterReturnCode.FILTER_E_PARTIALLY_FILTERED:
                            throw new IFFilterPartiallyFiltered("The file was to large to filter completely");

                        case NativeMethods.IFilterReturnCode.FILTER_E_ACCESS:
                            throw new IFAccessFailure("Could not acces IFilter object, invalid file");

                        case NativeMethods.IFilterReturnCode.FILTER_E_TOO_BIG:
                            throw new IFFileToLarge("The file is to large to filter");

                        case NativeMethods.IFilterReturnCode.FILTER_E_END_OF_CHUNKS:
                            _done = true;
                            _endOfChunks = true;
                            break;
                    }

                    // If the read chunk isn't valid then continue
                    if (!_chunkValid) continue;
                }

                var textLength = (uint)(count - charsRead);
                if (textLength < 8192)
                    textLength = 8192;

                var textBuffer = new char[textLength + 1];
                var textRead = false;

                switch (_chunk.flags)
                {
                    case NativeMethods.CHUNKSTATE.CHUNK_FILTER_OWNED_VALUE:
                        // No support for filter owned values so this chunk is always invalid
                        _chunkValid = false;
                        continue;

                    case NativeMethods.CHUNKSTATE.CHUNK_VALUE:

                        var pvValue = new NativeMethods.PROPVARIANT();

                        // To convert from our C# PropVariant to the interop IntPtr:
                        var valuePtr = Marshal.AllocHGlobal(Marshal.SizeOf(pvValue));
                        Marshal.StructureToPtr(pvValue, valuePtr, false);

                        try
                        {
                            var valueResult = _filter.GetValue(ref valuePtr);

                            CheckResult(valueResult);

                            switch (valueResult)
                            {
                                case NativeMethods.IFilterReturnCode.FILTER_E_NO_MORE_VALUES:
                                case NativeMethods.IFilterReturnCode.FILTER_E_NO_VALUES:
                                    _chunkValid = false;
                                    break;

                                case NativeMethods.IFilterReturnCode.S_OK:
                                case NativeMethods.IFilterReturnCode.FILTER_S_LAST_VALUES:
                                    var temp = GetPropertyNameAndValue(valuePtr);
                                    if (!string.IsNullOrEmpty(temp))
                                    {
                                        textBuffer = temp.ToCharArray();
                                        textLength = (uint)textBuffer.Length;
                                        textRead = true;
                                    }

                                    _chunkValid = false;
                                    break;
                            }
                        }
                        finally
                        {
                            if (valuePtr != IntPtr.Zero)
                                Marshal.FreeHGlobal(valuePtr);
                        }

                        break;

                    case NativeMethods.CHUNKSTATE.CHUNK_TEXT:

                        var textResult = _filter.GetText(ref textLength, textBuffer);
                        CheckResult(textResult);

                        switch (textResult)
                        {
                            case NativeMethods.IFilterReturnCode.FILTER_E_EMBEDDING_UNAVAILABLE:
                            case NativeMethods.IFilterReturnCode.FILTER_E_LINK_UNAVAILABLE:
                            case NativeMethods.IFilterReturnCode.FILTER_E_NO_MORE_TEXT:
                                _chunkValid = false;
                                break;

                            case NativeMethods.IFilterReturnCode.S_OK:
                            case NativeMethods.IFilterReturnCode.FILTER_S_LAST_TEXT:

                                textRead = true;

                                // Remove junk from the buffer
                                if (_options.DoCleanUpCharacters)
                                    CleanUpCharacters(textLength, textBuffer);

                                // Check the break type
                                switch (_chunk.breakType)
                                {
                                    case NativeMethods.CHUNK_BREAKTYPE.CHUNK_NO_BREAK:
                                        breakChar = string.Empty;
                                        break;

                                    case NativeMethods.CHUNK_BREAKTYPE.CHUNK_EOW:
                                        breakChar = _options.WordBreakSeparator;
                                        break;

                                    case NativeMethods.CHUNK_BREAKTYPE.CHUNK_EOC:
                                    case NativeMethods.CHUNK_BREAKTYPE.CHUNK_EOP:
                                    case NativeMethods.CHUNK_BREAKTYPE.CHUNK_EOS:
                                        breakChar = "\n";
                                        break;
                                }

                                if (textResult == NativeMethods.IFilterReturnCode.FILTER_S_LAST_TEXT)
                                    _chunkValid = false;

                                break;
                        }

                        break;
                }

                if (textRead)
                {
                    if (_chunkAttributesGuidPropSetOfLastText != _chunk.attribute.guidPropSet)
                    {
                        _chunkAttributesGuidPropSetOfLastText = _chunk.attribute.guidPropSet;

                        if (breakChar == String.Empty)
                            breakChar = _options.ChunkTypeSeparator;
                    }

                    var read = (int)textLength;
                    int breakCharLength;

                    if (breakChar != string.Empty)
                        breakCharLength = 1;
                    else
                        breakCharLength = 0;

                    if (read + charsRead + breakCharLength > count)
                    {
                        var charsLeft = read + charsRead + breakCharLength - count;
                        _charsLeftFromLastRead = new char[charsLeft];
                        Array.Copy(textBuffer, read - charsLeft, _charsLeftFromLastRead, 0, charsLeft);
                        read -= charsLeft;
                    }
                    else
                        _charsLeftFromLastRead = null;

                    if (breakChar != string.Empty)
                    {
                        Array.Copy(breakChar.ToCharArray(), 0, buffer, index + charsRead, 1);
                        Array.Copy(textBuffer, 0, buffer, index + charsRead + 1, read);
                        read += 1;
                    }
                    else
                        Array.Copy(textBuffer, 0, buffer, index + charsRead, read);

                    charsRead += read;
                }

                if (Timeout())
                    break;
            }

            return charsRead;
        }
        #endregion

        #region ReadBlock
        /// <summary>
        /// Reads a specified maximum number of characters from the current reader and writes the data to a buffer, 
        /// beginning at the specified index
        /// </summary>
        /// <param name="buffer">When this method returns, contains the specified character array with the values 
        /// between index and (index + count - 1) replaced by the characters read from the current source. </param>
        /// <param name="index">The position in buffer at which to begin writing. </param>
        /// <param name="count">The maximum number of characters to read. If the end of the reader is reached before 
        /// the specified number of characters is read into the buffer, the method returns. </param>
        /// <returns>The number of characters that have been read. The number will be less than or equal to count, 
        /// depending on whether the data is available within the reader.  This method returns 0 (zero) if it is called
        /// when no more characters are left to read</returns>
        /// <exception cref="IFFileIsPasswordProtected">Raised when a file is password protected</exception>
        /// <exception cref="IFAccessFailure">Raised when the <see cref="_fileName"/> or IFilter file can not be accessed</exception>
        /// <exception cref="OutOfMemoryException">Raised when there is not enough memory to process the file</exception>
        /// <exception cref="IFUnknownFormat">Raised when the <see cref="_fileName"/> is not in the format the IFilter expect it to be 
        /// <exception cref="IFOldFilterFormat">Raised when an old <see cref="NativeMethods.IFilter"/> format is used and no filename is supplied</exception>
        /// (e.g. files with a wrong extension)</exception>
        public override int ReadBlock(char[] buffer, int index, int count)
        {
            return Read(buffer, index, count);
        }
        #endregion

        #region ReadToEnd
        /// <summary>
        /// Reads all characters from the current position to the end of the text reader and returns them as one string.
        /// </summary>
        /// <returns>A string that contains all characters from the current position to the end of the text reader.</returns>
        /// <exception cref="IFFileIsPasswordProtected">Raised when a file is password protected</exception>
        /// <exception cref="IFAccessFailure">Raised when the <see cref="_fileName"/> or IFilter file can not be accessed</exception>
        /// <exception cref="OutOfMemoryException">Raised when there is not enough memory to process the file</exception>
        /// <exception cref="IFUnknownFormat">Raised when the <see cref="_fileName"/> is not in the format the IFilter expect it to be 
        /// <exception cref="IFOldFilterFormat">Raised when an old <see cref="NativeMethods.IFilter"/> format is used and no filename is supplied</exception>
        /// (e.g. files with a wrong extension)</exception>
        public override string ReadToEnd()
        {
            var stringBuilder = new StringBuilder();
            var line = ReadLine();

            while (line != null)
            {
                stringBuilder.AppendLine(line);
                if (Timeout())
                    return stringBuilder.ToString();
                line = ReadLine();
            }

            return stringBuilder.ToString();
        }
        #endregion

        #region CheckResult
        /// <summary>
        /// Validates the <paramref name="result"/> return by the <see cref="NativeMethods.IFilter.GetText"/>
        /// or <see cref="NativeMethods.IFilter.GetValue"/> method
        /// </summary>
        /// <param name="result">The <see cref="NativeMethods.IFilterReturnCode"/></param>
        private void CheckResult(NativeMethods.IFilterReturnCode result)
        {
            switch (result)
            {
                case NativeMethods.IFilterReturnCode.FILTER_E_PASSWORD:
                    throw new IFFileIsPasswordProtected($@"The file '{_fileName}' or a file inside this file (e.g. in the case of a ZIP) is password protected");

                case NativeMethods.IFilterReturnCode.E_ACCESSDENIED:
                case NativeMethods.IFilterReturnCode.FILTER_E_ACCESS:
                    throw new IFAccessFailure("Unable to acces the IFilter or file");

                case NativeMethods.IFilterReturnCode.E_OUTOFMEMORY:
                    throw new OutOfMemoryException($@"Not enough memory to proceed reading the file '{_fileName}'");

                case NativeMethods.IFilterReturnCode.FILTER_E_UNKNOWNFORMAT:
                    throw new IFUnknownFormat($@"The file '{_fileName}' is not in the format the IFilter would expect it to be");
            }
        }
        #endregion

        #region GetPropertyNameAndValue
        /// <summary>
        /// This method will handle the <see cref="IntPtr">pointer</see> that has been returned by the
        /// <see cref="NativeMethods.IFilter.GetValue"/> method. It tries to return a string with the name 
        /// of the property and it's value (e.g. Titel : this is a title)
        /// </summary>
        /// <param name="valuePtr">The <see cref="IntPtr"/></param>
        /// <returns>Name and value of the property or null when the property is empty</returns>
        private string GetPropertyNameAndValue(IntPtr valuePtr)
        {
            var propertyVariant = (NativeMethods.PROPVARIANT)
                Marshal.PtrToStructure(valuePtr, typeof(NativeMethods.PROPVARIANT));

            try
            {
                if (string.IsNullOrWhiteSpace(propertyVariant.Value.ToString()))
                    return null;

                // Read the string property
                if (_chunk.attribute.psProperty.ulKind == NativeMethods.PROPSPECKIND.PRSPEC_LPWSTR)
                {
                    var propertyNameString = Marshal.PtrToStringUni(_chunk.attribute.psProperty.data);
                    return GetMetaDataProperty(propertyNameString, propertyVariant.Value);
                }
                else
                {
                    var property = PropertyMapper.GetProperty(_chunk.attribute.guidPropSet,
                        (long)_chunk.attribute.psProperty.data);

                    if (!string.IsNullOrEmpty(property))
                        return GetMetaDataProperty(property, propertyVariant.Value);

                    // Reader the property guid and id
                    var propertyKey = new NativeMethods.PROPERTYKEY
                    {
                        fmtid = new Guid(_chunk.attribute.guidPropSet.ToString()),
                        pid = (long)_chunk.attribute.psProperty.data
                    };

                    var result = NativeMethods.PSGetNameFromPropertyKey(ref propertyKey, out var propertyName);
                    if (result == 0)
                    {
                        UnmappedPropertyEvent?.Invoke(this,
                            new UnmappedPropertyEventArgs(_chunk.attribute.guidPropSet,
                                _chunk.attribute.psProperty.data.ToString(), propertyName,
                                propertyVariant.Value.ToString()));

                        return GetMetaDataProperty(propertyName, propertyVariant.Value);
                    }
                    else
                    {
                        UnmappedPropertyEvent?.Invoke(this,
                            new UnmappedPropertyEventArgs(_chunk.attribute.guidPropSet,
                                _chunk.attribute.psProperty.data.ToString(), null, propertyVariant.Value.ToString()));

                        return GetMetaDataProperty(_chunk.attribute.guidPropSet + "/" + _chunk.attribute.psProperty.data,
                            propertyVariant.Value);
                    }
                }
            }
            finally
            {
                propertyVariant.Clear();
            }
        }
        #endregion

        #region GetMetaDataProperty
        /// <summary>
        /// Adds metadata to dictionary and outputs name/value string
        /// </summary>
        /// <param name="name">Metadata name</param>
        /// <param name="value">Metadata value</param>
        /// <returns>Name and value of the property or null if IncludeProperties option is false</returns>
        private string GetMetaDataProperty(string name, object value)
        {
            if (MetaDataProperties.ContainsKey(name))
                MetaDataProperties[name].Add(value);
            else
                MetaDataProperties.Add(name, new List<object> { value });

            return _options.IncludeProperties ? $"{name} : {value}\n" : null;
        }
        #endregion

        #region CleanUpCharacters
        /// <summary>
        /// Remove junk from the <paramref name="buffer"/>
        /// </summary>
        /// <param name="length">The length of the buffer</param>
        /// <param name="buffer">The bufer</param>
        private static void CleanUpCharacters(uint length, IList<char> buffer)
        {
            for (var i = 0; i < length; i++)
            {
                var ch = buffer[i];
                int chi = ch;

                switch (chi)
                {
                    case 0: // embedded null
                        buffer[i] = '\n';
                        break;

                    case 0x2000: // en quad
                    case 0x2001: // em quad
                    case 0x2002: // en space
                    case 0x2003: // em space
                    case 0x2004: // three-per-em space
                    case 0x2005: // four-per-em space
                    case 0x2006: // six-per-em space
                    case 0x2007: // figure space
                    case 0x2008: // puctuation space
                    case 0x2009: // thin space
                    case 0x200A: // hair space
                    case 0x200B: // zero-width space
                    case 0x200C: // zero-width non-joiner
                    case 0x200D: // zero-width joiner
                    case 0x202f: // no-break space
                    case 0x3000: // ideographic space
                        buffer[i] = ' ';
                        break;

                    case 0x000C: // page break char
                    case 0x00A0: // non breaking space
                    case 0x00B6: // pilcro
                    case 0x2028: // line seperator
                    case 0x2029: // paragraph seperator
                        buffer[i] = '\n';
                        break;

                    case 0x00AD: // soft-hyphen
                    case 0x00B7: // middle dot
                    case 0x2010: // hyphen
                    case 0x2011: // non-breaking hyphen
                    case 0x2012: // figure dash
                    case 0x2013: // en dash
                    case 0x2014: // em dash
                    case 0x2015: // quote dash
                    case 0x2027: // hyphenation point
                    case 0x2043: // hyphen bullet
                    case 0x208B: // subscript minus
                    case 0xFE31: // vertical em dash
                    case 0xFE32: // vertical en dash
                    case 0xFE58: // small em dash
                    case 0xFE63: // small hyphen minus
                        buffer[i] = '-';
                        break;

                    case 0x00B0: // degree
                    case 0x2018: // left single quote
                    case 0x2019: // right single quote
                    case 0x201A: // low right single quote
                    case 0x201B: // high left single quote
                    case 0x2032: // prime
                    case 0x2035: // reversed prime
                    case 0x2039: // left-pointing angle quotation mark
                    case 0x203A: // right-pointing angle quotation mark
                        buffer[i] = '\'';
                        break;

                    case 0x201C: // left double quote
                    case 0x201D: // right double quote
                    case 0x201E: // low right double quote
                    case 0x201F: // high left double quote
                    case 0x2033: // double prime
                    case 0x2034: // triple prime
                    case 0x2036: // reversed double prime
                    case 0x2037: // reversed triple prime
                    case 0x00AB: // left-pointing double angle quotation mark
                    case 0x00BB: // right-pointing double angle quotation mark
                    case 0x3003: // ditto mark
                    case 0x301D: // reversed double prime quotation mark
                    case 0x301E: // double prime quotation mark
                    case 0x301F: // low double prime quotation mark
                        buffer[i] = '\"';
                        break;

                    case 0x00A7: // section-sign
                    case 0x2020: // dagger
                    case 0x2021: // double-dagger
                    case 0x2022: // bullet
                    case 0x2023: // triangle bullet
                    case 0x203B: // reference mark
                    case 0xFE55: // small colon
                        buffer[i] = ':';
                        break;

                    case 0x2024: // one dot leader
                    case 0x2025: // two dot leader
                    case 0x2026: // elipsis
                    case 0x3002: // ideographic full stop
                    case 0xFE30: // two dot vertical leader
                    case 0xFE52: // small full stop
                        buffer[i] = '.';
                        break;

                    case 0x3001: // ideographic comma
                    case 0xFE50: // small comma
                    case 0xFE51: // small ideographic comma
                        buffer[i] = ',';
                        break;

                    case 0xFE54: // small semicolon
                        buffer[i] = ';';
                        break;

                    case 0x00A6: // broken-bar
                    case 0x2016: // double vertical line
                        buffer[i] = '|';
                        break;

                    case 0x2017: // double low line
                    case 0x203E: // overline
                    case 0x203F: // undertie
                    case 0x2040: // character tie
                    case 0xFE33: // vertical low line
                    case 0xFE49: // dashed overline
                    case 0xFE4A: // centerline overline
                    case 0xFE4D: // dashed low line
                    case 0xFE4E: // centerline low line
                        buffer[i] = '_';
                        break;

                    case 0x301C: // wave dash
                    case 0x3030: // wavy dash
                    case 0xFE34: // vertical wavy low line
                    case 0xFE4B: // wavy overline
                    case 0xFE4C: // double wavy overline
                    case 0xFE4F: // wavy low line
                        buffer[i] = '~';
                        break;

                    case 0x2038: // caret
                    case 0x2041: // caret insertion point
                        buffer[i] = ' ';
                        break;

                    case 0x2030: // per-mille
                    case 0x2031: // per-ten thousand
                    case 0xFE6A: // small per-cent
                        buffer[i] = '%';
                        break;

                    case 0xFE6B: // small commercial at
                        buffer[i] = '@';
                        break;

                    case 0x00A9: // copyright
                        buffer[i] = 'c';
                        break;

                    case 0x00B5: // micro
                        buffer[i] = 'u';
                        break;

                    case 0x00AE: // registered
                        buffer[i] = 'r';
                        break;

                    case 0x207A: // superscript plus
                    case 0x208A: // subscript plus
                    case 0xFE62: // small plus
                        buffer[i] = '+';
                        break;

                    case 0x2044: // fraction slash
                        buffer[i] = '/';
                        break;

                    case 0x2042: // asterism
                    case 0xFE61: // small asterisk
                        buffer[i] = '*';
                        break;
                    case 0x208C: // subscript equal
                    case 0xFE66: // small equal
                        buffer[i] = '=';
                        break;
                    case 0xFE68: // small reverse solidus
                        buffer[i] = '\\';
                        break;

                    case 0xFE5F: // small number sign
                        buffer[i] = '#';
                        break;

                    case 0xFE60: // small ampersand
                        buffer[i] = '&';
                        break;

                    case 0xFE69: // small dollar sign
                        buffer[i] = '$';
                        break;

                    case 0x2045: // left square bracket with quill
                    case 0x3010: // left black lenticular bracket
                    case 0x3016: // left white lenticular bracket
                    case 0x301A: // left white square bracket
                    case 0xFE3B: // vertical left lenticular bracket
                    case 0xFF41: // vertical left corner bracket
                    case 0xFF43: // vertical white left corner bracket
                        buffer[i] = '[';
                        break;

                    case 0x2046: // right square bracket with quill
                    case 0x3011: // right black lenticular bracket
                    case 0x3017: // right white lenticular bracket
                    case 0x301B: // right white square bracket
                    case 0xFE3C: // vertical right lenticular bracket
                    case 0xFF42: // vertical right corner bracket
                    case 0xFF44: // vertical white right corner bracket
                        buffer[i] = ']';
                        break;

                    case 0x208D: // subscript left parenthesis
                    case 0x3014: // left tortise-shell bracket
                    case 0x3018: // left white tortise-shell bracket
                    case 0xFE35: // vertical left parenthesis
                    case 0xFE39: // vertical left tortise-shell bracket
                    case 0xFE59: // small left parenthesis
                    case 0xFE5D: // small left tortise-shell bracket
                        buffer[i] = '(';
                        break;

                    case 0x208E: // subscript right parenthesis
                    case 0x3015: // right tortise-shell bracket
                    case 0x3019: // right white tortise-shell bracket
                    case 0xFE36: // vertical right parenthesis
                    case 0xFE3A: // vertical right tortise-shell bracket
                    case 0xFE5A: // small right parenthesis
                    case 0xFE5E: // small right tortise-shell bracket
                        buffer[i] = ')';
                        break;

                    case 0x3008: // left angle bracket
                    case 0x300A: // left double angle bracket
                    case 0xFF3D: // vertical left double angle bracket
                    case 0xFF3F: // vertical left angle bracket
                    case 0xFF64: // small less-than
                        buffer[i] = '<';
                        break;

                    case 0x3009: // right angle bracket
                    case 0x300B: // right double angle bracket
                    case 0xFF3E: // vertical right double angle bracket
                    case 0xFF40: // vertical right angle bracket
                    case 0xFF65: // small greater-than
                        buffer[i] = '>';
                        break;

                    case 0xFE37: // vertical left curly bracket
                    case 0xFE5B: // small left curly bracket
                        buffer[i] = '{';
                        break;

                    case 0xFE38: // vertical right curly bracket
                    case 0xFE5C: // small right curly bracket
                        buffer[i] = '}';
                        break;

                    case 0x00A1: // inverted exclamation mark
                    case 0x00AC: // not
                    case 0x203C: // double exclamation mark
                    case 0x203D: // interrobang
                    case 0xFE57: // small exclamation mark
                        buffer[i] = '!';
                        break;

                    case 0x00BF: // inverted question mark
                    case 0xFE56: // small question mark
                        buffer[i] = '?';
                        break;

                    case 0x00B9: // superscript one
                        buffer[i] = '1';
                        break;

                    case 0x00B2: // superscript two
                        buffer[i] = '2';
                        break;

                    case 0x00B3: // superscript three
                        buffer[i] = '3';
                        break;

                    case 0x2070: // superscript zero
                    case 0x2074: // superscript four
                    case 0x2075: // superscript five
                    case 0x2076: // superscript six
                    case 0x2077: // superscript seven
                    case 0x2078: // superscript eight
                    case 0x2079: // superscript nine
                    case 0x2080: // subscript zero
                    case 0x2081: // subscript one
                    case 0x2082: // subscript two
                    case 0x2083: // subscript three
                    case 0x2084: // subscript four
                    case 0x2085: // subscript five
                    case 0x2086: // subscript six
                    case 0x2087: // subscript seven
                    case 0x2088: // subscript eight
                    case 0x2089: // subscript nine
                    case 0x3021: // Hangzhou numeral one
                    case 0x3022: // Hangzhou numeral two
                    case 0x3023: // Hangzhou numeral three
                    case 0x3024: // Hangzhou numeral four
                    case 0x3025: // Hangzhou numeral five
                    case 0x3026: // Hangzhou numeral six
                    case 0x3027: // Hangzhou numeral seven
                    case 0x3028: // Hangzhou numeral eight
                    case 0x3029: // Hangzhou numeral nine
                        chi = chi & 0x000F;
                        buffer[i] = Convert.ToChar(chi);
                        break;

                    // ONE is at ZERO location... careful
                    case 0x3220: // parenthesized ideograph one
                    case 0x3221: // parenthesized ideograph two
                    case 0x3222: // parenthesized ideograph three
                    case 0x3223: // parenthesized ideograph four
                    case 0x3224: // parenthesized ideograph five
                    case 0x3225: // parenthesized ideograph six
                    case 0x3226: // parenthesized ideograph seven
                    case 0x3227: // parenthesized ideograph eight
                    case 0x3228: // parenthesized ideograph nine
                    case 0x3280: // circled ideograph one
                    case 0x3281: // circled ideograph two
                    case 0x3282: // circled ideograph three
                    case 0x3283: // circled ideograph four
                    case 0x3284: // circled ideograph five
                    case 0x3285: // circled ideograph six
                    case 0x3286: // circled ideograph seven
                    case 0x3287: // circled ideograph eight
                    case 0x3288: // circled ideograph nine
                        chi = (chi & 0x000F) + 1;
                        buffer[i] = Convert.ToChar(chi);
                        break;

                    case 0x3007: // ideographic number zero
                    case 0x24EA: // circled number zero
                        buffer[i] = '0';
                        break;

                    default:
                        if (0xFF01 <= ch // fullwidth exclamation mark 
                            && ch <= 0xFF5E) // fullwidth tilde
                        {
                            // the fullwidths line up with ASCII low subset
                            buffer[i] = Convert.ToChar(chi & 0xFF00 + '!' - 1);
                            //ch = ch & 0xFF00 + '!' - 1;               
                        }
                        else if (0x2460 <= ch // circled one
                                 && ch <= 0x2468) // circled nine
                        {
                            buffer[i] = Convert.ToChar(chi - 0x2460 + '1');
                            //ch = ch - 0x2460 + '1';
                        }
                        else if (0x2474 <= ch // parenthesized one
                                 && ch <= 0x247C) // parenthesized nine
                        {
                            buffer[i] = Convert.ToChar(chi - 0x2474 + '1');
                            // ch = ch - 0x2474 + '1';
                        }
                        else if (0x2488 <= ch // one full stop
                                 && ch <= 0x2490) // nine full stop
                        {
                            buffer[i] = Convert.ToChar(chi - 0x2488 + '1');
                            //ch = ch - 0x2488 + '1';
                        }
                        else if (0x249C <= ch // parenthesized small a
                                 && ch <= 0x24B5) // parenthesized small z
                        {
                            buffer[i] = Convert.ToChar(chi - 0x249C + 'a');
                            //ch = ch - 0x249C + 'a';
                        }
                        else if (0x24B6 <= ch // circled capital A
                                 && ch <= 0x24CF) // circled capital Z
                        {
                            buffer[i] = Convert.ToChar(chi - 0x24B6 + 'A');
                            //ch = ch - 0x24B6 + 'A';
                        }
                        else if (0x24D0 <= ch // circled small a
                                 && ch <= 0x24E9) // circled small z
                        {
                            buffer[i] = Convert.ToChar(chi - 0x24D0 + 'a');
                            //ch = ch - 0x24D0 + 'a';
                        }
                        else if (0x2500 <= ch // box drawing (begin)
                                 && ch <= 0x257F) // box drawing (end)
                        {
                            buffer[i] = '|';
                        }
                        else if (0x2580 <= ch // block elements (begin)
                                 && ch <= 0x259F) // block elements (end)
                        {
                            buffer[i] = '#';
                        }
                        else if (0x25A0 <= ch // geometric shapes (begin)
                                 && ch <= 0x25FF) // geometric shapes (end)
                        {
                            buffer[i] = '*';
                        }
                        else if (0x2600 <= ch // dingbats (begin)
                                 && ch <= 0x267F) // dingbats (end)
                        {
                            buffer[i] = '.';
                        }
                        break;
                }
            }
        }
        #endregion

        #region Close
        /// <summary>
        /// Closes this textreader
        /// </summary>
        public override void Close()
        {
            Dispose(true);
        }
        #endregion

        #region Not implemented methods
        /// <summary>
        /// This method is not supported and will aways throw an <see cref="NotSupportedException"/>
        /// </summary>
        /// <returns></returns>
        public override int Peek()
        {
            throw new NotSupportedException();
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Disposes this object
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (_filter != null)
                Marshal.ReleaseComObject(_filter);

            try
            {
                if (_chunkBuffer != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(_chunkBuffer);
                    _chunkBuffer = IntPtr.Zero;
                }
            }
            catch
            {
                // Ignore
            }

            _fileStream?.Dispose();

            _stopwatch = null;

            base.Dispose(true);

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}

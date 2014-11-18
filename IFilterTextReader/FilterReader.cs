using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using IFilterTextReader.Exceptions;

namespace IFilterTextReader
{
    /// <summary>
    /// This class implements a TextReader that reads from an IFilter 
    /// </summary>
    public class FilterReader : TextReader
    {
        #region Fields
        /// <summary>
        /// The IFilter interface
        /// </summary>
        private readonly NativeMethods.IFilter _filter;

        /// <summary>
        /// Contains the chunk that we are reading from the <see cref="NativeMethods.IFilter"/> interface
        /// </summary>
        private NativeMethods.STAT_CHUNK _currentChunk;

        /// <summary>
        /// Indicates that we are done with reading
        /// </summary>
        private bool _done;

        /// <summary>
        /// Indicates if the current chunk is valid
        /// </summary>
        private bool _currentChunkValid;

        /// <summary>
        /// Holds the chars that are left from the last chunck read
        /// </summary>
        private char[] _charsLeftFromLastRead;

        /// <summary>
        /// The file to read with an IFilter
        /// </summary>
        private readonly string _fileName;

        private bool _carriageReturnFound;
        #endregion

        #region Constructor en Destructor
        /// <summary>
        /// Creates an TextReader object for the given <see cref="fileName"/>
        /// </summary>
        /// <param name="fileName"></param>
        public FilterReader(string fileName)
        {
            _fileName = fileName;
            _filter = FilterLoader.LoadAndInitIFilter(fileName);
            if (_filter == null)
                throw new IFFilterNotFound("There is no IFilter installed for the file '" + Path.GetFileName(fileName) + "'");
        }
        
        ~FilterReader()
        {
            Dispose(false);
        }

        #endregion

        #region ReadLine
        /// <summary>
        ///  Reads a line of characters from the text reader and returns the data as a string.
        /// </summary>
        /// <returns>The next line from the reader, or null if all characters have been read.</returns>
        public override string ReadLine()
        {
            var buffer = new char[1024];
            var stringBuilder = new StringBuilder();
            var done = false;

            while (!done)
            {
                var charsRead = Read(buffer, 0, 1024);

                if (charsRead == 0)
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
        public override int Read()
        {
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
        public override int Read(char[] buffer, int index, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (buffer.Length - index < count)
                throw new ArgumentException("The buffer is to small");

            if (index < 0)
                throw new ArgumentOutOfRangeException("index");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            var charsRead = 0;

            while (!_done && charsRead < count)
            {
                if (_charsLeftFromLastRead != null)
                {
                    var charsToCopy = (_charsLeftFromLastRead.Length < count - charsRead)
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

                    continue;
                }

                if (!_currentChunkValid)
                {
                    var result = _filter.GetChunk(out _currentChunk);
                    _currentChunkValid = (result == NativeMethods.IFilterReturnCode.S_OK) &&
                                         ((_currentChunk.flags & NativeMethods.CHUNKSTATE.CHUNK_TEXT) != 0);

                    if (result == NativeMethods.IFilterReturnCode.FILTER_E_END_OF_CHUNKS)
                        _done = true;
                }

                if (_currentChunkValid)
                {
                    var bufLength = (uint)(count - charsRead);
                    if (bufLength < 8192)
                        bufLength = 8192;

                    var getTextBuf = new char[bufLength + 2];
                    var result = _filter.GetText(ref bufLength, getTextBuf);

                    switch (result)
                    {
                        case NativeMethods.IFilterReturnCode.FILTER_E_PASSWORD:
                            throw new IFFileIsPasswordProtected("The file '" + _fileName +
                                                                "' or a file inside this file (e.g. in the case of a ZIP) is password protected");

                        case NativeMethods.IFilterReturnCode.E_ACCESSDENIED:
                        case NativeMethods.IFilterReturnCode.FILTER_E_ACCESS:
                            throw new IFAccesFailure("Unable to acces the IFilter or file");

                        case NativeMethods.IFilterReturnCode.FILTER_E_EMBEDDING_UNAVAILABLE:
                        case NativeMethods.IFilterReturnCode.FILTER_E_LINK_UNAVAILABLE:
                            continue;

                        case NativeMethods.IFilterReturnCode.E_OUTOFMEMORY:
                            throw new OutOfMemoryException("Not enough memory to proceed reading the file '" + _fileName +
                                                           "'");

                        case NativeMethods.IFilterReturnCode.FILTER_E_UNKNOWNFORMAT:
                            throw new IFUnknownFormat("The file '" + _fileName +
                                                      "' is not in the format the IFilter would expect it to be");

                        case NativeMethods.IFilterReturnCode.FILTER_S_LAST_TEXT:
                        case NativeMethods.IFilterReturnCode.S_OK:
                            // Remove any null terminated chars
                            for (var i = bufLength - 1; i > 0; i--)
                            {
                                switch (getTextBuf[i])
                                {
                                    case '\0':
                                        bufLength--;
                                        break;
                                }
                            }

                            switch (_currentChunk.breakType)
                            {
                                case NativeMethods.CHUNK_BREAKTYPE.CHUNK_NO_BREAK:
                                    break;

                                case NativeMethods.CHUNK_BREAKTYPE.CHUNK_EOW:
                                    break;

                                case NativeMethods.CHUNK_BREAKTYPE.CHUNK_EOC:
                                case NativeMethods.CHUNK_BREAKTYPE.CHUNK_EOP:
                                case NativeMethods.CHUNK_BREAKTYPE.CHUNK_EOS:
                                    if (getTextBuf[bufLength - 1] != ' ')
                                    {
                                        getTextBuf[bufLength] = ' ';
                                        bufLength += 1;
                                    }
                                    break;
                            }

                            var read = (int)bufLength;
                            if (read + charsRead > count)
                            {
                                var charsLeft = (read + charsRead - count);
                                _charsLeftFromLastRead = new char[charsLeft];
                                Array.Copy(getTextBuf, read - charsLeft, _charsLeftFromLastRead, 0, charsLeft);
                                read -= charsLeft;
                            }
                            else
                                _charsLeftFromLastRead = null;

                            Array.Copy(getTextBuf, 0, buffer, index + charsRead, read);
                            charsRead += read;
                            break;
                    }

                    if (result == NativeMethods.IFilterReturnCode.FILTER_S_LAST_TEXT ||
                        result == NativeMethods.IFilterReturnCode.FILTER_E_NO_MORE_TEXT)
                        _currentChunkValid = false;
                }
            }

            return charsRead;
        }
        #endregion

        #region Close
        public override void Close()
        {
            Dispose(true);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Disposes this object
        /// </summary>
        /// <param name="disposing"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
        protected override void Dispose(bool disposing)
        {
            if (_filter != null)
                Marshal.ReleaseComObject(_filter);
            
            FilterLoader.Dispose();

            base.Dispose(true);

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}

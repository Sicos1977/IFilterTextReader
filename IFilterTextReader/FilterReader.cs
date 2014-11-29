using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using IFilterTextReader.Exceptions;

/*
   Copyright 2013-2014 Kees van Spelde

   Licensed under The Code Project Open License (CPOL) 1.02;
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.codeproject.com/info/cpol10.aspx

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

namespace IFilterTextReader
{
    /// <summary>
    /// This class is a <see cref="TextReader"/> wrapper around an IFilter. This way a file can be processed 
    /// like if it is a dead normal text file
    /// </summary>
    /// <exception cref="IFFileIsPasswordProtected">Raised when a file is password protected</exception>
    /// <exception cref="IFAccesFailure">Raised when the <see cref="_fileName"/> or IFilter file can not be accessed</exception>
    /// <exception cref="OutOfMemoryException">Raised when there is not enough memory to process the file</exception>
    /// <exception cref="IFUnknownFormat">Raised when the <see cref="_fileName"/> is not in the format the IFilter expect it to be 
    /// <exception cref="IFOldFilterFormat">Raised when an old <see cref="NativeMethods.IFilter"/> format is used and no filename is supplied</exception>
    /// (e.g. files with a wrong extension)</exception>
    public class FilterReader : TextReader
    {
        #region Delegate
        /// <summary>
        /// Raised when an unmapped property has been retrieved with the <see cref="NativeMethods.IFilter.GetValue"/> method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"><see cref="UnmappedPropertyEventArgs"/></param>
        public delegate void UnmappedPropertyEventHandler(Object sender, UnmappedPropertyEventArgs e);
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
        /// Indicates if the <see cref="_chunk"/> is valid
        /// </summary>
        private bool _chunkValid;
        
        /// <summary>
        /// Indicates that we are done with reading <see cref="_chunk">chunks</see>
        /// </summary>
        private bool _done;

        /// <summary>
        /// Holds the chars that are left from the last chunck read
        /// </summary>
        private char[] _charsLeftFromLastRead;

        /// <summary>
        /// When set to true all available properties will also be read with the <see cref="NativeMethods.IFilter.GetValue"/> method
        /// </summary>
        private readonly bool _includeProperties;
        
        /// <summary>
        /// Indicates when true that a carriage return was found on the previous line
        /// </summary>
        private bool _carriageReturnFound;

        /// <summary>
        /// Contains a list with <see cref="MetadataProperty"/>
        /// </summary>
        private static List<MetadataProperty> _metadataProperties; 
        #endregion

        #region Constructor en Destructor
        /// <summary>
        /// Creates an TextReader object for the given <see cref="fileName"/>
        /// </summary>
        /// <param name="fileName">The file to read</param>
        /// <param name="extension">Overrides the file extension</param>
        /// <param name="includeProperties">Set to true to also read any available 
        /// properties (e.g summary properties in a Word document)</param>
        public FilterReader(string fileName, 
                            string extension = "", 
                            bool includeProperties = false)
        {
            _fileName = fileName;
            _fileStream = File.OpenRead(fileName);

            if (string.IsNullOrWhiteSpace(extension))
                extension = Path.GetExtension(fileName);

            _filter = FilterLoader.LoadAndInitIFilter(_fileStream, extension, fileName);

            if (_filter == null)
            {
                if (string.IsNullOrWhiteSpace(extension))
                    throw new IFFilterNotFound("There is no IFilter installed for the file '" + Path.GetFileName(fileName) + "'");

                throw new IFFilterNotFound("There is no IFilter installed for the extension '" + extension + "'");
            }

            _includeProperties = includeProperties;
        }

        /// <summary>
        /// Creates an TextReader object for the given <see cref="Stream"/>
        /// </summary>
        /// <param name="stream">The file stream to read</param>
        /// <param name="extension">The extension for the <see cref="stream"/></param>
        /// <param name="includeProperties">Set to true to also read any available 
        /// properties (e.g summary properties in a Word document)</param>
        /// <exception cref="ArgumentException">Raised when the <see cref="extension"/> argument is null or empty</exception>
        public FilterReader(Stream stream,
                            string extension,
                            bool includeProperties = false)
        {
            if (string.IsNullOrWhiteSpace(extension))
                throw new ArgumentException("The extension cannot be empty", "extension");

            _filter = FilterLoader.LoadAndInitIFilter(stream, extension);

            if (_filter == null)
                throw new IFFilterNotFound("There is no IFilter installed for the stream with the extension '" + extension + "'");

            _includeProperties = includeProperties;
        }

        ~FilterReader()
        {
            Dispose(false);
        }
        #endregion

        #region ReadLine
        /// <summary>
        /// Reads a line of characters from the text reader and returns the data as a string.
        /// </summary>
        /// <returns>The next line from the reader, or null if all characters have been read.</returns>
        public override string ReadLine()
        {
            var buffer = new char[2048];
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

                // If we don't have a valid chunck anymore then read a new chunck
                if (!_chunkValid)
                {
                    var result = _filter.GetChunk(out _chunk);
                    _chunkValid = result == NativeMethods.IFilterReturnCode.S_OK;
                    if (result == NativeMethods.IFilterReturnCode.FILTER_E_END_OF_CHUNKS)
                        _done = true;

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

                        if (!_includeProperties)
                        {
                            _chunkValid = false;
                            continue;
                        }

                        var valuePtr = IntPtr.Zero;
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
                                    textLength = (uint) textBuffer.Length;
                                    textRead = true;
                                }
    
                                _chunkValid = false;
                                break;
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
                                CleanUpCharacters(textLength, textBuffer);

                                // Check the break type
                                switch (_chunk.breakType)
                                {
                                    case NativeMethods.CHUNK_BREAKTYPE.CHUNK_NO_BREAK:
                                        break;

                                    case NativeMethods.CHUNK_BREAKTYPE.CHUNK_EOW:
                                        break;

                                    case NativeMethods.CHUNK_BREAKTYPE.CHUNK_EOC:
                                    case NativeMethods.CHUNK_BREAKTYPE.CHUNK_EOP:
                                    case NativeMethods.CHUNK_BREAKTYPE.CHUNK_EOS:
                                        if (textBuffer[textLength - 1] != ' ' && textBuffer[textLength - 1] != '\n')
                                        {
                                            textBuffer[textLength] = ' ';
                                            textLength += 1;
                                        }
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

                    var read = (int)textLength;
                    if (read + charsRead > count)
                    {
                        var charsLeft = (read + charsRead - count);
                        _charsLeftFromLastRead = new char[charsLeft];
                        Array.Copy(textBuffer, read - charsLeft, _charsLeftFromLastRead, 0, charsLeft);
                        read -= charsLeft;
                    }
                    else
                        _charsLeftFromLastRead = null;

                    Array.Copy(textBuffer, 0, buffer, index + charsRead, read);
                    charsRead += read;
                }
            }

            return charsRead;
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
                    throw new IFFileIsPasswordProtected("The file '" + _fileName +
                                                        "' or a file inside this file (e.g. in the case of a ZIP) is password protected");

                case NativeMethods.IFilterReturnCode.E_ACCESSDENIED:
                case NativeMethods.IFilterReturnCode.FILTER_E_ACCESS:
                    throw new IFAccesFailure("Unable to acces the IFilter or file");

                case NativeMethods.IFilterReturnCode.E_OUTOFMEMORY:
                    throw new OutOfMemoryException("Not enough memory to proceed reading the file '" +
                                                   _fileName +
                                                   "'");

                case NativeMethods.IFilterReturnCode.FILTER_E_UNKNOWNFORMAT:
                    throw new IFUnknownFormat("The file '" + _fileName +
                                              "' is not in the format the IFilter would expect it to be");
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

                if (_chunk.attribute.psProperty.ulKind == NativeMethods.PROPSPECKIND.PRSPEC_LPWSTR)
                {
                    var propertyNameString = Marshal.PtrToStringUni(_chunk.attribute.psProperty.data);
                    return propertyNameString + " : " + propertyVariant.Value + "\n";
                }
                else
                {
                    var propertyKey = new NativeMethods.PROPERTYKEY
                    {
                        fmtid = new Guid(_chunk.attribute.guidPropSet.ToString()),
                        pid = (long) _chunk.attribute.psProperty.data
                    };

                    string propertyName;
                    var result = NativeMethods.PSGetNameFromPropertyKey(ref propertyKey, out propertyName);
                    if (result == 0)
                        return propertyName + " : " + propertyVariant.Value + "\n";
                    else
                        return _chunk.attribute.guidPropSet + "/" + _chunk.attribute.psProperty.data + " : " +
                               propertyVariant.Value + "\n";
                }
            }
            finally
            {
                propertyVariant.Clear();
            }
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

            if (_fileStream != null)
                _fileStream.Dispose();

            base.Dispose(true);

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}

//
// EncodingDetector.cs
//
// Author: Kees van Spelde <sicos2002@hotmail.com>
//
// Copyright (c) 2013-2024 Magic-Sessions. (www.magic-sessions.com)
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
using System.IO;
using System.Text;

namespace IFilterTextReader.Helpers
{
    /// <summary>
    ///     Helper class to detect file encoding before passing to IFilter
    /// </summary>
    public static class EncodingDetector
    {
        /// <summary>
        ///     Detects the encoding of a text file by checking BOM and analyzing byte patterns
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>Detected encoding or UTF-8 as default</returns>
        public static Encoding DetectEncoding(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return DetectEncoding(stream);
            }
        }

        /// <summary>
        ///     Detects the encoding of a stream by checking BOM and analyzing byte patterns
        /// </summary>
        /// <param name="stream">Stream to analyze</param>
        /// <returns>Detected encoding or UTF-8 as default</returns>
        public static Encoding DetectEncoding(Stream stream)
        {
            var originalPosition = stream.Position;

            try
            {
                stream.Position = 0;
                var buffer = new byte[4];
                var bytesRead = stream.Read(buffer, 0, 4);

                if (bytesRead >= 2)
                {
                    // UTF-16 LE with BOM
                    if (buffer[0] == 0xFF && buffer[1] == 0xFE)
                        return Encoding.Unicode;

                    // UTF-16 BE with BOM
                    if (buffer[0] == 0xFE && buffer[1] == 0xFF)
                        return Encoding.BigEndianUnicode;

                    // UTF-8 with BOM
                    if (bytesRead >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
                        return Encoding.UTF8;
                }

                // No BOM found - analyze content
                stream.Position = 0;
                var sampleSize = (int)Math.Min(8192, stream.Length);
                var sample = new byte[sampleSize];
                bytesRead = stream.Read(sample, 0, sampleSize);

                // Check if valid UTF-8 (without BOM)
                if (IsValidUtf8(sample, bytesRead))
                    return new UTF8Encoding(false); // UTF-8 WITHOUT BOM

                // Check for Shift-JIS patterns
                if (ContainsShiftJis(sample, bytesRead))
                {
                    try
                    {
                        return Encoding.GetEncoding("shift_jis");
                    }
                    catch
                    {
                        try
                        {
                            return Encoding.GetEncoding(932);
                        }
                        catch
                        {
                            return new UTF8Encoding(false);
                        }
                    }
                }

                // Default to UTF-8 without BOM
                return new UTF8Encoding(false);
            }
            finally
            {
                stream.Position = originalPosition;
            }
        }

        /// <summary>
        ///     Checks if the byte array contains valid UTF-8 sequences
        /// </summary>
        private static bool IsValidUtf8(byte[] buffer, int length)
        {
            for (var i = 0; i < length; i++)
            {
                var b = buffer[i];

                if (b < 0x80) continue; // ASCII range - valid in UTF-8

                // Check multi-byte sequence
                int expectedBytes;
                if ((b & 0xE0) == 0xC0) expectedBytes = 1;      // 2-byte sequence
                else if ((b & 0xF0) == 0xE0) expectedBytes = 2; // 3-byte sequence
                else if ((b & 0xF8) == 0xF0) expectedBytes = 3; // 4-byte sequence
                else return false; // Invalid start byte

                // Verify continuation bytes
                for (var j = 0; j < expectedBytes; j++)
                {
                    if (++i >= length || (buffer[i] & 0xC0) != 0x80)
                        return false;
                }
            }

            // All bytes were either valid ASCII or valid UTF-8 multi-byte sequences
            return true;
        }

        /// <summary>
        ///     Checks if the byte array contains Shift-JIS character patterns
        /// </summary>
        private static bool ContainsShiftJis(byte[] buffer, int length)
        {
            for (var i = 0; i < length - 1; i++)
            {
                var b1 = buffer[i];
                var b2 = buffer[i + 1];

                // Shift-JIS first byte ranges
                if ((b1 >= 0x81 && b1 <= 0x9F) || (b1 >= 0xE0 && b1 <= 0xFC))
                {
                    // Second byte range
                    if ((b2 >= 0x40 && b2 <= 0x7E) || (b2 >= 0x80 && b2 <= 0xFC))
                        return true;
                }
            }

            return false;
        }
    }
}

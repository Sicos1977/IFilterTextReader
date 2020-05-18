//
// Extensions.cs
//
// Author: Kees van Spelde <sicos2002@hotmail.com>
//
// Copyright (c) 2013-2020 Magic-Sessions. (www.magic-sessions.com)
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

using System.Text;

namespace IFilterTextReader
{
    internal static class Extensions
    {
        #region AppendLineFormat
        /// <summary>
        /// Appends a formatted string and the default line terminator to to this StringBuilder instance. 
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <param name="format"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static StringBuilder AppendLineFormat(this StringBuilder stringBuilder, string format,
            params object[] arguments)
        {
            var value = string.Format(format, arguments);
            stringBuilder.AppendLine(value);
            return stringBuilder;
        }
        #endregion

        #region AppendIf
        /// <summary>
        /// Appends the <paramref name="value"/> when the <paramref name="condition"/> is <c>True</c>
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <param name="condition"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static StringBuilder AppendIf(this StringBuilder stringBuilder, bool condition, string value)
        {
            if (condition) stringBuilder.Append(value);
            return stringBuilder;
        }
        #endregion

        #region EndsWith
        /// <summary>
        /// Returns true when the <paramref name="stringBuilder"/> EndsWith <paramref name="text"/>
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool EndsWith(this StringBuilder stringBuilder, string text)
        {
            if (stringBuilder.Length < text.Length)
                return false;

            var length = stringBuilder.Length;
            var textLength = text.Length;
            for (var i = 1; i <= textLength; i++)
            {
                if (text[textLength - i] != stringBuilder[length - i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true when the <paramref name="stringBuilder"/> EndsWith <paramref name="chr"/>
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <param name="chr"></param>
        /// <returns></returns>
        public static bool EndsWith(this StringBuilder stringBuilder, char chr)
        {
            if (stringBuilder.Length == 0)
                return false;

            return stringBuilder[stringBuilder.Length - 1] == chr;
        }
        #endregion

        #region TrimStart
        /// <summary>
        /// Trims all the spaces at the start of the <paramref name="stringBuilder"/>
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <returns></returns>
        public static StringBuilder TrimStart(this StringBuilder stringBuilder)
        {
            if (stringBuilder.Length == 0)
                return stringBuilder;

            var start = 0;

            for (var i = 0; i < stringBuilder.Length - 1; i++)
            {
                if (char.IsWhiteSpace(stringBuilder[i]))
                    start++;
            }

            return new StringBuilder(stringBuilder.ToString(), start, stringBuilder.Length - start,
                stringBuilder.Capacity);
        }
        #endregion

        #region TrimEnd
        /// <summary>
        /// Trims all the spaces at the end of the <paramref name="stringBuilder"/>
        /// </summary>
        /// <param name="stringBuilder"></param>
        public static void TrimEnd(this StringBuilder stringBuilder)
        {
            for (var i = stringBuilder.Length - 1; i > 0; i--)
            {
                if (char.IsWhiteSpace(stringBuilder[i]))
                    stringBuilder.Length--;
                else
                    return;
            }
        }
        #endregion
    }

}

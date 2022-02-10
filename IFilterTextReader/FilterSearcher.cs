//
// FilterSearcher.cs
//
// Author: Kees van Spelde <sicos2002@hotmail.com>
//
// Copyright (c) 2013-2022 Magic-Sessions. (www.magic-sessions.com)
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
using System.Linq;
using System.Text.RegularExpressions;
// ReSharper disable UnusedMember.Global

// ReSharper disable UnusedType.Global
namespace IFilterTextReader
{
    /// <summary>
    /// This class contains methods to search for text
    /// </summary>
    public class FilterSearcher
    {
        #region FileContainsText
        /// <summary>
        /// Returns true when the <paramref name="textToFind"/> is found in the 
        /// <paramref name="fileName"/>
        /// </summary>
        /// <param name="fileName">The file to inspect</param>
        /// <param name="textToFind">The text to find</param>
        /// <param name="ignoreCase">Set to false to search case sensitive</param>
        /// <returns></returns>
        public bool FileContainsText(string fileName, string textToFind, bool ignoreCase = true)
        {
            using (var reader = new FilterReader(fileName))
            {
                var line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                    return false;

                if (ignoreCase)
                {
                    if (line.IndexOf(textToFind, StringComparison.InvariantCultureIgnoreCase) >= 0)
                        return true;
                }
                else
                {
                    if (line.IndexOf(textToFind, StringComparison.InvariantCulture) >= 0)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true when at least one of the given <paramref name="textToFind"/> is found 
        /// in the <paramref name="fileName"/>
        /// </summary>
        /// <param name="fileName">The file to inspect</param>
        /// <param name="textToFind">The array with one or more text to find</param>
        /// <param name="ignoreCase">Set to false to search case sensitive</param>
        /// <returns></returns>
        public bool FileContainsText(string fileName, string[] textToFind, bool ignoreCase = true)
        {
            using (var reader = new FilterReader(fileName))
            {
                var line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                    return false;

                if (ignoreCase)
                    line = line.ToUpperInvariant();

                if (textToFind.Any(text => line.Contains(text)))
                    return true;
            }

            return false;
        }
        #endregion

        #region FileContainsRegexMatch
        /// <summary>
        /// Returns true when the <paramref name="regularExpression"/> is found in the 
        /// <paramref name="fileName"/>
        /// </summary>
        /// <param name="fileName">The file to inspect</param>
        /// <param name="regularExpression">The regular expression to use</param>
        /// <param name="ignoreCase">Set to false to search case sensitive</param>
        /// <returns></returns>
        public bool FileContainsRegexMatch(string fileName, string regularExpression, bool ignoreCase = true)
        {
            var regex = new Regex(regularExpression, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
            using (var reader = new FilterReader(fileName))
            {
                var line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                    return false;

                if (regex.IsMatch(line))
                    return true;
            }

            return false;
        }
        #endregion
        
        #region GetRegexMatchesFromFile
        /// <summary>
        /// Returns an array with all the matches that are found with the give <see cref="regularExpression"/> regular expression
        /// </summary>
        /// <param name="fileName">The file to inspect</param>
        /// <param name="regularExpression">The regular expression to use</param>
        /// <param name="ignoreCase">Set to false to search case sensitive</param>
        /// <returns></returns>
        public string[] GetRegexMatchesFromFile(string fileName, string regularExpression, bool ignoreCase = true)
        {
            var regex = new Regex(regularExpression, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
            var result = new List<string>();

            using (var reader = new FilterReader(fileName))
            {
                var text = reader.ReadToEnd();
                result.AddRange(from Match match in regex.Matches(text) select match.ToString());
            }

            return result.ToArray();
        }
        #endregion
    }
}

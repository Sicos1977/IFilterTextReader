using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;
using IFilterTextReader;

namespace IFilterTextViewer
{
    [ServiceContract]
    public interface IReader
    {
        [OperationContract]
        string GetAllText(string fileName);

        [OperationContract]
        bool FileContainsText(string fileName, string textToFind, bool ignoreCase = true);

        [OperationContract]
        bool FileContainsTextFromArray(string fileName, string[] textToFind, bool ignoreCase = true);

        [OperationContract]
        bool FileContainsRegexMatch(string fileName, string regularExpression, bool ignoreCase = true);

        [OperationContract]
        string[] GetRegexMatchesFromFile(string fileName, string regularExpression, bool ignoreCase = true);
    }

    /// <summary>
    /// This class contains methods to search for text inside a file with the help of a IFilter
    /// </summary>
    public class Reader : IReader
    {
        #region GetAllText
        /// <summary>
        /// Returns all the text that is inside the <paramref name="fileName"/>
        /// </summary>
        /// <param name="fileName">The file to read</param>
        /// <returns></returns>
        public string GetAllText(string fileName)
        {
            using (var reader = new FilterReader(fileName))
                return reader.ReadToEnd();
        }
        #endregion
        
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
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line))
                        continue;

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
        public bool FileContainsTextFromArray(string fileName, string[] textToFind, bool ignoreCase = true)
        {
            using (var reader = new FilterReader(fileName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line))
                        continue;

                    if (ignoreCase)
                        line = line.ToUpperInvariant();

                    foreach (var text in textToFind)
                    {
                        var temp = text;
                        if (ignoreCase)
                            temp = text.ToUpperInvariant();

                        if (line.Contains(temp))
                            return true;
                    }
                }
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
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line))
                        continue;

                    if (regex.IsMatch(line))
                        return true;
                }
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

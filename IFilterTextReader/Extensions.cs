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

// /**********************************************************************************************
// Author:  Vasily Kabanov
// Created  2017-06-12
// Comment  
// **********************************************************************************************/
// 
using System.Text.RegularExpressions;
using Pim.CommonLib;

namespace FulltextStorageLib.Util
{
    public static class StringHelper
    {
        private static readonly Regex WhiteSpaceRegex = new Regex("\\s");
        private static readonly Regex FirstNonEmptyLineRegex = new Regex(@"([\S]+.*[\S]+)\s*$", RegexOptions.Multiline);

        /// <summary>
        ///     Extract fragment at word boundaries.
        /// </summary>
        /// <param name="text">
        ///     Source text.
        /// </param>
        /// <param name="startIndex">
        ///     0-based, inclusive.
        /// </param>
        /// <param name="maxLength">
        ///     Limits result length
        /// </param>
        /// <param name="addTrailingEllipsis">
        ///     Append '...' if truncating.
        /// </param>
        public static string GetTextWithLimit(string text, int startIndex, int maxLength, bool addTrailingEllipsis = true)
        {
            Check.DoRequireArgumentNotNull(text, nameof(text));
            Check.DoCheckArgument(startIndex >= 0, nameof(startIndex));
            Check.DoCheckArgument(maxLength > 50, nameof(maxLength));

            var length = text.Length - startIndex;

            if (length > maxLength)
            {
                // need to truncate
                var firstWhiteSpace = WhiteSpaceRegex.Match(text, (int)(startIndex + 0.9 * maxLength));

                var summaryLength = maxLength - 3;

                if (firstWhiteSpace.Success && firstWhiteSpace.Index - (startIndex + maxLength) < 50)
                {
                    summaryLength = firstWhiteSpace.Index - startIndex;
                }
                var result = text.Substring(startIndex, summaryLength);
                if (addTrailingEllipsis)
                    result = result + "...";

                return result;
            }

            return text.Substring(startIndex);
        }

        /// <returns>
        ///     null if text contains any valid text (non-blank-space)
        /// </returns>
        public static string ExtractFirstLine(string text)
        {
            if (text == null)
                return null;

            var nameMatch = FirstNonEmptyLineRegex.Match(text);
            return nameMatch.Success
                ? nameMatch.Groups[1].Value
                : null;
        }
    }
}
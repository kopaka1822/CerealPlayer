using System;

namespace CerealPlayer.Utility
{
    public static class StringUtil
    {
        /// <summary>
        ///     returns a substring from source[from] until the first occurence of "to" ("to" is not included)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static string SubstringUntil(string source, int from, char to)
        {
            var res = "";
            while (source[from] != to)
            {
                res += source[from++];
            }

            return res;
        }

        /// <summary>
        ///     returns a substring from 'source' from the first whitespace
        ///     or " character before 'from' until the first whitespace or " character after 'from'
        /// </summary>
        /// <param name="source"></param>
        /// <param name="from">character index within the link</param>
        /// <param name="readFromStart">
        ///     indicates if the entire link should be read (true, default)
        ///     or only the link starting at index 'from' (false)
        /// </param>
        /// <returns></returns>
        public static string ReadLink(string source, int from, bool readFromStart = true)
        {
            var res = "";

            if (readFromStart)
            {
                // search for link start
                while (from > 0 && !char.IsWhiteSpace(source[from - 1]) && source[from - 1] != '\"')
                {
                    from--;
                }
            }

            // read until link end
            while (from < source.Length && !char.IsWhiteSpace(source[from]) && source[from] != '\"')
            {
                res += source[from++];
            }

            return res;
        }

        /// <summary>
        /// skips all whitespaces
        /// </summary>
        /// <param name="source">string</param>
        /// <param name="from">starting index</param>
        /// <returns></returns>
        public static int SkipWhitespace(string source, int from)
        {
            while (from < source.Length && char.IsWhiteSpace(source[from]))
            {
                ++from;
            }

            return from;
        }

        /// <summary>
        ///     returns the first index after "from" where the character "to" appears
        /// </summary>
        /// <param name="source"></param>
        /// <param name="from">start index</param>
        /// <param name="to">searched character</param>
        /// <returns></returns>
        public static int SkipUntil(string source, int from, char to)
        {
            while (from < source.Length && source[from] != to)
            {
                ++from;
            }

            return from;
        }

        /// <summary>
        ///     returns a substring from source that ends at index "lastIndex" and start after the first occurence of "to" ("to"
        ///     not included)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="lastIndex"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static string BackwardSubstringUntil(string source, int lastIndex, char to)
        {
            var res = "";
            while (source[lastIndex] != to)
            {
                res = source[lastIndex--] + res;
            }

            return res;
        }

        /// <summary>
        ///     combines all strings into on string using the seperator in between elements
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="seperator"></param>
        /// <returns></returns>
        public static string Reduce(string[] parts, string seperator)
        {
            return Reduce(parts, seperator, 0, parts.Length);
        }

        /// <summary>
        ///     combines all strings into on string using the seperator in between elements
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="seperator"></param>
        /// <param name="first">start index (included)</param>
        /// <param name="last">index of the last part (not included in the reduce)</param>
        /// <returns></returns>
        public static string Reduce(string[] parts, string seperator, int first, int last)
        {
            last = Math.Min(parts.Length, last);
            if (first >= last) return "";

            var res = "";
            for (var i = first; i < last - 1; ++i)
                res += parts[i] + seperator;

            return res + parts[last - 1];
        }
    }
}
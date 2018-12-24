﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CerealPlayer.Utility
{
    public static class StringUtil
    {
        /// <summary>
        /// returns a substring from source[from] until the first occurence of "to" ("to" is not included)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static string SubstringUntil(string source, int from, char to)
        {
            string res = "";
            while (source[from] != to)
            {
                res += source[from++];
            }

            return res;
        }

        /// <summary>
        /// returns a substring from source that ends at index "lastIndex" and start after the first occurence of "to" ("to" not included)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="lastIndex"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static string BackwardSubstringUntil(string source, int lastIndex, char to)
        {
            string res = "";
            while (source[lastIndex] != to)
            {
                res = source[lastIndex--] + res;
            }

            return res;
        }
    }
}

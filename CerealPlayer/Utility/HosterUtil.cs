using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CerealPlayer.Annotations;

namespace CerealPlayer.Utility
{
    /// <summary>
    /// string utilities that help to determine certain websites or information from those websites
    /// </summary>
    public static class HosterUtil
    {
        private static readonly CultureInfo Culture = new CultureInfo("en-US");

        public static string IncrementLastNumber([NotNull] string website)
        {
            // search last digit
            int last = website.Length - 1;
            while (last >= 0 && !Char.IsDigit(website[last]))
            {
                --last;
            }
            if(last < 0) throw new Exception("no number found in \"" + website + "\"");

            int first = last - 1;
            while (first >= 0 && Char.IsDigit(website[first]))
            {
                --first;
            }
            // number goes from [first + 1 to last]
            var left = website.Substring(0, first + 1);
            var right = website.Substring(last + 1);
            var strNumber = website.Substring(first + 1, last - first);

            var newNum = Int32.Parse(strNumber) + 1;
            return left + newNum + right;
        }
    }
}

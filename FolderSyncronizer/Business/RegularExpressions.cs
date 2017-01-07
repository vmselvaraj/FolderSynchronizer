using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FolderSyncronizer.Business
{
    public static class CommandLineDirOutputParser
    {
        #region DateTime Parsing
        public static DateTime? GetDateTimeString(ref string input)
        {
            string re1 = "((?:[0]?[1-9]|[1][012])[-:\\/.](?:(?:[0-2]?\\d{1})|(?:[3][01]{1}))[-:\\/.](?:(?:[1]{1}\\d{1}\\d{1}\\d{1})|(?:[2]{1}\\d{3})))(?![\\d])";   // MMDDYYYY 1
            string re2 = ".*?"; // Non-greedy match on filler
            string re3 = "((?:(?:[0-1][0-9])|(?:[2][0-3])|(?:[0-9])):(?:[0-5][0-9])(?::[0-5][0-9])?(?:\\s?(?:am|AM|pm|PM))?)";  // HourMinuteSec 1

            Regex r = new Regex(re1 + re2 + re3, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match m = r.Match(input);
            if (m.Success)
            {
                String mmddyyyy = m.Groups[1].ToString();
                String time = m.Groups[2].ToString();

                string dateTimeString = mmddyyyy + " " + time;
                input = input.Replace(mmddyyyy, string.Empty).Trim();
                input = input.Replace(time, string.Empty).Trim();
                return dateTimeString.toDate();
            }
            return null;
        }

        public static DateTime? toDate(this string dateTimeStr)
        {

            string[] dateFmt = {"M/d/yyyy h:mm:ss tt", "M/d/yyyy h:mm tt",
                     "MM/dd/yyyy hh:mm:ss", "M/d/yyyy h:mm:ss",
                     "M/d/yyyy hh:mm tt", "M/d/yyyy hh tt",
                     "M/d/yyyy h:mm", "M/d/yyyy h:mm",
                     "MM/dd/yyyy hh:mm", "M/dd/yyyy hh:mm"};


            // example: var dt="2011-03-21 13:26".toDate("yyyy-MM-dd HH:mm");
            const DateTimeStyles style = DateTimeStyles.AllowWhiteSpaces;
            DateTime? result = null;
            DateTime dt;
            if (DateTime.TryParseExact(dateTimeStr, dateFmt,
                CultureInfo.InvariantCulture, style, out dt)) result = dt;
            return result;
        }
        #endregion

        #region Directory Checking

        public static bool IsDirectory(ref string input)
        {
            string re1 = ".*?"; // Non-greedy match on filler
            string re2 = "(<[^>]+>)";   // Tag 1

            Regex r = new Regex(re1 + re2, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match m = r.Match(input);
            if(m.Success)
            {
                input = input.Replace(m.Groups[0].ToString(), string.Empty).Trim();
            }
            return m.Success;
        }
        #endregion

        #region FileSize

        public static long GetFileSize(ref string input)
        {
            string re1 = @"(?<=[^\d,.]|^)\d{1,3}(,(\d{3}))*((?=[,.]\s)|(\.\d+)?(?=[^\d,.]|$))";
            Regex r = new Regex(re1, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match m = r.Match(input);

            string matchedString = m.Groups[0].ToString();
            input = input.Replace(matchedString, string.Empty);
            return Convert.ToInt64(matchedString.Replace(",", string.Empty));
        }

        #endregion

    }
}

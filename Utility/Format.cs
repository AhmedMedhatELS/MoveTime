using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public static class Format
    {
        public static string FormatTimeInArabic(TimeSpan time)
        {
            int hour = time.Hours;
            string period = hour >= 12 ? "م" : "ص";
            hour = hour > 12 ? hour - 12 : (hour == 0 ? 12 : hour); // Convert to 12-hour format

            // Convert the hour to Arabic numerals
            string hourInArabic = hour.ToString(new CultureInfo("ar-SA"));
            return $"{hourInArabic} {period}";
        }

        public static readonly Dictionary<int, string> NumberMapping = new()
        {
            { 1, "الأولى" },
            { 2, "الثانية" },
            { 3, "الثالثة" },
            { 4, "الرابعة" },
            { 5, "الخامسة" },
            { 6, "السادسة" },
            { 7, "السابعة" },
            { 8, "الثامنة" },
            { 9, "التاسعة" },
            { 10, "العاشرة" },
            { 11, "الحادية عشر" },
            { 12, "الثانية عشر" }
        };
    }
}

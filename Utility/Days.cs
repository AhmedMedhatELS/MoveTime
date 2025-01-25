using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public enum Days
    {
        AllDays,
        Sunday,       
        Monday,      
        Tuesday,     
        Wednesday,    
        Thursday,     
        Friday,       
        Saturday
    }
    public static class DaysOfWeekTranslations
    {
        public static readonly Dictionary<Days, string> Translations = new()
        {
            { Days.AllDays, "كل الأيام" },
            { Days.Sunday, "الأحد" },
            { Days.Monday, "الإثنين" },
            { Days.Tuesday, "الثلاثاء" },
            { Days.Wednesday, "الأربعاء" },
            { Days.Thursday, "الخميس" },
            { Days.Friday, "الجمعة" },
            { Days.Saturday, "السبت" }
        };
    }

}

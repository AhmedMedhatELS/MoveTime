using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public enum Gender
    {
        Male,
        Female
    }

    public static class GenderTranslations
    {
        public static readonly Dictionary<Gender, string> Translations = new()
        {
            { Gender.Male, "ذكر" },
            { Gender.Female, "أنثى" }
        };
    }
}

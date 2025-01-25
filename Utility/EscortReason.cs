using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public enum EscortReason
    {
        None,
        Age,
        HealthCondition,
        MentalHealth
    }

    public static class EscortReasonTranslations
    {
        public static readonly Dictionary<EscortReason, string> Translations = new()
        {
            { EscortReason.None, "لا يوجد" }, // None
            { EscortReason.Age, "العمر" }, // Age
            { EscortReason.HealthCondition, "الحالة الصحية" }, // Health Condition
            { EscortReason.MentalHealth, "الصحة النفسية" } // Mental Health
        };
    }
}

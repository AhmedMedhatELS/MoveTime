using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public enum SubscriptionStatus
    {
        Active,        
        Canceled,      
        Suspended,     
        Expired        
    }

    public static class SubscriptionStatusTranslations
    {
        public static readonly Dictionary<SubscriptionStatus, string> Translations = new()
    {
        { SubscriptionStatus.Active, "نشط" },
        { SubscriptionStatus.Canceled, "ملغى" },
        { SubscriptionStatus.Suspended, "معلق" },
        { SubscriptionStatus.Expired, "منتهى" }
    };
    }


}

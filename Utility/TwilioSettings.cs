using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public class TwilioSettings
    {
        public string AccountSID { get; set; } = null!;
        public string AuthToken { get; set; } = null!;
        public string WhatsAppFrom { get; set; } = null!;
    }

}

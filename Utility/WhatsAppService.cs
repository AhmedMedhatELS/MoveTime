using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Utility
{
    public class WhatsAppService
    {
        private readonly TwilioSettings _twilioSettings;

        public WhatsAppService(IOptions<TwilioSettings> twilioSettings)
        {
            _twilioSettings = twilioSettings.Value;
            TwilioClient.Init(_twilioSettings.AccountSID, _twilioSettings.AuthToken);
        }

        public void SendWhatsAppMessage(string to, string messageBody)
        {
            _ = MessageResource.Create(
                body: messageBody,
                from: new Twilio.Types.PhoneNumber(_twilioSettings.WhatsAppFrom),
                to: new Twilio.Types.PhoneNumber("whatsapp:" + to)
            );
        }

        public void SendWhatsAppMessageList(List<string> to, string messageBody)
        {
            for(int i = 0; i < to.Count; i++) 
                _ = MessageResource.Create(
                    body: messageBody,
                    from: new Twilio.Types.PhoneNumber(_twilioSettings.WhatsAppFrom),
                    to: new Twilio.Types.PhoneNumber("whatsapp:" + to[i])
                );
        }
    }
}

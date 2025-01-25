using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("ahmedmedhatt2112@gmail.com", "qnyy vesd mars pcle")
            };

            return client.SendMailAsync(
                new MailMessage(from: "ahmedmedhatt2112@gmail.com",
                                to: email,
                                subject,
                                message
                                )
                { IsBodyHtml = true}
                );
        }
    }
}

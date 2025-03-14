using Application.DTOs;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(EmailRequest request)
        {
            try
            {
                // create message
                var message = new MimeMessage();
                message.Sender = new MailboxAddress(_config["MailSettings:DisplayName"], _config["MailSettings:EmailFrom"]);
                message.To.Add(MailboxAddress.Parse(request.To));
                message.Subject = request.Subject;

                var builder = new BodyBuilder();
                if (request.IsHtmlBody)
                {
                    builder.HtmlBody = request.Body; // Use HTML body
                }
                else
                {
                    builder.TextBody = request.Body; // Use plain text body
                }

                message.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                smtp.Connect(_config["MailSettings:SmtpHost"], Convert.ToInt32(_config["MailSettings:SmtpPort"]), SecureSocketOptions.StartTls);
                smtp.Authenticate(_config["MailSettings:SmtpUser"], _config["MailSettings:SmtpPass"]);
                await smtp.SendAsync(message);
                smtp.Disconnect(true);

            }
            catch (System.Exception ex)
            { }
        }
    }
}

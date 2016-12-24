using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles;
using Pomelo.Net.Smtp;

namespace Microsoft.AspNetCore.Mvc
{
    public class WebMail
    {
        private readonly IContentTypeProvider _contentTypeProvider;

        public WebMail(IContentTypeProvider contentTypeProvider)
        {
            _contentTypeProvider = contentTypeProvider;
        }

        public static string SmtpServer { get; set; }

        public static int SmtpPort { get; set; } = 25;

        public static string From { get; set; }

        public static bool EnableSsl { get; set; }

        public static string UserName { get; set; }

        public static string Password { get; set; }

        public void Send(string to,
                        string subject,
                        string body,
                        string from = null,
                        string cc = null,
                        IEnumerable<string> filesToAttach = null,
                        bool isBodyHtml = true,
                        IEnumerable<string> additionalHeaders = null,
                        string bcc = null,
                        string contentEncoding = null,
                        string headerEncoding = null,
                        string priority = null,
                        string replyTo = null,
                        IEnumerable<Attachment> attachments = null)
        {
            if (attachments == null) attachments = new Attachment[0];
            var sender = new SmtpEmailSender(_contentTypeProvider, SmtpServer, SmtpPort, from ?? From ?? UserName, UserName, UserName, Password, EnableSsl);
            sender.SendEmailAsync(to, cc, bcc, subject, body, attachments.ToArray()).Wait();
        }
    }
}

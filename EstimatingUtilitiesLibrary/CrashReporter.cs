using MailKit.Net.Smtp;
using MimeKit;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingUtilitiesLibrary
{
    public static class CrashReporter
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private const string senderUsername = "TECSystemsEstimating@gmail.com";
        private const string senderPassword = "gnitamitse";

        private const string domainName = "smtp.gmail.com";
        private const int port = 465;

        private const string subjectLine = "Crash Report";

        public static bool SendCrashReport(string logPath, string userReport, IEnumerable<string> recievingEmails)
        {
            //Create Message
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderUsername));
            foreach(string email in recievingEmails)
            {
                message.To.Add(new MailboxAddress(email));
            }
            message.Subject = subjectLine;

            TextPart bodyText = new TextPart("plain")
            {
                Text = string.Format("User Report: \n {0}", userReport)
            };

            MimePart attatchment = new MimePart("text", "log")
            {
                Content = new MimeContent(File.OpenRead(logPath)),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = logPath
            };

            Multipart body = new Multipart("mixed");
            body.Add(bodyText);
            body.Add(attatchment);

            message.Body = body;

            //Send Message
            using (var client = new SmtpClient())
            {
                try
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    client.Connect(domainName, port, true);

                    client.Authenticate(senderUsername, senderPassword);

                    client.Send(message);

                    client.Disconnect(true);

                    return true;
                }
                catch (Exception e)
                {
                    logger.Error(string.Format("Email exception: {0}", e));
                    return false;
                }
            }
        }
    }
}

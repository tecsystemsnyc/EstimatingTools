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
    public static class BugReporter
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private const string senderUsername = "TECSystemsEstimating@gmail.com";
        private const string senderPassword = "gnitamitse";

        private const string domainName = "smtp.gmail.com";
        private const int port = 465;

        public static bool SendBugReport(string reportType, string userName, string userEmail, string userReport, string logPath, IEnumerable<string> recievingEmails)
        {
            //Create Message
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderUsername));
            foreach(string email in recievingEmails)
            {
                message.To.Add(new MailboxAddress(email));
            }
            message.Subject = string.Format("{0} Report", reportType);

            Multipart body = new Multipart("mixed");
            message.Body = body;

            TextPart bodyText = new TextPart("plain")
            {
                Text = string.Format("{0} Report from {1}. Email: {2} \n Description:\n {3}", reportType, userName, userEmail, userReport)
            };
            body.Add(bodyText);

            if (logPath != "" && File.Exists(logPath))
            {
                MimePart attatchment = new MimePart("text", "log")
                {
                    Content = new MimeContent(File.OpenRead(logPath)),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = logPath
                };
                body.Add(attatchment);
            }

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

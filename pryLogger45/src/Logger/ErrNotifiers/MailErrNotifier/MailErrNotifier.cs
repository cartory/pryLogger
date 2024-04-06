using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace pryLogger.src.Logger.ErrNotifiers.MailErrNotifier
{
    public class MailErrNotifier : ErrNotifier
    {
        private Action<SmtpClient> OnSmtpClient { get; set; }
        public MailConnectionString MailConnectionString { get; private set; }

        public MailErrNotifier(string connectionString, Action<SmtpClient> OnSmtpClient = null)
        {
            this.OnSmtpClient = OnSmtpClient;
            MailConnectionString = new MailConnectionString(connectionString);
        }

        public static MailErrNotifier FromConnectionString(string connectionString, Action<SmtpClient> OnSmtpClient = null)
        {
            return new MailErrNotifier(connectionString, OnSmtpClient);
        }

        public override void Notify(ErrNotification err, bool throwException = false)
        {
            try
            {
                err = ErrNotification.FromLog(err.ErrLog.HtmlEncode());

                using (SmtpClient smtpClient = new SmtpClient(MailConnectionString.Host, MailConnectionString.Port))
                {
                    smtpClient.EnableSsl = MailConnectionString.EnableSsl;
                    if (MailConnectionString.Password != null)
                    {
                        smtpClient.Credentials = new NetworkCredential(MailConnectionString.From, MailConnectionString.Password);
                    }

                    OnSmtpClient?.Invoke(smtpClient);
                    err.Repository = MailConnectionString.Repository;

                    using (MailMessage message = new MailMessage(MailConnectionString.From, MailConnectionString.To)
                    {
                        IsBodyHtml = true,
                        Body = err.ToHtml(),
                        Subject = err.Title,
                    })
                    {
                        FileNames?.Select(fileName => 
                        {
                            if (File.Exists(fileName))
                            {
                                var attachment = new Attachment(fileName);
                                message.Attachments.Add(attachment);
                            }

                            return 0;
                        });

                        if (MailConnectionString.CopyTo?.Length > 0)
                        {
                            message.CC.Add(string.Join(",", MailConnectionString.CopyTo));
                        }

                        smtpClient.Send(message);
                        Console.WriteLine($"{nameof(MailErrNotifier)} mailTo={MailConnectionString.To} OK");
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"{nameof(MailErrNotifier)} mailTo={MailConnectionString.To} ERROR {e.Message}");
                if (throwException) throw;
            }
        }
    }
}

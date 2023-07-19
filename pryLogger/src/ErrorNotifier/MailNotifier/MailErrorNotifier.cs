using System;
using System.Text;
using System.Net.Mail;
using System.Collections.Generic;

namespace pryLogger.src.ErrorNotifier.MailNotifier
{
    public class MailErrorNotifier : IErrorNotifier
    {
        public DateTimeOffset LastNotificationSent { get; set; }

        public Attachment Attachment { get; private set; }
        public MailConnection MailConnection { get; private set; }

        public MailErrorNotifier() { }
        public MailErrorNotifier(MailConnection mailConnection) => this.MailConnection = mailConnection;
        public MailErrorNotifier(string connectionString) => this.MailConnection = new MailConnection(connectionString);

        public static MailErrorNotifier FromConnectionString(string connectionString) => new MailErrorNotifier(connectionString);

        public IErrorNotifier SetAttachMent(string fileName)
        {
            this.Attachment = new Attachment(fileName);
            return this;
        }

        public void Notify(ErrorNotification error)
        {
            DateTimeOffset now = DateTimeOffset.Now;
            TimeSpan diff = now - LastNotificationSent;

            if (diff.TotalMinutes < MailConnection.IntervalMinutes) return;

            LastNotificationSent = now;
            error.Repository = MailConnection.Repository;

            using (var smtp = new SmtpClient(MailConnection.Host, MailConnection.Port))
            {
                using (var message = new MailMessage(MailConnection.From, MailConnection.To) 
                {
                    IsBodyHtml = true,
                })
                {
                    message.Body = error.ToHtml();
                    message.Subject = error.Title;

                    if (Attachment != null) 
                    {
                        message.Attachments.Add(Attachment);
                    }

                    if (this.MailConnection.CopyTo?.Length > 0) 
                    {
                        message.CC.Add(string.Join(",", this.MailConnection.CopyTo));

                    }

                    smtp.Send(message);
                }
            }
        }
    }
}

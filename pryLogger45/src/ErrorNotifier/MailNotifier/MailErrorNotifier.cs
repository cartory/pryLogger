using System;
using System.Text;
using System.Net.Mail;
using System.Collections.Generic;

namespace pryLogger.src.ErrorNotifier.MailNotifier
{
    /// <summary>
    /// Represents an error notifier that sends notifications via email.
    /// </summary>
    public class MailErrorNotifier : IErrorNotifier
    {
        /// <summary>
        /// Gets or sets the timestamp of the last notification sent.
        /// </summary>
        public DateTimeOffset LastNotificationSent { get; set; }

        /// <summary>
        /// Gets or sets the email attachment for the notification.
        /// </summary>
        public Attachment Attachment { get; private set; }

        /// <summary>
        /// Gets or sets the mail connection configuration.
        /// </summary>
        public MailConnection MailConnection { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MailErrorNotifier class.
        /// </summary>
        public MailErrorNotifier() { }

        /// <summary>
        /// Initializes a new instance of the MailErrorNotifier class with a specified MailConnection.
        /// </summary>
        /// <param name="mailConnection">The MailConnection configuration.</param>
        public MailErrorNotifier(MailConnection mailConnection) => this.MailConnection = mailConnection;

        /// <summary>
        /// Initializes a new instance of the MailErrorNotifier class with a connection string.
        /// </summary>
        /// <param name="connectionString">The connection string for the mail server.</param>
        public MailErrorNotifier(string connectionString) => this.MailConnection = new MailConnection(connectionString);

        /// <summary>
        /// Creates a MailErrorNotifier instance from a connection string.
        /// </summary>
        /// <param name="connectionString">The connection string for the mail server.</param>
        /// <returns>A MailErrorNotifier instance.</returns>
        public static MailErrorNotifier FromConnectionString(string connectionString) => new MailErrorNotifier(connectionString);

        /// <summary>
        /// Sets an attachment to be included in the email notification.
        /// </summary>
        /// <param name="fileName">The path to the attachment file.</param>
        /// <returns>The updated MailErrorNotifier instance.</returns>
        public IErrorNotifier SetAttachMent(string fileName)
        {
            this.Attachment = new Attachment(fileName);
            return this;
        }

        /// <summary>
        /// Sends an error notification via email.
        /// </summary>
        /// <param name="error">The error notification to send.</param>
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

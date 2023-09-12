using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace pryLogger.src.ErrorNotifier.MailNotifier
{
    /// <summary>
    /// Represents a connection to a mail server for error notifications.
    /// </summary>
    public class MailConnection
    {
        /// <summary>
        /// Gets the connection string for the mail server.
        /// </summary>
        public readonly string ConnectionString;

        private readonly Dictionary<string, string> Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets the SMTP port for the mail server.
        /// </summary>
        public int Port { get; private set; } = 25;

        /// <summary>
        /// Gets or sets the interval in minutes for sending email notifications.
        /// </summary>
        public int IntervalMinutes { get; private set; } = 1;

        /// <summary>
        /// Gets or sets the recipient email address.
        /// </summary>
        public string To { get; private set; }

        /// <summary>
        /// Gets or sets the sender email address.
        /// </summary>
        public string From { get; private set; }

        /// <summary>
        /// Gets or sets the host name of the mail server.
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// Gets or sets an array of email addresses to copy notifications to.
        /// </summary>
        public string[] CopyTo { get; private set; }

        /// <summary>
        /// Gets or sets the repository associated with the error notifications.
        /// </summary>
        public string Repository { get; private set; }

        /// <summary>
        /// Sets the sender email address.
        /// </summary>
        /// <param name="mailFrom">The sender email address.</param>
        /// <returns>The updated MailConnection instance.</returns>
        public MailConnection SetMailFrom(string mailFrom)
        {
            this.From = mailFrom;
            return this;
        }

        /// <summary>
        /// Sets the repository associated with the error notifications.
        /// </summary>
        /// <param name="repository">The repository name.</param>
        /// <returns>The updated MailConnection instance.</returns>
        public MailConnection SetRepository(string repository)
        {
            this.Repository = repository;
            return this;
        }

        /// <summary>
        /// Sets the SMTP host and port for the mail server.
        /// </summary>
        /// <param name="host">The SMTP host name.</param>
        /// <param name="port">The SMTP port number.</param>
        /// <returns>The updated MailConnection instance.</returns>
        public MailConnection SetHostPort(string host, int port)
        {
            this.Host = host;
            this.Port = port;
            return this;
        }

        /// <summary>
        /// Sets the interval in minutes for sending email notifications.
        /// </summary>
        /// <param name="intervalMinutes">The interval in minutes.</param>
        /// <returns>The updated MailConnection instance.</returns>
        public MailConnection SetMailIntervalMinutes(int intervalMinutes)
        {
            this.IntervalMinutes = intervalMinutes;
            return this;
        }

        /// <summary>
        /// Sets the recipient email address and optional copy recipients.
        /// </summary>
        /// <param name="mailTo">The recipient email address.</param>
        /// <param name="copyTo">Optional email addresses to copy notifications to.</param>
        /// <returns>The updated MailConnection instance.</returns>
        public MailConnection SetMailTo(string mailTo, params string[] copyTo)
        {
            this.To = mailTo;
            this.CopyTo = copyTo.Select(c => c.Trim()).ToArray();
            return this;
        }

        /// <summary>
        /// Initializes a new instance of the MailConnection class.
        /// </summary>
        public MailConnection() { }

        /// <summary>
        /// Initializes a new instance of the MailConnection class with a connection string.
        /// </summary>
        /// <param name="connectionString">The connection string for the mail server.</param>
        public MailConnection(string connectionString)
        {
            this.ConnectionString = connectionString.Trim();
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException(nameof(connectionString));

            if (connectionString.Contains("="))
            {
                foreach (string keyValue in connectionString.Split(';'))
                {
                    if (string.IsNullOrEmpty(keyValue) || string.IsNullOrWhiteSpace(keyValue))
                    {
                        continue;
                    }

                    var arrKeyValue = keyValue.Trim().Split('=');
                    Values.Add(key: arrKeyValue[0], value: arrKeyValue[1]);
                }

                this.To = Values["to"];
                this.From = Values["from"];

                this.Host = Values["host"];
                this.Port = int.Parse(Values["port"]);

                if (Values.ContainsKey("repo"))
                {
                    this.Repository = Values["repo"];
                }

                if (Values.ContainsKey("interval"))
                {
                    this.IntervalMinutes = int.Parse(Values["interval"]);
                }

                if (Values.ContainsKey("copyto"))
                {
                    this.CopyTo = Values["copyto"].Split(',').Select(c => c.Trim()).ToArray();
                }
            }
        }
    }
}

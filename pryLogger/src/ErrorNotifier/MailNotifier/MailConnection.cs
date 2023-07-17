using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace pryLogger.src.ErrorNotifier.MailNotifier
{
    public class MailConnection
    {
        public readonly string ConnectionString;
        private readonly Dictionary<string, string> Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public int Port { get; private set; } = 25;
        public int IntervalMinutes { get; private set; } = 1;

        public string To { get; private set; }
        public string From { get; private set; }
        public string Host { get; private set; }

        public string[] CopyTo { get; private set; }
        public string Repository { get; private set; }

        public MailConnection SetMailFrom(string mailFrom)
        {
            this.From = mailFrom;
            return this;
        }

        public MailConnection SetRepository(string repository)
        {
            this.Repository = repository;
            return this;
        }

        public MailConnection SetHostPort(string host, int port)
        {
            this.Host = host;
            this.Port = port;

            return this;
        }

        public MailConnection SetMailIntervalMinutes(int intervalMinutes)
        {
            this.IntervalMinutes = intervalMinutes;
            return this;
        }

        public MailConnection SetMailTo(string mailTo, params string[] copyTo)
        {
            this.To = mailTo;
            this.CopyTo = copyTo.Select(c => c.Trim()).ToArray();

            return this;
        }

        public MailConnection() { }

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

                    string key = arrKeyValue[0];
                    string value = arrKeyValue[1];

                    Values.Add(key, value);
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

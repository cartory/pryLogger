using System;
using System.Linq;
using System.Collections.Generic;

namespace pryLogger.src.Logger.ErrNotifiers.MailErrNotifier
{
    public class MailConnectionString
    {
        public readonly string ConnectionString;

        public bool EnableSsl { get; set; }
        public int Port { get; set; }

        public string Host { get; set; }
        public string[] CopyTo { get; set; }

        public string To { get; set; }
        public string From { get; set; }

        public string Password { get; set; }
        public string Repository { get; set; }

        public MailConnectionString SetHostPort(string host, int port)
        {
            Host = host;
            Port = port;
            return this;
        }

        public MailConnectionString SetMailFromTo(
            string mailFrom, string password, 
            string mailTo, params string[] copyTo
        )
        {
            To = mailTo;
            From = mailFrom;
            Password = password;

            if (copyTo.Length > 0)
            {
                CopyTo = copyTo.Select(c => c.Trim()).ToArray();
            }

            return this;
        }

        public static MailConnectionString FromConnectionString(string connectionString)
        {
            return new MailConnectionString(connectionString);
        }

        public MailConnectionString(string connectionString)
        {
            ConnectionString = connectionString = connectionString.Trim();
            var values = connectionString.ToDictionary() ?? throw new ArgumentException("invalid connectionString");

            To = values["to"];
            From = values["from"];

            Host = values["host"];
            Port = int.Parse(values["port"]);

            var keysAction = new Dictionary<string, Action<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["repo"] = k => Repository = values[k],
                ["password"] = k => Password = values[k],
                ["ssl"] = k => EnableSsl = bool.Parse(values[k]),
                ["copyto"] = k => CopyTo = values[k].Split(',')
                                                    .Select(v => v.Trim())
                                                    .Where(v => !string.IsNullOrEmpty(v))
                                                    .ToArray(),
            };

            foreach (string key in keysAction.Keys)
            {
                if (values.ContainsKey(key))
                {
                    keysAction[key](key);
                }
            }
        }
    }
}

using System;
using System.Text;
using System.Collections.Generic;

namespace pryLogger.src.Log.Strategies.File
{
    /// <summary>
    /// Represents a connection configuration for file logging.
    /// </summary>
    public class FileConnection
    {
        /// <summary>
        /// Gets the connection string.
        /// </summary>
        public readonly string ConnectionString;

        private readonly Dictionary<string, string> Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets the maximum number of log files to retain.
        /// </summary>
        public int MaxFiles { get; private set; } = 7;

        /// <summary>
        /// Gets or sets the path for storing log files.
        /// </summary>
        public string Path { get; private set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date format used in log file names.
        /// </summary>
        public string DateFormat { get; private set; } = "ddMMyyyy";

        /// <summary>
        /// Initializes a new instance of the <see cref="FileConnection"/> class with a connection string.
        /// </summary>
        /// <param name="connectionString">The connection string for configuring file logging.</param>
        public FileConnection(string connectionString)
        {
            ConnectionString = connectionString.Trim();

            if (connectionString.Contains("="))
            {
                foreach (var keyValue in connectionString.Split(';'))
                {
                    if (string.IsNullOrEmpty(keyValue) || string.IsNullOrEmpty(keyValue))
                    {
                        continue;
                    }

                    var arrKeyValue = keyValue.Trim().Split('=');
                    Values.Add(key: arrKeyValue[0], value: arrKeyValue[1]);
                }

                if (Values.ContainsKey("path"))
                {
                    Path = Values["path"];
                }

                if (Values.ContainsKey("maxfiles"))
                {
                    MaxFiles = int.Parse(Values["maxfiles"]);
                }

                if (Values.ContainsKey("dateformat"))
                {
                    DateFormat = Values["dateformat"];
                }
            }
        }
    }
}

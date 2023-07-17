using System;
using System.Text;
using System.Collections.Generic;

namespace pryLogger.src.LogStrategies.File
{
    public class FileConnection
    {
        public readonly string ConnectionString;
        private readonly Dictionary<string, string> Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public int MaxFiles { get; private set; } = 7;
        public string Path { get; private set; } = string.Empty;

        public FileConnection(string connectionString)
        {
            this.ConnectionString = connectionString.Trim();

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
                    this.Path = Values["path"];
                }

                if (Values.ContainsKey("maxfiles")) 
                {
                    this.MaxFiles = int.Parse(Values["maxfiles"]);
                }
            }
        }
    }
}

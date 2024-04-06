using System;
using System.IO;

namespace pryLogger.src.Logger.Loggers.FileLogger
{
    public class FileConnectionString
    {
        public readonly string ConnectionString;

        public string FileName { get; set; }

        public int MaxLines { get; set; } = 1000;
        public EventType Events { get; set; } = EventType.Log;

        public static FileConnectionString FromConnectionString(string connectionString)
        {
            return new FileConnectionString(connectionString);
        }

        public FileConnectionString(string connectionString)
        {
            var values = connectionString.ToDictionary();
            ConnectionString = connectionString = connectionString.Trim();
            string path = values.ContainsKey("filename") ? values["filename"] : connectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            string fileName = Path.GetFileName(path);
            string directoryName = Path.GetDirectoryName(path);

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException($"fileName required");
            }

            directoryName = string.IsNullOrEmpty(directoryName) ? Environment.CurrentDirectory : directoryName;
            FileName = Path.Combine(directoryName, fileName);

            if (values != null)
            {
                if (values.ContainsKey("maxlines"))
                {
                    MaxLines = int.Parse(values["maxlines"]);
                }

                if (values.ContainsKey("events"))
                {
                    Events = values["events"].ParseEventType();
                }
            }
        }
    }
}

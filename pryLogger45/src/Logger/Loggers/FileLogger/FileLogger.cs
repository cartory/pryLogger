using System;
using System.IO;

namespace pryLogger.src.Logger.Loggers.FileLogger
{
    public sealed class FileLogger : ILogger
    {
        public EventType Events { get; set; }
        public FileConnectionString FileConnectionString { get; private set; }
        public string[] FileNames => new string[] { FileConnectionString.FileName };

        public FileLogger(string connectionString)
        {
            this.FileConnectionString = FileConnectionString.FromConnectionString(connectionString);
            this.Events = FileConnectionString.Events;
        }

        public static FileLogger FromConnectionString(string connectionString)
        {
            return new FileLogger(connectionString);
        }

        public void Log(LogEvent log, bool throwException = false)
        {
            try
            {
                string fileName = FileConnectionString.FileName;
                string newLine = $"[{log.Starts:s}] {log.ToJson()}";

                if (!File.Exists(fileName))
                {
                    File.WriteAllText(fileName, string.Empty);
                }
                else
                {
                    int fileLines = File.ReadAllLines(fileName)?.Length ?? 0;

                    if (fileLines > FileConnectionString.MaxLines)
                    {
                        File.WriteAllText(fileName, string.Empty);
                    }
                }

                File.AppendAllText(fileName, $"{newLine}{Environment.NewLine}");
                Console.WriteLine($"{nameof(FileLogger)} {FileConnectionString.FileName} OK");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"{nameof(FileLogger)} {FileConnectionString.FileName} ERROR {e.Message}");
                if (throwException) throw;
            }
        }
    }
}

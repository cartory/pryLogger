using System;
using System.IO;
using System.Text;

using pryLogger.src.Attributes;
using pryLogger.src.ErrorNotifier;

namespace pryLogger.src.LogStrategies.File
{
    public partial class FileLog : LogAttribute
    {
        public static FileLog New => GetInstance<FileLog>();
        public static FileConnection Connection { get; private set; }

        public FileLog() : base() { }
        public FileLog(string onExceptionName) : base(onExceptionName) { }

        public static void SetConnectionString(string connectionString) => Connection = new FileConnection(connectionString);

        public override void LogAndNotify(LogEvent logEvent)
        {
            string fileName = string.Empty;

            try
            {
                string dirPath = Environment.CurrentDirectory;
                string logLine = $"[{logEvent.Start:s}] {logEvent.ToJson()}";

                if (Path.IsPathRooted(Connection.Path)) 
                {
                    dirPath = Connection.Path;
                }

                dirPath = $"{dirPath}\\logs";
                fileName = $"{dirPath}\\{DateTimeOffset.Now:ddMMyyyy}.log";

                if (!Directory.Exists(dirPath)) 
                {
                    Directory.CreateDirectory(dirPath);
                }

                var dirFiles = Directory.GetFiles(dirPath);

                if (dirFiles.Length > Connection.MaxFiles) 
                {
                    foreach (var dirFile in dirFiles)
                    {
                        System.IO.File.Delete(dirFile);
                    }
                }

                var fileStream = !System.IO.File.Exists(fileName) 
                    ? System.IO.File.Create(fileName)
                    : new FileStream(fileName, FileMode.Append, FileAccess.Write);

                using (fileStream)
                {
                    var bytes = Encoding.UTF8.GetBytes(logLine + Environment.NewLine);
                    fileStream.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"errorOnLog : {e.Message}");
            }

            try
            {
                if (ErrorNotifier != null) 
                { 
                    if (logEvent.HasError(out LogEvent errLog)) 
                    {
                        ErrorNotifier
                            .SetAttachMent(fileName)
                            .Notify(ErrorNotification.FromLogEvent(logEvent, errLog));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"errorOnNotify : {e.Message}");
            }
        }
    }
}

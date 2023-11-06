using System;
using System.IO;
using System.Text;

using pryLogger.src.ErrorNotifier;
using pryLogger.src.Log.Attributes;

namespace pryLogger.src.Log.Strategies.File
{
    /// <summary>
    /// Represents a file-based logging strategy.
    /// </summary>
    public partial class FileLog : LogAttribute
    {
        /// <summary>
        /// Gets or sets the connection configuration for the file log.
        /// </summary>
        public static FileConnection Connection { get; private set; }

        /// <summary>
        /// Initializes a new instance of the FileLog class.
        /// </summary>
        public FileLog() : base() { }

        /// <summary>
        /// Initializes a new instance of the FileLog class with a custom exception name.
        /// </summary>
        /// <param name="onExceptionName">The custom exception name.</param>
        public FileLog(string onExceptionName) : base(onExceptionName) { }

        /// <summary>
        /// Sets the connection string for the file log.
        /// </summary>
        /// <param name="connectionString">The connection string for the file log.</param>
        public static void SetConnectionString(string connectionString) => Connection = new FileConnection(connectionString);

        /// <summary>
        /// Logs a log event and notifies on errors.
        /// </summary>
        /// <param name="log">The log event to log and notify.</param>
        public override void LogAndNotify(LogEvent log)
        {
            string fileName = string.Empty;

            try
            {
                string dirPath = Environment.CurrentDirectory;
                string logLine = $"[{log.Starts:s}] {log.ToJson()}";

                if (Path.IsPathRooted(Connection.Path))
                {
                    dirPath = Connection.Path;
                }

                dirPath = $"{dirPath}\\logs";
                fileName = $"{dirPath}\\{DateTimeOffset.Now.ToString(Connection.DateFormat)}.log";

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
                if (log.HasError(out LogEvent errLog))
                {
                    var errNotifier = LocalMailErrorNotifier ?? MailErrorNotifier;

                    errNotifier?
                        .SetAttachMent(fileName)
                        .Notify(ErrorNotification.FromLogEvent(log, errLog.Method));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"errorOnNotify : {e.Message}");
            }
        }
    }
}

using System;
using System.Linq;

using pryLogger.src.ErrorNotifier;
using pryLogger.src.Log.Attributes;

using LiteDB;

namespace pryLogger.src.Log.Strategies.Lite
{
    /// <summary>
    /// Provides a logging strategy using LiteDB as the storage backend.
    /// </summary>
    public class LiteLog : LogAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LiteLog"/> class.
        /// </summary>
        public LiteLog() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LiteLog"/> class with a specified exception handling method.
        /// </summary>
        /// <param name="onExceptionName">The name of the exception handling method.</param>
        public LiteLog(string onExceptionName) : base(onExceptionName) { }

        /// <summary>
        /// Gets or sets the LiteDB logging settings.
        /// </summary>
        public static LiteSettings Settings { get; private set; }

        /// <summary>
        /// Logs a log event and notifies in case of errors.
        /// </summary>
        /// <param name="log">The log event to be logged.</param>
        public override void LogAndNotify(LogEvent log)
        {
            try
            {
                using (var lite = new LiteDatabase(Settings.Connection))
                {
                    var logs = lite.GetCollection<LogEvent>("logs");

                    if (logs.Count() > Settings.MaxCount)
                    {
                        logs.DeleteAll();
                    }

                    logs.Insert(log);
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
                        .SetAttachMent(Settings.Connection.Filename)
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

using System;
using System.Linq;
using pryLogger.src.ErrorNotifier;

using LiteDB;
using pryLogger.src.Log.Attributes;

namespace pryLogger.src.Log.Strategies.LiteDb
{
    public class LiteLog : LogAttribute
    {
        public LiteLog() : base() { }
        public LiteLog(string onExceptionName) : base(onExceptionName) { }

        public static LiteSettings Settings { get; private set; }

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
                    ErrorNotifier?
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

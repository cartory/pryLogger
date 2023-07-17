using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using ArxOne.MrAdvice.Advice;
using pryLogger.src.Attributes;
using pryLogger.src.ErrorNotifier;

namespace pryLogger.src.LogStrategies
{
    public partial class ConsoleLog : LogAttribute
    {
        public static ConsoleLog New => GetInstance<ConsoleLog>();

        public ConsoleLog() : base() { }
        public ConsoleLog(string onExceptionName) : base(onExceptionName) { }

        public override void LogAndNotify(LogEvent logEvent)
        {
            try
            {
                Console.WriteLine($"[{logEvent.Start:s}] {logEvent.ToJson()}");

                if (ErrorNotifier != null) 
                { 
                    if (logEvent.HasError(out LogEvent errLog)) 
                    {
                        ErrorNotifier.Notify(ErrorNotification.FromLogEvent(logEvent, errLog));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"errorOnLogAndNotify : {e.Message}");
            }
        }
    }
}

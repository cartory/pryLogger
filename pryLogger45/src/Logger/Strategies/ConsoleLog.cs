using System;
using System.Text;

using pryLogger.src.ErrorNotifier;
using pryLogger.src.Log.Attributes;

namespace pryLogger.src.Log.Strategies
{
    /// <summary>
    /// A logging strategy that logs events to the console.
    /// </summary>
    public partial class ConsoleLog : LogAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLog"/> class.
        /// </summary>
        public ConsoleLog() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLog"/> class with an exception handler method.
        /// </summary>
        /// <param name="onExceptionName">The name of the exception handler method to call when an exception occurs.</param>
        public ConsoleLog(string onExceptionName) : base(onExceptionName) { }

        /// <summary>
        /// Logs the specified <paramref name="log"/> event to the console and notifies an error if present.
        /// </summary>
        /// <param name="log">The log event to be logged.</param>
        public override void LogAndNotify(LogEvent log)
        {
            try
            {
                Console.WriteLine($"[{log.Starts:s}] {log.ToJson()}");

                if (log.HasError(out LogEvent errLog))
                {
                    var errNotifier = LocalMailErrorNotifier ?? MailErrorNotifier;
                    errNotifier?.Notify(ErrorNotification.FromLogEvent(log, errLog.Method));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"errorOnLogAndNotify : {e.Message}");
            }
        }
    }
}

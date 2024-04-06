using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace pryLogger.src.Logger.Loggers
{
    public sealed class ConsoleLogger : ILogger
    {
        public static readonly ConsoleLogger Instance = new ConsoleLogger();

        public string[] FileNames => new string[] { };
        public EventType Events { get; set; } = EventType.All;

        private ConsoleLogger() { }

        public void Log(LogEvent log, bool throwException = false)
        {
            Console.WriteLine($"[{log.Starts:s}] {log.ToJson()}");
        }
    }
}

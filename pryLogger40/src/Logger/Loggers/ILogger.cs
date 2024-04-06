using System;
using System.Linq;
using System.Text;

namespace pryLogger.src.Logger.Loggers
{
    public interface ILogger
    {
        string[] FileNames { get; }
        EventType Events { get; set; }

        void Log(LogEvent log, bool throwException = false);
    }
}

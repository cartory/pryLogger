using System;
using System.Linq;
using System.Collections.Generic;

using pryLogger.src.Db;
using pryLogger.src.Rest;

namespace pryLogger.src.Logger
{
    public static class LogEventExtensions
    {
        private static readonly Dictionary<EventType, string> stringTypes = new Dictionary<EventType, string>()
        {
            [EventType.Db] = "db",
            [EventType.Log] = "log",
            [EventType.All] = "all",
            [EventType.Rest] = "rest",
            [EventType.None] = "none",
        };

        private static readonly Dictionary<Type, Func<EventType, bool>> classFlags = new Dictionary<Type, Func<EventType, bool>>()
        {
            [typeof(DbEvent)] = e => e.HasFlag(EventType.Db),
            [typeof(LogEvent)] = e => !e.HasFlag(EventType.None),
            [typeof(RestEvent)] = e => e.HasFlag(EventType.Rest),
        };

        public static LogEvent FilterByEventType(this LogEvent log, EventType events)
        {
            var tmpLog = LogEvent.FromLog(log);

            if (events.HasFlag(EventType.None))
            {
                tmpLog.Events = null;
            }
            else 
            { 
                if (!events.HasFlag(EventType.All))
                {
                    tmpLog.Events = log.Events?
                        .Where(e =>
                        {
                            var type = e.GetType();
                            return classFlags.ContainsKey(type) && classFlags[type](events);
                        })
                        .Select(e => e is LogEvent innerLog ? innerLog.FilterByEventType(events) : e)
                        .ToList();
                }
            }

            return tmpLog;
        }

        public static EventType ParseEventType(this string eventTypeString)
        {
            EventType eventType = EventType.Log;

            var arrStringTypes = eventTypeString.Trim()
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(v => v.Trim())
                .Where(v => !string.IsNullOrEmpty(v))
                .Distinct().Select(t => t.ToLower()).ToArray();

            foreach (string stringType in arrStringTypes)
            {
                foreach (EventType type in stringTypes.Keys)
                {
                    if (stringType.Equals(stringTypes[type]))
                    {
                        if (!type.HasFlag(EventType.Log))
                        {
                            eventType |= type;
                        }
                    }
                }

                if (eventType.HasFlag(EventType.None)) 
                {
                    eventType = EventType.None;
                    break;
                }

                if (eventType.HasFlag(EventType.All))
                {
                    eventType = EventType.All;
                    break;
                }
            }

            return eventType;
        }
    }
}

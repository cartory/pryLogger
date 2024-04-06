using System;
using System.Linq;
using System.Text;

using System.Globalization;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace pryLogger.src.Logger
{
    public class Error
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("stackTrace")]
        public string[] StackTrace { get; set; }

        public Error(string message, string[] stackTrace)
        {
            this.Message = message;
            this.StackTrace = stackTrace;
        }

        public static Error FromException(Exception e)
        {
            string[] stackTrace = e.StackTrace?
                .Split(new String[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(st => st.Trim().Substring(3))
                .Where(st => !string.IsNullOrEmpty(st))
                .Where(st => !Regex.IsMatch(st.ToLower(), @"^(arxone|prylogger)\."))
                .ToArray();

            return new Error(e.Message, stackTrace);
        }
    }

    public class LogEvent : Event
    {
        public override DateTimeOffset Starts { get; set; }
        public override double TotalMilliseconds { get; set; }

        [JsonProperty("methodName")]
        public string MethodName { get; set; }

        [JsonIgnore]
        public string[] StackTrace { get; private set; }

        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Params { get; set; }

        [JsonProperty("events", NullValueHandling = NullValueHandling.Ignore)]
        public List<Event> Events { get; set; }

        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public Error Error { get; set; }

        [JsonProperty("returns", NullValueHandling = NullValueHandling.Ignore)]
        public object Returns { get; set; }

        public List<Event> GetEvents()
        {
            if (Events == null) 
            {
                Events = new List<Event>();
            }

            return Events;
        }

        public void SetException(Exception e) => Error = Error.FromException(e);

        public List<LogEvent> GetInnerLogs()
        {
            return this.Events?
                .Where(e => e is LogEvent)
                .Select(e => (LogEvent)e)
                .ToList();
        }

        public LogEvent(string methodName) => this.MethodName = methodName;

        public LogEvent(string methodName, string[] StackTrace)
        {
            this.MethodName = methodName;
            this.StackTrace = StackTrace;
        }

        public LogEvent(string methodName, string stackTrace)
        {
            this.MethodName = methodName;
            this.StackTrace = stackTrace
                .Split(new String[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(st => st.Trim().Substring(3))
                .Where(st => !Regex.IsMatch(st, @"(:line \d+|Object\[\] \))$", RegexOptions.IgnoreCase))
                .Where(st => !Regex.IsMatch(st, @"^(system\.environment|arxone|prylogger)\.", RegexOptions.IgnoreCase))
                .ToArray();
        }

        public bool HasError(out LogEvent errLog)
        {
            errLog = null;
            bool hasError = Error != null;

            if (hasError)
            {
                errLog = this;
            }
            else
            {
                var innerLogs = this.GetInnerLogs();

                if (innerLogs != null)
                {
                    foreach (var inner in innerLogs)
                    {
                        hasError = inner.HasError(out errLog);

                        if (hasError)
                        {
                            if (errLog != null) 
                            { 
                                errLog = this;
                            }

                            break;
                        }
                    }
                }
            }

            return hasError;
        }

        public LogEvent GetFather(LogEvent child)
        {
            LogEvent logFather = null;
            string currStackTrace = this.StackTrace?.FirstOrDefault();

            if (child.StackTrace?.Length > 1 && !string.IsNullOrEmpty(currStackTrace))
            {
                for (int index = 1; index < child.StackTrace.Length; index++)
                {
                    if (child.StackTrace[index].StartsWith(currStackTrace))
                    {
                        if (index < 2)
                        {
                            logFather = this;
                            break;
                        }
                        else
                        {
                            var innerLogs = this.GetInnerLogs();
                            if (innerLogs == null) continue;

                            foreach (var inner in innerLogs)
                            {
                                logFather = inner.GetFather(child);
                                if (logFather != null) break;
                            }

                            if (logFather != null) break;
                        }
                    }
                }
            }

            return logFather;
        }

        public static LogEvent FromJson(string json) => JsonConvert.DeserializeObject<LogEvent>(json, Converter.Settings);
        public string ToJson(Formatting formatting = Formatting.None) => JsonConvert.SerializeObject(this, formatting, Converter.Settings);

        public static LogEvent FromLog(LogEvent log)
        {
            return new LogEvent(log.MethodName, log.StackTrace)
            {
                Error = log.Error,
                Starts = log.Starts,
                Events = log.Events,
                Params = log.Params,
                Returns = log.Returns,
                TotalMilliseconds = log.TotalMilliseconds,
            };
        }
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings()
        {
            DateParseHandling = DateParseHandling.None,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}

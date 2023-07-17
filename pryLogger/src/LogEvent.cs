using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace pryLogger.src
{
    public partial class LogEvent : IEvent
    {
        public LogEvent(string method) => this.Method = method;

        [JsonProperty("start")]
        public DateTimeOffset Start { get; set; } = DateTimeOffset.Now;

        [JsonProperty("elapsedTime")]
        public double ElapsedTime { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("params")]
        public Dictionary<string, object> Params { get; set; }

        [JsonProperty("events")]
        public List<IEvent> Events { get; set; }

        [JsonProperty("error")]
        public Error Error { get; set; }

        [JsonProperty("returns")]
        public object Returns { get; set; }

        public List<IEvent> GetEvents()
        {
            Events = Events ?? new List<IEvent>();
            return Events;
        }

        public List<LogEvent> GetInnerLogs()
        {
            return Events?
                .Where(e => e is LogEvent)
                .Select(e => (LogEvent)e)
                .ToList();
        }

        public void SetParams(params object[] args)
        {
            if (args.Length < 1) return;

            this.Params = args
                .Select((arg, index) => new { index, arg })
                .ToDictionary(x => $"p{x.index}", x => args[x.index]);
        }

        public void SetException(Exception e) => this.Error = Error.FromException(e);

        public void Finish()
        {
            var diff = DateTimeOffset.Now - Start;
            this.ElapsedTime = diff.TotalMilliseconds;
        }
    }

    public partial class Error
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("stackTrace")]
        public string[] StackTrace { get; set; }

        public static Error FromException(Exception e)
        {
            var arrStackTrace = e.StackTrace
                .Split(new String[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(st => st.Trim().Substring(3))
                .Where(st => !string.IsNullOrEmpty(st))
                .ToArray();

            return new Error()
            {
                Message = e.Message,
                StackTrace = arrStackTrace
            };
        }
    }

    public partial class LogEvent
    {
        [JsonIgnore]
        public string[] StackTrace { get; private set; }

        public LogEvent(string method, string stackTrace)
        {
            this.Method = method;
            this.StackTrace = stackTrace
                .Split(new String[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(st => st.Trim().Substring(3))
                .Where(st => !string.IsNullOrEmpty(st))
                .Where(st => !Regex.IsMatch(st, @"(:line \d+|Object\[\] \))$"))
                .Where(st => !Regex.IsMatch(st.ToLower(), "^(system.|arxone.|prylogger.)"))
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
                var innerLogs = GetInnerLogs();

                if (innerLogs != null)
                {
                    foreach (var inner in innerLogs)
                    {
                        hasError = inner.HasError(out errLog);

                        if (hasError)
                        {
                            errLog = errLog ?? this;
                            break;
                        }
                    }
                }
            }

            return hasError;
        }

        public LogEvent GetFather(LogEvent child, bool isLambdaLog = false)
        {
            LogEvent logFather = null;
            string currStackTrace = this.StackTrace.FirstOrDefault();

            if (isLambdaLog)
            {
                if (child.StackTrace.FirstOrDefault()?.StartsWith(currStackTrace) ?? false)
                {
                    return this;
                }
            }

            if (child.StackTrace.Length > 1)
            {
                for (int index = 1; index < child.StackTrace.Length; index++)
                {
                    if (child.StackTrace[index].StartsWith(currStackTrace))
                    {
                        if (index < 2 && !isLambdaLog)
                        {
                            logFather = this;
                            break;
                        }
                        else
                        {
                            var innerLogs = GetInnerLogs();
                            if (innerLogs == null) continue;

                            foreach (var inner in innerLogs)
                            {
                                logFather = inner.GetFather(child, isLambdaLog);
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
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
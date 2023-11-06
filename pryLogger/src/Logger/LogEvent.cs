using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace pryLogger.src
{
    /// <summary>
    /// Represents a log event, including information about method execution and errors.
    /// </summary>
    public partial class LogEvent : IEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogEvent"/> class with a method name.
        /// </summary>
        /// <param name="method">The name of the method.</param>
        public LogEvent(string method) => this.Method = method;

        /// <summary>
        /// Gets or sets the elapsed time in milliseconds for the log event.
        /// </summary>
        [JsonProperty("elapsedTime")]
        public double ElapsedTime { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the log event started.
        /// </summary>
        [JsonProperty("starts")]
        public DateTimeOffset Starts { get; set; }

        /// <summary>
        /// Gets or sets the name of the method associated with the log event.
        /// </summary>
        [JsonProperty("method")]
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets the parameters associated with the log event.
        /// </summary>
        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Params { get; set; }

        /// <summary>
        /// Gets or sets a list of child events associated with the log event.
        /// </summary>
        [JsonProperty("events", NullValueHandling = NullValueHandling.Ignore)]
        public List<IEvent> Events { get; set; }

        /// <summary>
        /// Gets or sets information about errors associated with the log event.
        /// </summary>
        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public Error Error { get; protected set; }

        /// <summary>
        /// Gets or sets the return value of the log event.
        /// </summary>
        [JsonProperty("returns", NullValueHandling = NullValueHandling.Ignore)]
        public object Returns { get; set; }

        /// <summary>
        /// Gets a list of inner logs within the current log event.
        /// </summary>
        [JsonIgnore]
        public List<LogEvent> InnerLogs
        {
            get
            {
                return this.Events?
                    .Where(e => e is LogEvent)
                    .Select(e => e as LogEvent)
                    .ToList();
            }
        }

        /// <summary>
        /// Sets the exception information for the log event.
        /// </summary>
        /// <param name="e">The exception to set.</param>
        public void SetExcepcion(Exception e) => Error = Error.FromException(e);

        /// <summary>
        /// Gets the list of events associated with the log event.
        /// </summary>
        /// <returns>A list of events.</returns>
        public List<IEvent> GetEvents() => Events = Events ?? new List<IEvent>();

        /// <summary>
        /// Sets the parameters for the log event.
        /// </summary>
        /// <param name="args">The parameters to set.</param>
        public void SetParams(params object[] args)
        {
            if (args.Length < 1) return;

            this.Params = args
                .Select((arg, index) => new { index, arg })
                .ToDictionary(x => $"p{x.index}", x => args[x.index]);
        }

        /// <summary>
        /// Marks the start of the log event.
        /// </summary>
        public void Start() => Starts = DateTimeOffset.Now;

        /// <summary>
        /// Marks the finish of the log event and calculates the elapsed time.
        /// </summary>
        public void Finish()
        {
            TimeSpan diff = DateTimeOffset.Now - Starts;
            ElapsedTime = diff.TotalMilliseconds;
        }
    }

    /// <summary>
    /// Represents error information associated with a log event.
    /// </summary>
    public partial class Error
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the stack trace associated with the error.
        /// </summary>
        [JsonProperty("stackTrace")]
        public string[] StackTrace { get; set; }

        /// <summary>
        /// Creates an <see cref="Error"/> object from an exception.
        /// </summary>
        /// <param name="e">The exception to create the error from.</param>
        /// <returns>An <see cref="Error"/> object representing the exception.</returns>
        public static Error FromException(Exception e)
        {
            var arrStackTrace = e.StackTrace
                .Split(new String[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(st => st.Trim().Substring(3))
                .Where(st => !string.IsNullOrEmpty(st))
                .Where(st => !Regex.IsMatch(st.ToLower(), @"^(arxone|prylogger)\."))
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
        /// <summary>
        /// Gets or sets the stack trace associated with the log event.
        /// </summary>
        [JsonIgnore]
        public string[] StackTrace { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEvent"/> class with a method name and an array of stack trace lines.
        /// </summary>
        /// <param name="method">The name of the method.</param>
        /// <param name="stackTrace">An array of stack trace lines associated with the log event.</param>
        public LogEvent(string method, string[] stackTrace)
        {
            this.Method = method;
            this.StackTrace = stackTrace;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEvent"/> class with a method name and a stack trace string.
        /// </summary>
        /// <param name="method">The name of the method.</param>
        /// <param name="stackTrace">A string containing the stack trace associated with the log event.</param>
        public LogEvent(string method, string stackTrace)
        {
            this.Method = method;
            this.StackTrace = stackTrace
                .Split(new String[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(st => st.Trim().Substring(3))
                .Where(st => !Regex.IsMatch(st, @"(:line \d+|Object\[\] \))$"))
                .Where(st => !Regex.IsMatch(st.ToLower(), @"^(system\.environment|arxone|prylogger)\."))
                .ToArray();
        }

        /// <summary>
        /// Checks if the log event or any of its child events contain an error.
        /// </summary>
        /// <param name="errLog">If an error is found, this will be set to the log event containing the error.</param>
        /// <returns>True if an error is found, otherwise false.</returns>
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
                var innerLogs = InnerLogs;
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

        /// <summary>
        /// Retrieves the parent log event of a given child log event.
        /// </summary>
        /// <param name="child">The child log event to find the parent for.</param>
        /// <returns>The parent log event, or null if not found.</returns>
        public LogEvent GetFather(LogEvent child)
        {
            LogEvent logFather = null;
            string currStackTrace = this.StackTrace.FirstOrDefault();

            if (child.StackTrace.Length > 1)
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
                            var innerLogs = InnerLogs;
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

        /// <summary>
        /// Deserializes a JSON string into a <see cref="LogEvent"/> object.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A <see cref="LogEvent"/> object deserialized from the JSON string.</returns>
        public static LogEvent FromJson(string json) => JsonConvert.DeserializeObject<LogEvent>(json, Converter.Settings);

        /// <summary>
        /// Serializes the current <see cref="LogEvent"/> object to a JSON string.
        /// </summary>
        /// <param name="formatting">The formatting style to apply to the JSON string.</param>
        /// <returns>A JSON string representing the current <see cref="LogEvent"/> object.</returns>
        public string ToJson(Formatting formatting = Formatting.None) => JsonConvert.SerializeObject(this, formatting, Converter.Settings);
    }

    /// <summary>
    /// A utility class for providing JSON serialization settings.
    /// </summary>
    internal static class Converter
    {
        /// <summary>
        /// Gets the JSON serialization settings used by the application.
        /// </summary>
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            // Do not parse dates during deserialization.
            DateParseHandling = DateParseHandling.None,
            // Ignore metadata properties during serialization.
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            // Register a custom converter for handling ISO formatted dates with universal DateTime styles.
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
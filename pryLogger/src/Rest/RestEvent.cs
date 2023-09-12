using System;
using System.Text;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace pryLogger.src.Rest
{
    /// <summary>
    /// Extension methods for working with RestEvent objects.
    /// </summary>
    internal static class RestLogEventExtensions
    {
        /// <summary>
        /// Adds a RestEvent to the list of events associated with a LogEvent.
        /// </summary>
        /// <param name="log">The LogEvent to which the RestEvent will be added.</param>
        /// <param name="rest">The RestEvent to be added.</param>
        public static void AddRestEvent(this LogEvent log, RestEvent rest) => log.GetEvents().Add(rest);
    }

    /// <summary>
    /// Represents an event related to a REST request and response.
    /// </summary>
    public class RestEvent : IEvent
    {
        /// <summary>
        /// Gets or sets the elapsed time (in milliseconds) for the REST event.
        /// </summary>
        [JsonProperty("elapsedTime")]
        public double ElapsedTime { get; set; }

        /// <summary>
        /// Gets or sets the start time of the REST event.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset Starts { get; set; }

        /// <summary>
        /// Gets or sets the REST request associated with this event.
        /// </summary>
        [JsonProperty("request")]
        public RestRequest Request { get; set; }

        /// <summary>
        /// Gets or sets the REST response associated with this event.
        /// </summary>
        [JsonProperty("response")]
        public RestResponse Response { get; set; }

        /// <summary>
        /// Initializes the REST event and starts the timer.
        /// </summary>
        public void Start() => Starts = DateTimeOffset.Now;

        /// <summary>
        /// Finishes the REST event, calculating the elapsed time.
        /// </summary>
        public void Finish()
        {
            TimeSpan diff = DateTimeOffset.Now - Starts;
            ElapsedTime = diff.TotalMilliseconds;
        }

        /// <summary>
        /// Finishes the REST event and associates it with a specific request and response.
        /// </summary>
        /// <param name="req">The REST request associated with this event.</param>
        /// <param name="res">The REST response associated with this event.</param>
        public void Finish(RestRequest req, RestResponse res)
        {
            Request = req;
            Response = res;
            Finish();
        }
    }
}

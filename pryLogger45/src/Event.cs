using System;
using Newtonsoft.Json;

namespace pryLogger.src
{
    public abstract class Event
    {
        [JsonProperty("starts")]
        public abstract DateTimeOffset Starts { get; set; }

        [JsonProperty("totalMilliseconds")]
        public abstract double TotalMilliseconds { get; set; }

        public virtual void Start() => Starts = DateTimeOffset.Now;

        public virtual void Stop()
        {
            TimeSpan diff = DateTimeOffset.Now - this.Starts;
            this.TotalMilliseconds = diff.TotalMilliseconds;
        }
    }
}

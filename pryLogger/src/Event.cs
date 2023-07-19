using System;
using System.Text;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace pryLogger.src
{
    public abstract class Event
    {
        [JsonProperty("starts")]
        public DateTimeOffset Starts { get; protected set; }

        [JsonProperty("elapsedTime")]
        public double ElapsedTime { get; protected set; }

        public virtual void Start() => Starts = DateTimeOffset.Now;

        public virtual void Finish() 
        { 
            TimeSpan diff = DateTimeOffset.Now - Starts;
            ElapsedTime = diff.TotalMilliseconds;
        }
    }
}

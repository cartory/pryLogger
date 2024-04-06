using System;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace pryLogger.src.Rest
{
    public class RestEvent : Event
    {
        public override DateTimeOffset Starts { get; set; }
        public override double TotalMilliseconds { get; set; }

        [JsonProperty("request", NullValueHandling = NullValueHandling.Ignore)]
        public RestRequest Request { get; set; }

        [JsonProperty("response", NullValueHandling = NullValueHandling.Ignore)]
        public RestResponse Response { get; set; }

        public void Stop(RestRequest request, RestResponse response)
        {
            this.Request = request;
            this.Response = response;
            this.Stop();
        }
    }
}

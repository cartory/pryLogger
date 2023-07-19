using System;
using System.Text;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace pryLogger.src.Rest
{
    internal static class RestLogEventExtensions
    {
        public static void AddRestEvent(this LogEvent log, RestEvent rest) => log.GetEvents().Add(rest);
    }

    public class RestEvent : Event
    {
        [JsonProperty("request")]
        public RestRequest Request { get; set; }

        [JsonProperty("response")]
        public RestResponse Response { get; set; }

        public void Finish(RestRequest req, RestResponse res)
        {
            Request = req; 
            Response = res;
            Finish();
        }
    }
}

using System;
using System.IO;
using System.Net;

using Newtonsoft.Json;

namespace pryLogger.src.Rest
{
    public class RestResponse
    {
        public static implicit operator RestResponse(HttpWebResponse response) => new RestResponse(response);

        [JsonIgnore]
        public HttpWebResponse Response { get; private set; }

        [JsonProperty("statusCode")]
        public HttpStatusCode StatusCode { get; set; }

        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public string Content { get; set; }

        [JsonProperty("errMessage", NullValueHandling = NullValueHandling.Ignore)]
        public string ErrMessage { get; set; }

        internal RestResponse() { }

        public RestResponse(HttpWebResponse response)
        {
            Response = response;
            StatusCode = response.StatusCode;

            using (var resStream = Response.GetResponseStream())
            {
                using (var reader = new StreamReader(resStream))
                {
                    Content = reader.ReadToEnd();
                }
            }
        }
    }
}
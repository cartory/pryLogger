using System;
using System.Net;

using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;
using System.IO;

namespace pryLogger.src.Rest
{
    public class RestRequest
    {
        public static implicit operator RestRequest(HttpWebRequest request) => new RestRequest(request);

        [JsonIgnore]
        public HttpWebRequest Request { get; private set; }

        [JsonIgnore]
        public Action<HttpWebRequest> OnRequest { get; set; }

        [JsonProperty("method")]
        public string Method { get => Request.Method; set => Request.Method = value; }

        [JsonProperty("contentType", NullValueHandling = NullValueHandling.Ignore)]
        public string ContentType { get; set; } = "application/json";

        [JsonProperty("url")]
        public string Url { get => Request.RequestUri.AbsoluteUri; }

        [JsonProperty("headers", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Headers { get; set; }

        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public string Content { get; set; }

        public RestRequest(HttpWebRequest request) => Request = request;

        public RestRequest(string url) => Request = (HttpWebRequest)WebRequest.Create(url);

        public RestRequest(string url, Dictionary<string, object> @params)
        {
            string query = string.Join("&", @params?.Select(p =>
            {
                return $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value.ToString())}";
            }));

            Request = (HttpWebRequest)WebRequest.Create($"{url}?{query}");
        }
    }
}
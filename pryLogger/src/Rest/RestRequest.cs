using System;
using System.Net;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace pryLogger.src.Rest
{
    public class RestRequest
    {
        private string stringContent;
        public static implicit operator RestRequest(HttpWebRequest request) => new RestRequest(request);

        [JsonIgnore]
        public HttpWebRequest Request { get; private set; }

        [JsonProperty("method")]
        public string Method { get => Request.Method; set => Request.Method = value; }
        
        [JsonProperty("url")]
        public string Url { get => Request.RequestUri.AbsoluteUri; }

        [JsonProperty("headers", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Headers { get; set; }

        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public string Content
        {
            get => stringContent;
            set
            {
                stringContent = value;
                var bytes = Encoding.UTF8.GetBytes(stringContent);

                using (var reqStream = Request.GetRequestStream())
                {
                    Request.ContentLength = bytes.Length;
                    reqStream.Write(bytes, 0, bytes.Length);
                }
            }
        }

        private RestRequest(HttpWebRequest request) => Request = request;
        public RestRequest(string url) => Request = WebRequest.CreateHttp(url);

        public RestRequest(string url, Dictionary<string, object> @params)
        {
            string query = string.Join("&", @params?.Select(p =>
            {
                return $"{WebUtility.UrlEncode(p.Key)}={WebUtility.UrlEncode(p.Value.ToString())}";
            }));

            Request = WebRequest.CreateHttp($"{url}?{query}");
        }
    }

}

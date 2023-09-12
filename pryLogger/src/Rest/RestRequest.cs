using System;
using System.Net;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace pryLogger.src.Rest
{
    /// <summary>
    /// Represents a REST request that can be sent to a web service.
    /// </summary>
    public class RestRequest
    {
        /// <summary>
        /// Implicitly converts an HttpWebRequest to a RestRequest.
        /// </summary>
        /// <param name="request">The HttpWebRequest to convert.</param>
        /// <returns>A RestRequest instance.</returns>
        public static implicit operator RestRequest(HttpWebRequest request) => new RestRequest(request);

        /// <summary>
        /// Gets the underlying HttpWebRequest associated with this RestRequest.
        /// </summary>
        [JsonIgnore]
        public HttpWebRequest Request { get; private set; }

        /// <summary>
        /// Gets or sets the HTTP method for the request (e.g., GET, POST).
        /// </summary>
        [JsonProperty("method")]
        public string Method { get => Request.Method; set => Request.Method = value; }

        /// <summary>
        /// Gets or sets the content type of the request.
        /// </summary>
        [JsonProperty("contentType", NullValueHandling = NullValueHandling.Ignore)]
        public string ContentType { get; set; } = "application/json";

        /// <summary>
        /// Gets the URL of the request.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get => Request.RequestUri.AbsoluteUri; }

        /// <summary>
        /// Gets or sets the headers of the request.
        /// </summary>
        [JsonProperty("headers", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Headers { get; set; }

        /// <summary>
        /// Gets or sets the content of the request.
        /// </summary>
        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public string Content { get; set; }

        /// <summary>
        /// Initializes a new instance of the RestRequest class using an existing HttpWebRequest.
        /// </summary>
        /// <param name="request">The HttpWebRequest to wrap as a RestRequest.</param>
        private RestRequest(HttpWebRequest request) => Request = request;

        /// <summary>
        /// Initializes a new instance of the RestRequest class with the specified URL.
        /// </summary>
        /// <param name="url">The URL to create the request for.</param>
        public RestRequest(string url) => Request = WebRequest.CreateHttp(url);

        /// <summary>
        /// Initializes a new instance of the RestRequest class with the specified URL and parameters.
        /// </summary>
        /// <param name="url">The URL to create the request for.</param>
        /// <param name="params">The query parameters to include in the URL.</param>
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

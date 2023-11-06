using System;
using System.IO;
using System.Net;

using Newtonsoft.Json;

namespace pryLogger.src.Rest
{
    /// <summary>
    /// Represents a REST response received from a web service.
    /// </summary>
    public class RestResponse
    {
        /// <summary>
        /// Implicitly converts an HttpWebResponse to a RestResponse.
        /// </summary>
        /// <param name="response">The HttpWebResponse to convert.</param>
        /// <returns>A RestResponse instance.</returns>
        public static implicit operator RestResponse(HttpWebResponse response) => new RestResponse(response);

        /// <summary>
        /// Initializes a new instance of the RestResponse class.
        /// </summary>
        internal RestResponse() { }

        /// <summary>
        /// Gets the underlying HttpWebResponse associated with this RestResponse.
        /// </summary>
        [JsonIgnore]
        public HttpWebResponse Response { get; private set; }

        /// <summary>
        /// Gets the HTTP status code of the response.
        /// </summary>
        [JsonProperty("statusCode")]
        public HttpStatusCode StatusCode { get; private set; }

        /// <summary>
        /// Gets or sets the content of the response.
        /// </summary>
        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public string Content { get; private set; }

        /// <summary>
        /// Gets or sets the error message associated with the response (if any).
        /// </summary>
        [JsonProperty("errMessage", NullValueHandling = NullValueHandling.Ignore)]
        public string ErrMessage { get; set; }

        /// <summary>
        /// Sets the content of the response.
        /// </summary>
        /// <param name="content">The content to set.</param>
        internal void SetContent(string content) => Content = content;

        /// <summary>
        /// Sets the HTTP status code of the response.
        /// </summary>
        /// <param name="statusCode">The HTTP status code to set.</param>
        internal void SetStatusCode(HttpStatusCode statusCode) => StatusCode = statusCode;

        /// <summary>
        /// Initializes a new instance of the RestResponse class with an HttpWebResponse.
        /// </summary>
        /// <param name="response">The HttpWebResponse to wrap as a RestResponse.</param>
        private RestResponse(HttpWebResponse response)
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

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using pryLogger.src.Log.Attributes;

namespace pryLogger.src.Rest
{
    /// <summary>
    /// Represents a REST client for making HTTP requests.
    /// </summary>
    public class RestClient
    {
        private static readonly Regex regex = new Regex(@"\((\d{3})\)");

        /// <summary>
        /// Sends a REST request and returns the response.
        /// </summary>
        /// <param name="req">The REST request to send.</param>
        /// <returns>The REST response received from the server.</returns>
        public static RestResponse Fetch(RestRequest req)
        {
            return Fetch(req, rest => rest);
        }

        /// <summary>
        /// Sends a REST request and performs an action when the response is received.
        /// </summary>
        /// <param name="req">The REST request to send.</param>
        /// <param name="onResponse">The action to perform when the response is received.</param>
        public static void Fetch(RestRequest req, Action<RestResponse> onResponse)
        {
            Fetch(req, res =>
            {
                onResponse(res);
                return 0;
            });
        }

        /// <summary>
        /// Sends a REST request and processes the response with a specified function.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="req">The REST request to send.</param>
        /// <param name="onResponse">The function to process the response and return a result.</param>
        /// <returns>The result of processing the REST response.</returns>
        public static T Fetch<T>(RestRequest req, Func<RestResponse, T> onResponse)
        {
            RestEvent rest = new RestEvent();
            RestResponse res = new RestResponse();
            var bytes = Encoding.UTF8.GetBytes(req.Content);

            LogAttribute.CurrentLog?.AddRestEvent(rest);

            if (req.Headers != null)
            {
                foreach (var header in req.Headers)
                {
                    req.Request.Headers.Add(header.Key, header.Value.ToString());
                }
            }

            try
            {
                req.Request.ContentLength = bytes.Length;
                req.Request.ContentType = req.ContentType;

                using (var reqStream = req.Request.GetRequestStream())
                {
                    reqStream.Write(bytes, 0, bytes.Length);
                }

                rest.Start();
                res = (HttpWebResponse)req.Request.GetResponse();
            }
            catch (Exception e)
            {
                res.ErrMessage = e.Message;
                Match match = regex.Match(e.Message);

                if (match.Success)
                {
                    string statusCodeString = match.Groups[1].Value;

                    if (Enum.TryParse(statusCodeString, out HttpStatusCode statusCode))
                    {
                        res.SetStatusCode(statusCode);
                    }
                }
            }

            try
            {
                using (res.Response)
                {
                    var result = onResponse(res);
                    rest.Finish(req, res);

                    return result;
                }
            }
            catch (Exception)
            {
                rest.Finish(req, res);
                throw;
            }
        }
    }
}

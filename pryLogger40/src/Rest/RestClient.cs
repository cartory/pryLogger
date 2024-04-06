using System;
using System.Net;

using System.Text;
using System.Text.RegularExpressions;

using pryLogger.src.Logger.Attributes;

namespace pryLogger.src.Rest
{
    public class RestClient
    {
        private static readonly Regex regex = new Regex(@"\((\d{3})\)");

        public static void Fetch(RestRequest req, Action<RestResponse> onResponse)
        {
            Fetch(req, res =>
            {
                onResponse(res);
                return 0;
            });
        }

        public static T Fetch<T>(RestRequest req, Func<RestResponse, T> onResponse)
        {
            RestEvent rest = new RestEvent();
            RestResponse res = new RestResponse();

            try
            {
                req.Request.ContentType = req.ContentType;
                LogAttribute.Current?.GetEvents().Add(rest);

                if (req.Headers != null)
                {
                    foreach (var header in req.Headers)
                    {
                        req.Request.Headers.Add(header.Key, header.Value.ToString());
                    }
                }

                if (req.OnRequest != null)
                {
                    req.OnRequest.Invoke(req.Request);
                }
                else 
                { 
                    byte[] bytes = Encoding.UTF8.GetBytes(req.Content);
                    req.Request.ContentLength = bytes.Length;

                    using (var reqStream = req.Request.GetRequestStream())
                    {
                        reqStream.Write(bytes, 0, bytes.Length);
                    }
                }

                rest.Start();
                res = (HttpWebResponse)req.Request.GetResponse();

                using (res.Response)
                {
                    var result = onResponse(res);
                    rest.Stop(req, res);

                    return result;
                }
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
                        res.StatusCode = statusCode;
                    }
                }

                rest.Stop(req, res);
                throw;
            }
        }
    }
}
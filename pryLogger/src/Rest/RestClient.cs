using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Text.RegularExpressions;

using pryLogger.src.Log.Attributes;

namespace pryLogger.src.Rest
{
    public class RestClient
    {
        private static readonly Regex regex = new Regex(@"\((\d{3})\)");

        public static RestResponse Fetch(RestRequest req)
        {
            return Fetch(req, rest => rest);
        }

        public static void Fetch(RestRequest req, Action<RestResponse> callback)
        {
            Fetch(req, res =>
            {
                callback(res);
                return 0;
            });
        }

        public static T Fetch<T>(RestRequest req, Func<RestResponse, T> callback) 
        {
            RestEvent rest = new RestEvent();
            RestResponse res = new RestResponse();

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
                var result = callback(res);
                rest.Finish(req, res);
                return result;
            }
            catch (Exception)
            {
                rest.Finish(req, res);
                throw;
            }
        }
    }
}

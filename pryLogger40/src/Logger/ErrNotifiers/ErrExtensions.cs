using System;
using System.IO;

using System.Net;
using System.Text;

using System.Linq;
using System.Xml.Linq;

using pryLogger.src.Rest;

namespace pryLogger.src.Logger.ErrNotifiers
{
    internal static class ErrExtensions
    {
        public static bool IsXml(this string xml)
        {
            try
            {
                XDocument.Parse(xml);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void WriteString(this Stream stream, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static string ToHtml(this ErrNotification err)
        {
            string addressTableHtml = string
                .Join(string.Empty, err.IpAdresses.Select(ip => $"<tr><td><a href=\"{ip}\">{ip}</a></td></tr>"));

            var children = new string[]
            {
                $"<h2 class=\"errorTitle\">{err.Title}</h2>",
                $"<p><em>{err.Message}</em></p>",
                err.IpAdresses.Length > 0
                        ? $"<table><thead><tr><td><strong>Ip Adresses</strong></td></tr></thead><tbody>{addressTableHtml}</tbody></table>"
                        : string.Empty,
                string.IsNullOrEmpty(err.Repository)? string.Empty: $"<h4><a href=\"{err.Repository}\">Repositorio</a></h4>",
                $"<small><pre><code>{err.JsonError}</code></pre></small>",
            };

            return $"<html><head><meta charset=\"UTF-8\"></head><body>{string.Join(string.Empty, children)}</body></html>";
        }

        public static LogEvent HtmlEncode(this LogEvent log)
        {
            var tmp = LogEvent.FromLog(log);
            string returns = tmp.Returns?.ToString();

            if (returns?.IsXml() ?? false)
            {
                tmp.Returns = WebUtility.HtmlEncode(returns);
            }

            if (tmp.Params != null)
            {
                foreach (var param in tmp.Params)
                {
                    string value = param.Value?.ToString();

                    if (value?.IsXml() ?? false)
                    {
                        tmp.Params[param.Key] = WebUtility.HtmlEncode(value);
                    }
                }
            }

            if (tmp.Events != null)
            {
                for (int i = 0; i < tmp.Events.Count; i++)
                {
                    var ev = tmp.Events[i];

                    if (ev is LogEvent innerLog)
                    {
                        tmp.Events[i] = innerLog.HtmlEncode();
                    }

                    if (ev is RestEvent restEvent)
                    {
                        string reqStringContent = restEvent.Request.Content;
                        string resStringContent = restEvent.Response.Content;

                        if (reqStringContent?.IsXml() ?? false)
                        {
                            restEvent.Request.Content = WebUtility.HtmlEncode(reqStringContent);
                            tmp.Events[i] = restEvent;
                        }

                        if (resStringContent?.IsXml() ?? false)
                        {
                            restEvent.Response.Content = WebUtility.HtmlEncode(resStringContent);
                            tmp.Events[i] = restEvent;
                        }
                    }
                }
            }

            return tmp;
        }
    }
}

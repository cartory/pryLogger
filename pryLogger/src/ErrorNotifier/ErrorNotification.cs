using System;
using System.Net;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

namespace pryLogger.src.ErrorNotifier
{
    internal static class StringExtensions 
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

        public static string EncodeXml(this string xml) 
        {
            return xml?
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("'", "&apos;")
                .Replace(@"""", "&quot;");
        }

        public static void EncodeXml(this LogEvent log) 
        {
            if (log == null) return;

            var innerLogs = log.InnerLogs;
            string returns = log.Returns?.ToString();

            if (returns?.IsXml() ?? false)
            {
                log.Returns = returns.EncodeXml();
            }

            if (log?.Params != null) 
            {
                foreach (var param in log.Params)
                {
                    string value = param.Value?.ToString();

                    if (value?.IsXml() ?? false)
                    {
                        log.Params[param.Key] = value.EncodeXml();
                    }
                }
            }

            if (innerLogs != null) 
            {
                foreach (var inner in innerLogs)
                {
                    inner.EncodeXml();
                }
            }
        }
    }

    public class ErrorNotification
    {
        public string Title { get; set; }
        public string JsonError { get; set; }
        public string ErrorMessage { get; set; }

        public string Repository { get; set; }
        public string[] IpAdresses { get; set; }

        public static ErrorNotification FromLogEvent(LogEvent log, string errLocation)
        {
            log.EncodeXml();
            var ipAddresses = Dns.GetHostAddresses(Dns.GetHostName())
                    .Select(ip => ip.ToString())
                    .Where(ip => Regex.IsMatch(ip, @"^\d+\.\d+\.\d+\.\d+$"));

            return new ErrorNotification()
            {
                IpAdresses = ipAddresses.ToArray(),
                JsonError = log.ToJson(Formatting.Indented),
                Title = $"Error Detected At {errLocation}",
                ErrorMessage = $"At {Environment.CurrentDirectory}",
            };
        }

        public string ToHtml()
        {
            string html = string.Empty;

            if (!string.IsNullOrEmpty(Title))
            {
                html += (
                    $@"<h2 class=""errorTitle"">{Title}</h2>"
                );
            }

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                html += (
                    $@"<p><em>{ErrorMessage}</em></p>"
                );
            }

            if (IpAdresses?.Length > 0)
            {
                html += "<table><thead><tr><td><strong>Direccion Ip</strong></td></tr></thead><tbody>";

                foreach (string ipAddress in IpAdresses)
                {
                    html += $@"<tr><td><a href=""{ipAddress}"">{ipAddress}</a></td></tr>";
                }

                html += "</tbody></table>";
            }

            if (!string.IsNullOrEmpty(Repository))
            {
                html += (
                    @"<h4>" +
                        $@"<a href=""{Repository}"">Repositorio</a>" +
                    "</h4>"
                );
            }

            if (!string.IsNullOrEmpty(JsonError))
            {
                html += (
                    @"<small><pre>" +
                        $"<code>{JsonError}</code>" +
                    "</pre></small>"
                );
            }

            html = (
                $"<html><head><style></style></head><body>{html}</body></html>"
            );

            return html;
        }
    }
}

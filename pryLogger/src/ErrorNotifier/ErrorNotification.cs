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
    }

    public class ErrorNotification
    {
        public string Title { get; set; }
        public string JsonError { get; set; }
        public string ErrorMessage { get; set; }

        public string Repository { get; set; }
        public string[] IpAdresses { get; set; }

        public static ErrorNotification FromLogEvent(LogEvent log, LogEvent errLog)
        {
            var ipAddresses = Dns.GetHostAddresses(Dns.GetHostName())
                    .Select(ip => ip.ToString())
                    .Where(ip => Regex.IsMatch(ip, @"^\d+\.\d+\.\d+\.\d+$"));

            return new ErrorNotification()
            {
                IpAdresses = ipAddresses.ToArray(),
                JsonError = log.ToJson(Formatting.Indented),
                Title = $"Error Detected At {errLog.Method}",
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

            if (IpAdresses != null && IpAdresses.Length > 0)
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
                if (JsonError.Contains(@"returnValue"": """))
                {
                    int b = JsonError.LastIndexOf(@"""");
                    int a = JsonError.LastIndexOf(@""": """);

                    string returnValue = JsonError.Substring(a + 4, b - a - 4).Replace(@"\", "");

                    if (returnValue.IsXml())
                    {
                        returnValue = returnValue.Replace("&", "&amp;")
                            .Replace("<", "&lt;")
                            .Replace(">", "&gt;")
                            .Replace("'", "&apos;")
                            .Replace(@"""", "&quot;")
                            .Replace("iso - 8859 - 1", "iso-8859-1");

                        JsonError = JsonError
                                .Remove(a + 4, b - a - 4)
                                .Insert(a + 4, returnValue);
                    }
                }

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

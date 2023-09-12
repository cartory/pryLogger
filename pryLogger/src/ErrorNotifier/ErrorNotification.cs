using System;
using System.Net;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using pryLogger.src.Rest;

namespace pryLogger.src.ErrorNotifier
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Determines whether a string is a valid XML document.
        /// </summary>
        /// <param name="xml">The input string to check.</param>
        /// <returns>True if the string is a valid XML document; otherwise, false.</returns>
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

        /// <summary>
        /// Encodes XML content in a log event to prevent XML-related issues.
        /// </summary>
        /// <param name="log">The log event to encode.</param>
        public static void EncodeXml(this LogEvent log)
        {
            if (log == null) return;

            string returns = log?.Returns?.ToString();

            if (returns?.IsXml() ?? false)
            {
                log.Returns = WebUtility.HtmlEncode(returns);
            }

            if (log.Params != null)
            {
                foreach (var param in log.Params)
                {
                    string value = param.Value?.ToString();

                    if (value?.IsXml() ?? false)
                    {
                        log.Params[param.Key] = WebUtility.HtmlEncode(value);
                    }
                }
            }

            log.Events?.ForEach(e =>
            {
                if (e is LogEvent logEvent)
                {
                    logEvent.EncodeXml();
                }

                if (e is RestEvent restEvent)
                {
                    string reqContent = restEvent.Request?.Content;
                    string resContent = restEvent.Response?.Content;

                    if (reqContent?.IsXml() ?? false)
                    {
                        restEvent.Request.Content = WebUtility.HtmlEncode(reqContent);
                    }

                    if (resContent?.IsXml() ?? false)
                    {
                        restEvent.Response.SetContent(WebUtility.HtmlEncode(resContent));
                    }
                }
            });
        }
    }

    /// <summary>
    /// Represents an error notification.
    /// </summary>
    public class ErrorNotification
    {
        /// <summary>
        /// Gets or sets the title of the error notification.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the JSON representation of the error.
        /// </summary>
        public string JsonError { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the repository related to the error.
        /// </summary>
        public string Repository { get; set; }

        /// <summary>
        /// Gets or sets an array of IP addresses.
        /// </summary>
        public string[] IpAdresses { get; set; }

        /// <summary>
        /// Creates an ErrorNotification from a LogEvent and error location.
        /// </summary>
        /// <param name="log">The LogEvent containing the error information.</param>
        /// <param name="errLocation">The location where the error occurred.</param>
        /// <returns>An ErrorNotification instance.</returns>
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

        /// <summary>
        /// Converts the error notification to an HTML representation.
        /// </summary>
        /// <returns>An HTML representation of the error notification.</returns>
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

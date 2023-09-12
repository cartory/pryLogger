using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace pryLogger.src.Rest.Xml
{
    /// <summary>
    /// Provides XML declarations commonly used in SOAP requests and responses.
    /// </summary>
    public static class Declarations
    {
        /// <summary>
        /// Represents the UTF-8 XML declaration.
        /// </summary>
        public static readonly XDeclaration UTF8 = new XDeclaration("1.0", "utf-8", "yes");

        /// <summary>
        /// Represents the ISO-8859-1 XML declaration.
        /// </summary>
        public static readonly XDeclaration ISO_8859_1 = new XDeclaration("1.0", "iso-8859-1", "yes");
    }

    /// <summary>
    /// Provides utility methods for creating and parsing SOAP XML messages.
    /// </summary>
    public static class Soap
    {
        /// <summary>
        /// Creates a SOAP XML request message with a specified header and body.
        /// </summary>
        /// <param name="name">The name of the XML request.</param>
        /// <param name="header">The XML header object.</param>
        /// <param name="body">The XML body object.</param>
        /// <returns>A string representing the SOAP XML request message.</returns>
        public static string CreateXmlRequest(XName name, object header, object body)
        {
            return CreateXmlRequest(name, header, body, null);
        }

        /// <summary>
        /// Creates a SOAP XML request message with a specified header, body, and XML declaration.
        /// </summary>
        /// <param name="name">The name of the XML request.</param>
        /// <param name="header">The XML header object.</param>
        /// <param name="body">The XML body object.</param>
        /// <param name="declaration">The XML declaration for the request.</param>
        /// <returns>A string representing the SOAP XML request message.</returns>
        public static string CreateXmlRequest(XName name, object header, object body, XDeclaration declaration)
        {
            string request = header?.ToXml("soap:Header") ?? "<soap:Header/>";

            if (body == null)
            {
                request += $"<soap:Body/>";
            }
            else
            {
                string methodParams = Regex.Replace(body.ToXml("params"), @"^<(.*?)>(.*?)<\/\1>$", "$2");
                request += $@"<soap:Body><{name.LocalName} xmlns=""{name.NamespaceName}"">{methodParams}</{name.LocalName}></soap:Body>";
            }

            request = $@"<soap:Envelope xmlns:xsd=""http://www.w3.org/2001/XMLSchema""  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">{request}</soap:Envelope>";
            return $"{declaration}{request}";
        }

        /// <summary>
        /// Parses a SOAP XML response and extracts the content with the specified name.
        /// </summary>
        /// <param name="name">The name of the XML element to extract.</param>
        /// <param name="content">The SOAP XML response content.</param>
        /// <returns>The extracted content as an object.</returns>
        public static object FromXmlResponse(XName name, string content)
        {
            XElement bodyElement = XElement.Parse(content);
            XElement resElement = bodyElement.Descendants(name).FirstOrDefault();

            object result = null;
            var innerXml = resElement?.Descendants().FirstOrDefault()?.Value;

            if (innerXml != null)
            {
                innerXml = Regex.Replace(innerXml, @"^<\?xml.*\?>", string.Empty);

                if (innerXml.IsXml(out XDocument doc))
                {
                    doc.Descendants().Attributes().Remove();
                    string json = JsonConvert.SerializeXNode(doc);
                    result = JObject.Parse(json).ToObject<Dictionary<string, object>>();
                }
                else
                {
                    result = innerXml;
                }
            }

            return result;
        }
    }
}

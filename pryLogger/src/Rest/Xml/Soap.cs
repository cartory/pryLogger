using System;
using System.Linq;

using System.Xml.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace pryLogger.src.Rest.Xml
{
    public static class Declarations 
    {
        public static readonly XDeclaration UTF8 = new XDeclaration("1.0", "utf-8", "yes");
        public static readonly XDeclaration ISO_8859_1 = new XDeclaration("1.0", "iso-8859-1", "yes");
    }

    public static class Soap
    {
        public static string CreateXmlRequest(XName name, object header, object body)
        {
            return CreateXmlRequest(name, header, body, null);
        }

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

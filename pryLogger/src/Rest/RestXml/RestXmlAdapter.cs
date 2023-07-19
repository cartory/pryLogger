using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace pryLogger.src.Rest.RestXml
{
    public static class RestXmlAdapter
    {
        public static bool IsJson(this string json) 
        {
            try
            {
                JObject.Parse(json);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsXml(this string xml) 
        {
            try
            {
                JObject.Parse(xml);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string ToXml(this object arg)
        {
            Type type = arg?.GetType() ?? typeof(Nullable);
            return arg?.ToXml(type.Name);
        }

        public static string ToXml(this object arg, string tagXml) 
        {
            Type type = arg?.GetType() ?? typeof(Nullable);
            string strValue = arg?.ToString() ?? JsonConvert.SerializeObject(arg);

            if (strValue.IsXml())
            {
                return strValue;
            }

            if (!strValue.IsJson())
            {
                strValue = JsonConvert.SerializeObject(new Dictionary<string, object>()
                {
                    [type.Name] = arg
                });
            }

            XmlDocument xmlDocument = JsonConvert.DeserializeXmlNode(strValue, tagXml);
            XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "iso-8859-1", string.Empty);

            string xml;
            xmlDocument.InsertBefore(xmlDeclaration, xmlDocument.DocumentElement);

            using (StringWriter stringWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter))
                {
                    xmlDocument.WriteTo(xmlWriter);
                    xmlWriter.Flush();

                    xml = stringWriter.GetStringBuilder().ToString();
                }
            }

            return xml;
        }

        public static string Fetch(string tagXml, RestRequest req) 
        {
            return RestXmlAdapter.Fetch(tagXml, req, res => res.Content);
        }

        public static string Fetch<T>(string tagXml, RestRequest req, Func<RestResponse, T> callback) 
        {
            return RestClient.Fetch(req, res =>
            {
                var result = callback(res);
                return result.ToXml(tagXml);
            });
        }
    }
}

using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace pryLogger.src.Rest.Xml
{
    public static class XmlExtensions
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
                XDocument.Parse(xml);
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
    }
}

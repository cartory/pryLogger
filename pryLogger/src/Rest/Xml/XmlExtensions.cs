using System;
using System.IO;
using System.Xml;

using System.Xml.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

        public static bool IsXml(this string xml, out XDocument doc)
        {

            try
            {
                doc = XDocument.Parse(xml);
            }
            catch (Exception)
            {
                doc = null;
            }

            return doc != null;
        }

        public static string ToXml(this object arg) => arg?.ToXml(declaration: null);
        public static string ToXml(this object arg, XDeclaration declaration)
        {
            Type type = arg?.GetType() ?? typeof(Nullable);
            return arg?.ToXml(type.Name, declaration);
        }

        public static string ToXml(this object arg, string tagXml) => arg?.ToXml(tagXml, null);
        public static string ToXml(this object arg, string tagXml, XDeclaration declaration)
        {
            Type type = arg?.GetType() ?? typeof(Nullable);
            string strValue = arg is string ? arg.ToString() : JsonConvert.SerializeObject(arg);

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

            string xml;
            XmlDocument xmlDocument = JsonConvert.DeserializeXmlNode(strValue, tagXml);

            if (declaration != null)
            {
                XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration(declaration.Version, declaration.Encoding, declaration.Standalone);
                xmlDocument.InsertBefore(xmlDeclaration, xmlDocument.DocumentElement);
            }

            using (StringWriter stringWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter))
                {
                    xmlDocument.WriteTo(xmlWriter);
                    xmlWriter.Flush();

                    xml = stringWriter.GetStringBuilder().ToString();
                }
            }

            return declaration != null ? xml : Regex.Replace(xml, @"^<\?xml.*\?>", string.Empty);
        }
    }
}

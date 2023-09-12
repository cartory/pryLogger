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
    /// <summary>
    /// Provides extension methods for working with XML and JSON.
    /// </summary>
    public static class XmlExtensions
    {
        /// <summary>
        /// Determines whether the input string is valid JSON.
        /// </summary>
        /// <param name="json">The input string to check.</param>
        /// <returns>True if the string is valid JSON; otherwise, false.</returns>
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

        /// <summary>
        /// Determines whether the input string is valid XML.
        /// </summary>
        /// <param name="xml">The input string to check.</param>
        /// <returns>True if the string is valid XML; otherwise, false.</returns>
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
        /// Parses the input string into an XDocument if it's valid XML.
        /// </summary>
        /// <param name="xml">The input string to parse.</param>
        /// <param name="doc">The resulting XDocument if parsing is successful; otherwise, null.</param>
        /// <returns>True if the string is valid XML and parsing is successful; otherwise, false.</returns>
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

        /// <summary>
        /// Converts an object to an XML string.
        /// </summary>
        /// <param name="arg">The object to convert to XML.</param>
        /// <returns>An XML string representing the object.</returns>
        public static string ToXml(this object arg) => arg?.ToXml(declaration: null);

        /// <summary>
        /// Converts an object to an XML string with a specified XML declaration.
        /// </summary>
        /// <param name="arg">The object to convert to XML.</param>
        /// <param name="declaration">The XML declaration for the resulting XML string.</param>
        /// <returns>An XML string representing the object with the specified declaration.</returns>
        public static string ToXml(this object arg, XDeclaration declaration)
        {
            Type type = arg?.GetType() ?? typeof(Nullable);
            return arg?.ToXml(type.Name, declaration);
        }

        /// <summary>
        /// Converts an object to an XML string with a specified XML tag.
        /// </summary>
        /// <param name="arg">The object to convert to XML.</param>
        /// <param name="tagXml">The XML tag to use for the root element.</param>
        /// <returns>An XML string representing the object with the specified root tag.</returns>
        public static string ToXml(this object arg, string tagXml) => arg?.ToXml(tagXml, null);

        /// <summary>
        /// Converts an object to an XML string with a specified XML tag and declaration.
        /// </summary>
        /// <param name="arg">The object to convert to XML.</param>
        /// <param name="tagXml">The XML tag to use for the root element.</param>
        /// <param name="declaration">The XML declaration for the resulting XML string.</param>
        /// <returns>An XML string representing the object with the specified root tag and declaration.</returns>
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

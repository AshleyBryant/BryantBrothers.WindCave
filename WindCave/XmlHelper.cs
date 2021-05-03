using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BryantBrothers.WindCave
{
	/// <summary>
	/// Error Details
	/// </summary>
	public class XmlHelper
	{
        /// <summary>
        /// Gets the string value of a tag within an XMLDoc.
        /// </summary>
        /// <param name="xml"> XmlDocument </param>
        /// <param name="tagName"> Name of tag </param>
        /// <returns></returns>
        public static string GetString(XmlDocument xml, string tagName)
        {
            return xml.GetElementsByTagName(tagName)[0].InnerText;
        }

        /// <summary>
        /// Gets the string value of a tag within an XMLNode.
        /// </summary>
        /// <param name="node"> XmlNode </param>
        /// <param name="tagName"> Name of tag </param>
        /// <returns></returns>
        public static string GetString(XmlNode node, string tagName)
        {
            return node.SelectNodes(tagName)[0].InnerText;
        }

        /// <summary>
        /// Returns true if the tag exists, false otherwise.
        /// </summary>
        /// <param name="xml"> XmlDocument </param>
        /// <param name="tagName"> Name of tag to check </param>
        /// <returns></returns>
        public static bool TagExists(XmlDocument xml, string tagName)
        {
            return xml.GetElementsByTagName(tagName).Count > 0;
        }

        /// <summary>
        /// Gets the string value from the XMLDoc and converts it to the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public static T GetEnum<T>(XmlDocument xml, string tagName)
        {
            var val = GetString(xml, tagName);

            return (T)Enum.Parse(typeof(T), val);
        }
    }
}

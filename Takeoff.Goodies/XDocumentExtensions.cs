using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace System.Xml.Linq
{
    public static class XDocumentExtensions
    {
        #region XElement

        public static string GetAttributeValue(this XElement element, XName attributeName)
        {
            var att = element.Attribute(attributeName);
            if (att == null)
                return null;
            return att.Value;
        }

        public static bool HasAttributeWithValue(this XElement element, XName attributeName, string value, StringComparison comparison)
        {
            var attValue = element.GetAttributeValue(attributeName);
            if (attValue == null)
                return false;
            return attValue.Equals(value, comparison);
        }


        #endregion

    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Xunit.Sdk;

namespace Microsoft.AspNet.Mvc.Xml
{
    /// <summary>
    /// Xunit assertions related to Xml content.
    /// </summary>
    public static class XmlAssert
    {
        /// <summary>
        /// Compares two xml strings ignoring an element's attribute order.
        /// </summary>
        /// <param name="expectedXml">Expected xml string.</param>
        /// <param name="actualXml">Actual xml string.</param>
        public static void Equal(string expectedXml, string actualXml)
        {
			var sortedExpectedXDoc = XDocument.Parse(expectedXml).SortAttributes();
			var sortedActualXDoc = XDocument.Parse(actualXml).SortAttributes();

			// Since XNode's DeepEquals does not check for presence of xml declaration,
			// check it explicitly
			bool areEqual = VerifyXmlDeclaration(sortedExpectedXDoc.Declaration, sortedActualXDoc.Declaration);
			areEqual = areEqual && XNode.DeepEquals(sortedExpectedXDoc, sortedActualXDoc);

            if (!areEqual)
            {
				throw new EqualException(sortedExpectedXDoc.GetRawXml(), sortedActualXDoc.GetRawXml());
            }
        }

		private static bool VerifyXmlDeclaration(XDeclaration expected, XDeclaration actual)
		{
			if (expected == null && actual == null)
			{
				return true;
			}

			if (expected == null || actual == null)
			{
				return false;
			}

			return string.Equals(expected.Version, actual.Version, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(expected.Encoding, actual.Encoding, StringComparison.OrdinalIgnoreCase);
		}
		private static XDocument SortAttributes(this XDocument doc)
        {
            return new XDocument(
                    doc.Declaration,
					doc.Nodes().OfType<XText>().FirstOrDefault(),
					doc.Root.SortAttributes());
        }

		private static XElement SortAttributes(this XElement element)
        {
            return new XElement(
                    element.Name,
					element.Nodes().OfType<XText>().FirstOrDefault(),
                    element.Attributes().OrderBy(a => a.Name.ToString()),
					element.Elements().Select(child => child.SortAttributes()));
        }
		private static string GetRawXml(this XDocument xdocument)
		{
			string xml = null;
			var stream = new MemoryStream();
			xdocument.Save(stream, SaveOptions.DisableFormatting);
			stream.Position = 0;
			using (var reader = new StreamReader(stream))
			{
				xml = reader.ReadToEnd();
    }
			return xml;
		}
	}
}
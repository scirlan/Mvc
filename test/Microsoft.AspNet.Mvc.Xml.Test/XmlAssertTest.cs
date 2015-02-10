// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;
using Xunit.Sdk;

namespace Microsoft.AspNet.Mvc.Xml.Test
{
	public class XmlAssertTest
	{
		[Theory]
		[InlineData("<A>hello</A>", "<A>hey</A>")]
		[InlineData("<A><B>hello world</B></A>", "<A><B>hello world!!</B></A>")]
		public void ValidatesTextNodes(string input1, string input2)
		{
			Assert.Throws<EqualException>(() => XmlAssert.Equal(input1, input2));
		}

		[Theory]
		[InlineData("<A></A>", "<A></A>")]
		[InlineData("<A/>", "<A/>")]
		public void DoesNotFail_OnEmptyTextNodes(string input1, string input2)
		{
			XmlAssert.Equal(input1, input2);
		}

		[Theory]
		[InlineData("<?xml version=\"1.0\" encoding=\"UTF-8\"?><A></A>", 
			"<A></A>")]
		[InlineData("<?xml version=\"1.0\" encoding=\"UTF-8\"?><A></A>", 
			"<?xml version=\"1.0\" encoding=\"UTF-16\"?><A></A>")]
		public void Validates_XmlDeclaration(string input1, string input2)
		{
			Assert.Throws<EqualException>(() => XmlAssert.Equal(input1, input2));
		}

		[Fact]
		public void Validates_XmlDeclaration_IgnoresCase()
		{
			// Arrange and Act
			var input1 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
						"<A><B color=\"red\" size=\"medium\">hello world</B></A>";
			var input2 = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
						"<A><B size=\"medium\" color=\"red\">hello world</B></A>";

			// Assert
			XmlAssert.Equal(input1, input2);
		}

		[Fact]
		public void IgnoresAttributeOrder()
		{
			// Arrange and Act
			var input1 = "<A><B color=\"red\" size=\"medium\">hello world</B></A>";
			var input2 = "<A><B size=\"medium\" color=\"red\">hello world</B></A>";

			// Assert
			XmlAssert.Equal(input1, input2);
		}

		[Fact]
		public void ValidatesAttributes_IgnoringOrder()
		{
			// Arrange and Act
			var input1 = "<A><B color=\"red\" size=\"medium\">hello world</B></A>";
			var input2 = "<A><B size=\"Medium\" color=\"red\">hello world</B></A>";

			// Assert
			Assert.Throws<EqualException>(() => XmlAssert.Equal(input1, input2));
		}
	}
}
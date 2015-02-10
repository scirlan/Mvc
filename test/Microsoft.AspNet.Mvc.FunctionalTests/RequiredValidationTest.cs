// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
	public class RequiredValidationTest
	{
		private readonly string errorMessageFormat = "Property '{0}' on type '{1}' has RequiredAttribute but " +
														"no DataMember(IsRequired = true) attribute.";
		private readonly IServiceProvider _services = TestHelper.CreateServices(nameof(XmlFormattersWebSite));
		private readonly Action<IApplicationBuilder> _app = new XmlFormattersWebSite.Startup().Configure;

		[Fact]
		public async Task Model_HasRequired_AndNoDataMemberRequiredAttribute()
		{
			// Arrange
			var server = TestServer.Create(_services, _app);
			var client = server.CreateClient();
			var typeName = "XmlFormattersWebSite.Models.CustomerWithRequiredOnly";
			var input = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
						"<CustomerWithRequiredOnly><Name>John</Name></CustomerWithRequiredOnly>";
			var content = new StringContent(input, Encoding.UTF8, "application/xml-xmlser");

			// Act
			var response = await client.PostAsync(
				"http://localhost/RequiredValidation/CustomerWithRequiredOnly",
				content);

			// Assert
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
			var actual = await response.Content.ReadAsStringAsync();

			var modelStateErrors = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(actual);

			Assert.NotNull(modelStateErrors);
			string[] errors;
			modelStateErrors.TryGetValue(typeName, out errors);
			Assert.NotNull(errors);
			Assert.Equal(1, errors.Length);
			Assert.Equal(string.Format(errorMessageFormat, "Id", typeName), errors[0]);
		}

		[Fact]
		public async Task Model_HasRequired_AndSomeDataMemberRequiredAttributes()
		{
			// Arrange
			var server = TestServer.Create(_services, _app);
			var client = server.CreateClient();
			var typeName = "XmlFormattersWebSite.Models.CustomerWithRequiredAndDataMember";
			var input = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
						"<CustomerWithRequiredAndDataMember><Id>10</Id>" +
						"<Name>John</Name></CustomerWithRequiredAndDataMember>";
			var content = new StringContent(input, Encoding.UTF8, "application/xml-xmlser");

			// Act
			var response = await client.PostAsync(
				"http://localhost/RequiredValidation/CustomerWithRequiredAndDataMember",
				content);

			// Assert
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
			var actual = await response.Content.ReadAsStringAsync();

			var modelStateErrors = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(actual);

			Assert.NotNull(modelStateErrors);
			string[] errors;
			modelStateErrors.TryGetValue(typeName, out errors);
			Assert.NotNull(errors);
			Assert.Equal(1, errors.Length);
			Assert.Equal(string.Format(errorMessageFormat, "Age", typeName), errors[0]);
		}
	}
}
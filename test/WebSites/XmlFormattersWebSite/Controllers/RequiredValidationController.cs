// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using XmlFormattersWebSite.Models;

namespace XmlFormattersWebSite.Controllers
{
    public class RequiredValidationController : Controller
    {
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			if (!context.ModelState.IsValid)
			{
				// By default, information related to exceptions in model state are
				// not serialized to the client for security reasons. Since we want to verify the 
				// exception messages for our scenario, here we try to get the information about the
				// exceptions and serialize them too.
				var serializableModelState = new Dictionary<string, string[]>();
				foreach (var keyModelStatePair in context.ModelState)
				{
					var key = keyModelStatePair.Key;
					var errors = keyModelStatePair.Value.Errors;
					if (errors != null && errors.Count > 0)
					{
						var errorMessages = errors.Select(error =>
						{
							if (string.IsNullOrEmpty(error.ErrorMessage))
							{
								if (error.Exception != null)
								{
									return error.Exception.Message;
								}
							}

							return error.ErrorMessage;
								
						}).ToArray();

						serializableModelState.Add(key, errorMessages);
					}
				}

				context.Result = new JsonResult(serializableModelState) { StatusCode = 400 };
			}
		}

		public CustomerWithRequiredOnly CustomerWithRequiredOnly([FromBody] CustomerWithRequiredOnly customer)
		{
			return customer;
		}

		public CustomerWithRequiredAndDataMember CustomerWithRequiredAndDataMember(
			[FromBody] CustomerWithRequiredAndDataMember customer)
		{
			return customer;
		}

		public CustomerWithComplexPropertyHavingRequiredOnly CustomerWithComplexPropertyHavingRequiredOnly(
			[FromBody] CustomerWithComplexPropertyHavingRequiredOnly customer)
		{
			return customer;
		}
	}
}
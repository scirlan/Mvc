// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc.Xml
{
	public static class RequiredValidationHelper
	{
		private static ConcurrentDictionary<Type, List<string>> probedTypes
			= new ConcurrentDictionary<Type, List<string>>();

		public static void CheckRequiredValidation(Type modelType, ModelStateDictionary modelState)
		{
			var errors = CheckRequiredValidation(modelType);

			foreach (var error in errors)
			{
				modelState.AddModelError(
					modelType.FullName,
					new InvalidOperationException(error));
			}
		}

        private static List<string> CheckRequiredValidation(Type modelType)
		{
			//if (modelType.IsGenericType)
			//{
			//	modelType = ExtractTypeParameter(modelType);
			//}

			// Check if a type has already been probed and get the errors.
			if (probedTypes.ContainsKey(modelType))
			{
				return probedTypes[modelType];
			}

			var errors = new List<string>();

			if (!modelType.IsValueType())
			{
				probedTypes.TryAdd(modelType, errors);
				foreach (var property in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
				{
					if (property.PropertyType.IsValueType())
					{
						var required = property.GetCustomAttribute(typeof(RequiredAttribute), inherit: true);
						if (required != null)
						{
							var hasDataMemberRequired = false;
							var dataMemberRequired = (DataMemberAttribute)property.GetCustomAttribute(
								typeof(DataMemberAttribute),
								inherit: true);
							if (dataMemberRequired != null && dataMemberRequired.IsRequired)
							{
								hasDataMemberRequired = true;
							}

							if (!hasDataMemberRequired)
							{
								errors.Add(string.Format(
									"Property '{0}' on type '{1}' has Required attribute but is missing DataMember(IsRequired = true)",
									property.Name, modelType.FullName));
							}
						}
					}
					else
					{
						errors.AddRange(CheckRequiredValidation(property.PropertyType));
					}
				}
			}

			return errors;
		}

		static Type ExtractTypeParameter(Type type)
		{
			var enumerableOfT = type.ExtractGenericInterface(typeof(IEnumerable<>));
			if (enumerableOfT != null)
			{
				return enumerableOfT.GetGenericArguments()[0];
			}

			return type;
		}

		static Type ExtractGenericInterface(this Type queryType, Type interfaceType)
		{
			Func<Type, bool> matchesInterface =
				t => t.IsGenericType() && (t.GetGenericTypeDefinition() == interfaceType);
			return (matchesInterface(queryType)) ?
				queryType :
				queryType.GetInterfaces().FirstOrDefault(matchesInterface);
		}
	}
}
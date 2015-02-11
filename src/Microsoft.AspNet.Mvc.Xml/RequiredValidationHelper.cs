// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
        public static void CheckForRequiredAttribute(Type modelType, ModelStateDictionary modelState)
        {
            var errors = CheckForRequiredAttributeHelper(modelType, modelState);
        }

        private static List<string> CheckForRequiredAttributeHelper(Type modelType, ModelStateDictionary modelState)
        {
            List<string> errors;

            if (modelType.IsGenericType())
            {
                var enumerableOfT = modelType.ExtractGenericInterface(typeof(IEnumerable<>));
                if (enumerableOfT != null)
                {
                    modelType = enumerableOfT.GetGenericArguments()[0];
                }
            }

            errors = new List<string>();
            if (!modelType.IsValueType())
            {
                foreach (var property in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (!property.PropertyType.IsNullableValueType() && property.PropertyType.IsValueType())
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
                                var errorMessage = Resources.FormatRequiredProperty_MustHaveDataMemberRequired(
                                                                    property.Name,
                                                                    property.DeclaringType.FullName);

                                modelState.TryAddModelError(property.DeclaringType.FullName, errorMessage);
                            }
                        }
                    }
                    else
                    {
                        errors.AddRange(CheckForRequiredAttributeHelper(property.PropertyType, modelState));
                    }
                }
            }

            return errors;
        }
    }
}
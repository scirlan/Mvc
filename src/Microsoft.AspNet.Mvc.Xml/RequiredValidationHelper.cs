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
        private static ConcurrentDictionary<Type, List<string>> probedTypes
            = new ConcurrentDictionary<Type, List<string>>();

        public static void CheckForRequiredAttribute(Type modelType, ModelStateDictionary modelState)
        {
            var errors = CheckForRequiredAttributeHelper(modelType, modelState);

            foreach (var error in errors)
            {
                modelState.TryAddModelError(modelType.FullName, error);
            }
        }

        private static List<string> CheckForRequiredAttributeHelper(Type modelType, ModelStateDictionary modelState)
        {
            List<string> errors;

            // For scenarios where the model being bound is for exmaple List<Person>
            // or a property is List<Person>.
            if (modelType.IsGenericType())
            {
                var enumerableOfT = modelType.ExtractGenericInterface(typeof(IEnumerable<>));
                if (enumerableOfT != null)
                {
                    modelType = enumerableOfT.GetGenericArguments()[0];
                }
            }

            // if we have already visited the type, then skip to avoid infinite
            // recursion where a property of a type refers to itself.
            if (probedTypes.TryGetValue(modelType, out errors))
            {
                return errors;
            }

            errors = new List<string>();
            if (!modelType.IsValueType() && !modelType.IsNullableValueType())
            {
                List<PropertyInfo> referenceTypeProperties = new List<PropertyInfo>();
                foreach (var property in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var propertyType = property.PropertyType;
                    if (!propertyType.IsNullableValueType() && propertyType.IsValueType())
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

                                errors.Add(errorMessage);
                            }
                        }
                    }
                    else
                    {
                        referenceTypeProperties.Add(property);
                    }
                }

                probedTypes.TryAdd(modelType, errors);

                foreach (var referenceTypeProperty in referenceTypeProperties)
                {
                    errors.AddRange(CheckForRequiredAttributeHelper(referenceTypeProperty.PropertyType, modelState));
                }
            }

            return errors;
        }
    }
}
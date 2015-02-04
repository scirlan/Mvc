// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// A validator that validates a given object and adds erros to the given <see cref="ModelStateDictionary"/>.
    /// </summary>
    public interface IObjectModelValidator
    {
        /// <summary>
        /// Validates the given model in <see cref="ModelValidationContext.ModelMetadata"/>.
        /// </summary>
        /// <param name="validationContext">The <see cref="ModelValidationContext"/> associated with the current call.
        /// </param>
        /// <param name="modelStatePrefix">The prefix to be used while adding errors to the 
        /// <see cref="ModelStateDictionary"/></param>
        void Validate(ModelValidationContext validationContext, string modelStatePrefix);
    }
}

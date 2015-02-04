// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Provides a default implementation of <see cref="IControllerActionArgumentBinder"/>.
    /// Uses ModelBinding to populate action parameters.
    /// </summary>
    public class DefaultControllerActionArgumentValidator : IControllerActionArgumentValidator
    {
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly IValidationExcludeFiltersProvider _options;

        public DefaultControllerActionArgumentValidator(
            IModelMetadataProvider modelMetadataProvider,IValidationExcludeFiltersProvider excludeFilters)
        {
            _modelMetadataProvider = modelMetadataProvider;
            _options = excludeFilters;
        }

        public Task ValidateArgumentsAsync([NotNull]ActionContext actionContext, [NotNull]ActionBindingContext bindingContext, [NotNull]IDictionary<string, object> actionArguments)
        {
            var actionDescriptor = actionContext.ActionDescriptor as ControllerActionDescriptor;
            if (actionDescriptor == null)
            {
                throw new ArgumentException(
                    Resources.FormatActionDescriptorMustBeBasedOnControllerAction(
                        typeof(ControllerActionDescriptor)),
                        nameof(actionContext));
            }

            var validator = new DefaultModelValidator();
            foreach (var parameter in actionArguments)
            {
                var metadata = _modelMetadataProvider.GetMetadataForParameter(
                    modelAccessor: null,
                    methodInfo: actionDescriptor.MethodInfo,
                    parameterName: parameter.Key);
                metadata.Model = parameter.Value;
                var validationContext = new ModelValidationContext(
                    _modelMetadataProvider,
                    bindingContext.ValidatorProvider,
                    actionContext.ModelState,
                    metadata,
                    containerMetadata: null,
                    excludeFromValidationFilters: _options.ExcludeFilters);
                validator.Validate(validationContext, parameter.Key);
            }

            return Task.FromResult(true);
        }
    }
}

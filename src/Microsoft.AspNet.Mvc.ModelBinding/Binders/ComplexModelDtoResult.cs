// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public sealed class ComplexModelDtoResult
    {
        public static ComplexModelDtoResult FromBindingContext([NotNull] ModelBindingContext context)
        {
            return new ComplexModelDtoResult(context.Model, context.IsModelSet, context.ModelName);
        }

        public ComplexModelDtoResult(
            object model,
            bool isModelBound,
            string modelStateKey)
        {
            ModelStateKey = modelStateKey;
            Model = model;
            IsModelBound = isModelBound;
        }

        public string ModelStateKey { get; set; }

        public bool IsModelBound { get; }

        public object Model { get; set; }
    }
}

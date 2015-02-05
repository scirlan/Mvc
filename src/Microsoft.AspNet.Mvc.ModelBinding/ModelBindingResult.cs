// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ModelBindingResult
    {
        public static ModelBindingResult FromBindingContext([NotNull] ModelBindingContext context)
        {
            return new ModelBindingResult(context.Model, context.IsModelSet, context.ModelName);
        }

        public ModelBindingResult(
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

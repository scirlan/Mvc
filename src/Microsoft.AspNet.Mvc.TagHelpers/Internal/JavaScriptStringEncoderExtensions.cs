// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text;
using Microsoft.AspNet.WebUtilities.Encoders;

namespace Microsoft.AspNet.Mvc.TagHelpers.Internal
{
    /// <summary>
    /// Utility methods for dealing with JavaScript.
    /// </summary>
    public static class JavaScriptStringEncoderExtensions
    {
        /// <summary>
        /// Encodes a .NET string array for safe use as a JavaScript array literal, including inline in an HTML file.
        /// </summary>
        public static string JavaScriptStringArrayEncode(
            this IJavaScriptStringEncoder encoder,
            IEnumerable<string> values)
        {
            var builder = new StringBuilder();

            var firstAdded = false;

            builder.Append('[');
            
            foreach (var value in values)
            {
                if (firstAdded)
                {
                    builder.Append(',');
                }
                builder.Append('"');
                builder.Append(encoder.JavaScriptStringEncode(value));
                builder.Append('"');
                firstAdded = true;
            }

            builder.Append(']');

            return builder.ToString();
        }
    }
}
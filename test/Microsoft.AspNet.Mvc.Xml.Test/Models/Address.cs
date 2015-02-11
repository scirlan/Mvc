// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.AspNet.Mvc.Xml
{
    public class Address
    {
        [Required]
        public int Zipcode { get; set; }

        [Required]
        public bool IsResidential { get; set; }
    }
}
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.AspNet.Mvc.Xml
{
    public class Customer
    {
        [Required]
        public List<int> TicketsIds { get; set; }

        [Required]
        public int? Age { get; set; }
    }
}
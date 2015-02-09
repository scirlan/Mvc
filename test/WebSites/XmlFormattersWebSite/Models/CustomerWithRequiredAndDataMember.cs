// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace XmlFormattersWebSite.Models
{
	[DataContract]
	public class CustomerWithRequiredAndDataMember
	{
		[Required]
		[DataMember(IsRequired = true)]
		public int Id { get; set; }

		[Required]
		[DataMember(Name = "Age")]
		public int Age { get; set; }

		[DataMember]
		public string Name { get; set; }
	}
}
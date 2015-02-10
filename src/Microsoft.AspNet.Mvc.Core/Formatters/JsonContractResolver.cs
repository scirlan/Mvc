// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.AspNet.Mvc
{
	/// <summary>
	/// Represents the default <see cref="IContractResolver"/> used by <see cref="JsonInputFormatter"/> and
	/// <see cref="JsonOutputFormatter"/>.
	/// </summary>
	public class JsonContractResolver : DefaultContractResolver
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="JsonContractResolver" /> class.
		/// </summary>
		public JsonContractResolver()
		{
			// Need this setting to have [Serializable] types serialized correctly
			IgnoreSerializableAttribute = true;
		}

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var property = base.CreateProperty(member, memberSerialization);
			property.Required = Required.AllowNull;
			property.DefaultValueHandling = DefaultValueHandling.Include;
			property.NullValueHandling = NullValueHandling.Include;
			return property;
		}
	}
}
using System;
using System.ComponentModel.DataAnnotations;

namespace XmlFormattersWebSite.Models
{
    public class ModelWithNoErrors
    {
		public int IntProperty { get; set; }
		
		public bool BoolProperty { get; set; }

		public int? NullableIntProperty { get; set; }

		//public int IntProperty { get; set; }
	}
}
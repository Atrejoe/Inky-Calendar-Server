using System;
using System.ComponentModel.DataAnnotations;

namespace InkyCal.Server.Config
{
	[AttributeUsage(AttributeTargets.Property)]
	public class UrlAttribute : ValidationAttribute
	{
		public UriKind UriKind { get; set; } = UriKind.RelativeOrAbsolute;

		public override bool IsValid(object value)
		{
			if(value is null)
				return true;

			return value is string strValue 
				&& Uri.TryCreate(strValue, UriKind, out var _);
		}

		public override bool RequiresValidationContext => false;
	}
}

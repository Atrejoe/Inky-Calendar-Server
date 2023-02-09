using System;
using System.ComponentModel.DataAnnotations;

namespace InkyCal.Server.Config
{
	/// <summary>
	/// Google Oauth configuration
	/// </summary>
	public class GoogleOAuth
	{
		public static bool Enabled { get; set; }

		public static string ClientId { get; set; }

		public static string ClientSecret { get; set; }

		public static string ProjectId { get; set; }

		[Url]
		public static Uri InkyCalRoot { get; set; }

		[Url(UriKind= UriKind.Absolute)]
		public static Uri Website { get; set; }
	}

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

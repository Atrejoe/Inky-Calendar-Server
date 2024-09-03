using System;

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
}

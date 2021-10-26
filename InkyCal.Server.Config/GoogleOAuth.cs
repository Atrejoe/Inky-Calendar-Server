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

		public static Uri InkyCalRoot { get; set; } = new Uri("/", UriKind.Relative);

		public static Uri Website { get; set; } = new Uri("https://inkycal.robertsirre.nl", UriKind.Absolute);
	}
}

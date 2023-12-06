using System.ComponentModel.DataAnnotations;

namespace InkyCal.Models
{
	/// <summary>
	/// A panel for displaying weather information
	/// </summary>
	public class WeatherPanel : Panel
	{
		/// <summary>
		/// The authentication token, possibly moved to the users' profile
		/// </summary>
		[Required, MaxLength(255), MinLength(1)]
		public string Token { get; set; }

		/// <summary>
		/// The location to display weather for
		/// </summary>
		[Required, MaxLength(255), MinLength(1)]
		public string Location { get; set; }

	}
}

using System.ComponentModel.DataAnnotations;

namespace InkyCal.Models
{
	public class WeatherPanel : Panel
	{
		[Required, MaxLength(255), MinLength(1)]
		public string Token { get; set; }

		[Required, MaxLength(255), MinLength(1)]
		public string Location { get; set; }

	}
}

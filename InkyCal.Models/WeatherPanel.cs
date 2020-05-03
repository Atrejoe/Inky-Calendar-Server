using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InkyCal.Models
{
	[Table("WeatherPanel", Schema = "InkyCal")]
	public class WeatherPanel : Panel
	{
		[Required, MaxLength(255), MinLength(1)]
		public string Token { get; set; }

		[Required, MaxLength(255), MinLength(1)]
		public string Location { get; set; }

	}
}

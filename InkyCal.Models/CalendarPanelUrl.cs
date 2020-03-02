using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InkyCal.Models
{
	[Table("ImagePanelUrl", Schema = "InkyCal")]
	public class CalendarPanelUrl
	{

		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[ForeignKey(nameof(CalendarPanel))]
		public Guid IdPanel { get; set; }

		[Url, Required]
		public string Url { get; set; }

		public CalendarPanel Panel { get; set; }

		public static implicit operator Uri(CalendarPanelUrl calendarPanelUrl)
		{
			return new Uri(calendarPanelUrl.Url);
		}
	}
}

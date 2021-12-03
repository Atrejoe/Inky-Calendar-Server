using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace InkyCal.Models
{
	[Table("CalendarPanelUrl",Schema = "InkyCal")]
	public class CalendarPanelUrl
	{

		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[ForeignKey(nameof(CalendarPanel))]
		public Guid IdPanel { get; set; }

		[Url, Validation.Url(UriKind.Absolute), Required]
		[SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "Using string for databinding")]
		public string Url { get; set; }

		public CalendarPanel Panel { get; set; }

		public static implicit operator Uri(CalendarPanelUrl calendarPanelUrl)
		{
			if (calendarPanelUrl is null)
				return null;

			return new Uri(calendarPanelUrl.Url);
		}

		public Uri ToUri()
		{
			return new Uri(Url);
		}
	}
}

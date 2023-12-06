using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace InkyCal.Models
{
	/// <summary>
	/// A URL for a <see cref="CalendarPanel"/>"/>
	/// </summary>
	[Table("CalendarPanelUrl",Schema = "InkyCal")]
	public class CalendarPanelUrl
	{
		/// <summary>
		/// Identifier of the url
		/// </summary>
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		/// <summary>
		/// Identifier of the <see cref="CalendarPanel"/> this url belongs to.
		/// </summary>
		[ForeignKey(nameof(CalendarPanel))]
		public Guid IdPanel { get; set; }

		/// <summary>
		/// The url of the calendar
		/// </summary>
		[Url, Validation.Url(UriKind.Absolute), Required]
		[SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "Using string for databinding")]
		public string Url { get; set; }

		/// <summary>
		/// The <see cref="CalendarPanel"/> this url belongs to.
		/// </summary>
		public CalendarPanel Panel { get; set; }

		/// <summary>
		/// Implicitly converts a <see cref="CalendarPanelUrl"/> to a <see cref="Uri"/>
		/// </summary>
		/// <param name="calendarPanelUrl"></param>
		public static implicit operator Uri(CalendarPanelUrl calendarPanelUrl)
		{
			if (calendarPanelUrl is null)
				return null;

			return new Uri(calendarPanelUrl.Url);
		}

		/// <summary>
		/// Converts this <see cref="CalendarPanelUrl"/> to a <see cref="Uri"/>
		/// </summary>
		/// <returns></returns>
		public Uri ToUri()
		{
			return new Uri(Url);
		}
	}
}

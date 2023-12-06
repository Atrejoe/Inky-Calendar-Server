using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using InkyCal.Models.Validation;

namespace InkyCal.Models
{

	/// <summary>
	/// A panel that displays a calendar
	/// </summary>
	public class CalendarPanel : Panel
	{
		/// <summary>
		/// The urls of anonymously accessible, iCal-formatted calendars
		/// </summary>
		[Required, MaxLength(5), MinLength(0)]
		public HashSet<CalendarPanelUrl> CalenderUrls { get; set; } = [];

		/// <summary>
		/// SUbscribed Google calenders, accessibly via <see cref="GoogleOAuthAccess"/>.
		/// </summary>
		public HashSet<SubscribedGoogleCalender> SubscribedGoogleCalenders { get; set; } = [];

		/// <summary>
		/// The draw mode of the calendar
		/// </summary>
		[Required, DefinedEnum]
		public CalenderDrawMode DrawMode { get; set; } = CalenderDrawMode.List;
	}
}

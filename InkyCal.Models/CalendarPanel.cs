using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InkyCal.Models
{

	public class CalendarPanel : Panel
	{
		[Required, MaxLength(5), MinLength(0)]
		public HashSet<CalendarPanelUrl> CalenderUrls { get; set;  } = new HashSet<CalendarPanelUrl>();

		public HashSet<SubscribedGoogleCalender> SubscribedGoogleCalenders { get; set; } = new HashSet<SubscribedGoogleCalender>();

	}
}

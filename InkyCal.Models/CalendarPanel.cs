using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InkyCal.Models
{

	[Table("CalendarPanel", Schema = "InkyCal")]
	public class CalendarPanel : Panel
	{
		[Required, MaxLength(5), MinLength(0)]
		public HashSet<CalendarPanelUrl> CalenderUrls { get; set;  } = new HashSet<CalendarPanelUrl>();

		public HashSet<SubscribedGoogleCalender> SubscribedGoogleCalenders { get; set; } = new HashSet<SubscribedGoogleCalender>();

	}
}

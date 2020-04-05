using System;

namespace InkyCal.Utils
{
	/// <summary>
	/// A test version for <see cref="CalendarPanelRenderer"/>
	/// </summary>
	public class TestCalendarPanelRenderer : CalendarPanelRenderer
	{
		/// <summary>
		/// A public iCal calendar with public holidays for demo/test purposes
		/// </summary>
		public const string PublicHolidayCalenderUrl = @"https://calendar.google.com/calendar/ical/en.usa%23holiday%40group.v.calendar.google.com/public/basic.ics";

		/// <summary>
		/// A public iCal calendar with debug events for demo/test purposes
		/// </summary>
		public const string DebugCalenderUrl = @"https://calendar.google.com/calendar/ical/6nqv871neid5l0t7hgk6jgr24c%40group.calendar.google.com/private-c9ab692c99fb55360cbbc28bf8dedb3a/basic.ics";

		/// <summary>
		/// 
		/// </summary>
		public TestCalendarPanelRenderer() : base(
			iCalUrls: new[] { 
				new Uri(PublicHolidayCalenderUrl), 
				new Uri(DebugCalenderUrl) 
			})
		{
		}
	}
}

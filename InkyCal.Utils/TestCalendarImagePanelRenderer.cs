using System;
using InkyCal.Models;

namespace InkyCal.Utils
{
	/// <summary>
	/// A test version for <see cref="CalendarPanelRenderer"/>
	/// </summary>
	public class TestCalendarImagePanelRenderer : CalendarPanelRenderer
	{
		/// <summary>
		/// A public iCal calendar with public holidays for demo/test purposes
		/// </summary>
		public const string PublicHolidayCalenderUrl = @"https://calendar.google.com/calendar/ical/en.usa%23holiday%40group.v.calendar.google.com/public/basic.ics";

		/// <summary>
		/// A public iCal calendar with phases of the moon, for demo/test purposes
		/// </summary>
		public const string PhasesOfTheMoonCalenderUrl = @"https://calendar.google.com/calendar/ical/ht3jlfaac5lfd6263ulfh4tql8%40group.calendar.google.com/public/basic.ics";

		/// <summary>
		/// 
		/// </summary>
		public TestCalendarImagePanelRenderer() : base(
			async (token) =>
			{
				await System.Threading.Tasks.Task.CompletedTask;
			},
			iCalUrls: [
				new Uri(PublicHolidayCalenderUrl),
				new Uri(PhasesOfTheMoonCalenderUrl)
			],
			[],
			drawMode: CalenderDrawMode.AIImage)
		{
		}
	}
}

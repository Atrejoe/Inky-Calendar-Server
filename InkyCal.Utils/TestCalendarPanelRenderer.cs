using System;

namespace InkyCal.Utils
{
    /// <summary>
    /// A test version for <see cref="CalendarPanelRenderer"/>
    /// </summary>
    public class TestCalendarPanelRenderer : CalendarPanelRenderer
    {
        /// <summary>
        /// A public iCal calendar for demo/test purposes
        /// </summary>
        public const string CalenderUrl = @"https://calendar.google.com/calendar/ical/en.usa%23holiday%40group.v.calendar.google.com/public/basic.ics";

        /// <summary>
        /// 
        /// </summary>
        public TestCalendarPanelRenderer() : base(new Uri(CalenderUrl))
        {
        }
    }
}

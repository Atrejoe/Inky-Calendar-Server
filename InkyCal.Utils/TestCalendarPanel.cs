using System;

namespace InkyCal.Utils
{
    /// <summary>
    /// A test version for <see cref="CalendarPanel"/>
    /// </summary>
    public class TestCalendarPanel : CalendarPanel
    {
        /// <summary>
        /// A public iCal calendar for demo/test purposes
        /// </summary>
        public const string CalenderUrl = @"https://calendar.google.com/calendar/ical/en.usa%23holiday%40group.v.calendar.google.com/public/basic.ics";

        /// <summary>
        /// 
        /// </summary>
        public TestCalendarPanel() : base(new Uri(CalenderUrl))
        {
        }
    }
}

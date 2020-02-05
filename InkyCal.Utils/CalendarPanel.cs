using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace InkyCal.Utils
{
    /// <summary>
    /// A test version for <see cref="CalendarPanel"/>
    /// </summary>
    public class TestCalendar : CalendarPanel
    {
        /// <summary>
        /// A public iCal calendar for demo/test purposes
        /// </summary>
        public const string CalenderUrl = @"https://calendar.google.com/calendar/ical/en.usa%23holiday%40group.v.calendar.google.com/public/basic.ics";

        /// <summary>
        /// 
        /// </summary>
        public TestCalendar() : base(new Uri(CalenderUrl))
        {
        }
    }

    /// <summary>
    /// A panel that shows one or more calendars
    /// </summary>
    public class CalendarPanel : IPanel
    {
        /// <summary>
        /// Shows a single calendar
        /// </summary>
        /// <param name="iCalUrl"></param>
        public CalendarPanel(Uri iCalUrl) : this(new[] { iCalUrl })
        {
            if (iCalUrl is null)
                throw new ArgumentNullException(nameof(iCalUrl));

        }
        /// <summary>
        /// Show one or more calendars
        /// </summary>
        /// <param name="iCalUrls"></param>
        public CalendarPanel(Uri[] iCalUrls)
        {
            ICalUrls = iCalUrls ?? throw new ArgumentNullException(nameof(iCalUrls));
        }

        /// <summary>
        /// The calendars to render
        /// </summary>
        public Uri[] ICalUrls { get; }

        /// <summary>
        /// Renders the calendars in portrait mode (flipping <paramref name="width"/> and <paramref name="height"/>)
        /// </summary>
        /// <param name="width">The height of the panel (in landscape mode).</param>
        /// <param name="height">The width of the panel (in landscape mode).</param>
        /// <param name="colors">The number of color to render in.</param>
        /// <returns></returns>
        public Image GetImage(int width, int height, Color[] colors)
        {
            var result = new Image<Argb32>(new Configuration(), width, height, new Argb32(0, 0, 0, 0));

            return result;
        }
    }
}

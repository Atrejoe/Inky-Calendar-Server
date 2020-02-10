using Ical.Net;
using Ical.Net.CalendarComponents;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Linq;
using System.Net.Http;

namespace InkyCal.Utils
{

    /// <summary>
    /// A panel that shows one or more calendars
    /// </summary>
    public class CalendarPanel : IPanel
    {
        private static readonly HttpClient client = new HttpClient();

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
            var font = SystemFonts.CreateFont("Courier new", 14, FontStyle.Regular);

            var calendars = new CalendarCollection();

            foreach (var iCalUrl in ICalUrls)
                calendars.Add(Calendar.Load(client.GetStreamAsync(iCalUrl.ToString()).Result));


            var item = calendars
                        .GetOccurrences(DateTime.Now, DateTime.Now.AddYears(1))
                        .Select(x => x.Source)
                        .Cast<CalendarEvent>()
                        .FirstOrDefault();


            var p = new PointF(100, 100);
            var text = $@"{item.Start} = {item.End}
{item.Name}
{item.Summary}
";

            System.Diagnostics.Trace.WriteLine(text);

            result.Mutate(x =>
            {
                x.DrawText(new TextGraphicsOptions()
                {
                    Antialias = false,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                }
                    //x.DrawText(iCalUrl, font, Color.Black, center, new TextGraphicsOptions(true));
                    , "hALLO", font, Color.Pink, p); ; ;

                });

            return result;
        }
    }
}

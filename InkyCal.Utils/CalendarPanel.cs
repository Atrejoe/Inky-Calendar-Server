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
using System.Reflection;

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
            var result = new Image<Argb32>(new Configuration() { }, width, height, new Argb32(0, 0, 0, 0));

            FontCollection fonts = new FontCollection();
            var assembly = typeof(CalendarPanel).GetTypeInfo().Assembly;

            //FontFamily NotoSans;
            //using (var resource = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.NotoSans-SemiCondensed.ttf"))
            //{
            //    NotoSans = fonts.Install(resource);
            //}

            FontFamily MonteCarlo;
            using (var resource = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.MonteCarloFixed12.ttf"))
                MonteCarlo = fonts.Install(resource);


            var font = MonteCarlo.CreateFont(16);

            var calendars = new CalendarCollection();

            foreach (var iCalUrl in ICalUrls)
                calendars.Add(Calendar.Load(client.GetStreamAsync(iCalUrl.ToString()).Result));


            var items = calendars
                        .GetOccurrences(DateTime.Now, DateTime.Now.AddYears(1))
                        .Select(x => x.Source)
                        .Cast<CalendarEvent>()
                        .Take(10);

            var p = new PointF(0, 0);
            var text = string.Join(
                Environment.NewLine,
                items.Select(item=>
                    $@"{(item.Start.HasTime
                        ?$"{item.Start:dd MMM HH:mm} - {item.End:HH:mm}" 
                        :$"{item.Start:dd MMM} All day")} {(item.Summary.Length>23? item.Summary.Substring(0, 20)+"...":item.Summary)}"));

            System.Diagnostics.Trace.WriteLine(text);
            var brush = new SolidBrush(Color.Black);
            result.Mutate(x =>
            {
                x.DrawText(new TextGraphicsOptions(false), text, font, brush, p);
            });
        
            return result;
        }
}
}

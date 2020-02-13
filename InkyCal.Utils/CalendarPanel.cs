using Ical.Net;
using Ical.Net.CalendarComponents;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using static InkyCal.Utils.FontHelper;

namespace InkyCal.Utils
{

	/// <summary>
	/// A helper class for obtaining calender info
	/// </summary>
	public static class CalenderExtensions
	{


		private static readonly HttpClient client = new HttpClient();

		/// <summary>
		/// Get 
		/// </summary>
		/// <param name="ICalUrls"></param>
		/// <param name="errorDetailsLength"></param>
		/// <param name="errors"></param>
		/// <returns></returns>
		public static CalendarCollection GetCalendars(this Uri[] ICalUrls, int errorDetailsLength, out StringBuilder errors)
		{
			var sbErrors = new StringBuilder();
			var sw = Stopwatch.StartNew();

			var calendars = new CalendarCollection();
			ICalUrls
					.AsParallel()
					.ForAll(
						iCalUrl =>
						{
							try
							{
								calendars.Add(Calendar.Load(client.GetStreamAsync(iCalUrl.ToString()).Result));
							}
							catch (HttpRequestException ex)
							{
								sbErrors.AppendLine($"Failed to obtain calender data:\n{ex.Message.Limit(errorDetailsLength, " ...")}");
							}
							catch (SerializationException ex)
							{
								sbErrors.AppendLine($"Failed to parse calender data:\n{ex.Message.Limit(errorDetailsLength, " ...")}");
							}
							catch (Exception ex)
							{
								sbErrors.AppendLine($"Failed to obtain or parse calender data:\n{ex.Message.Limit(errorDetailsLength, " ...")}");
							}
						});

			Trace.WriteLine($"Obtained calendars in {sw.Elapsed}");
			Trace.WriteLine(sbErrors.ToString());

			errors = sbErrors;

			return calendars;
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

			var result = new Image<Rgba32>(new Configuration() { }, width, height, Rgba32.White);

			var font = MonteCarlo.CreateFont(12); //Font that works well anti-aliassed

			var characterWidth = font.GetCharacterWidth(); //Works only for some known fixed-width fonts

			var characterPerLine = characterWidth > 0
								? (width / characterWidth.Value)
								: width / 7; //For now fall back to 100 characters width, which is nonsense

			var twoLines = characterPerLine * 2;

			var calendars = ICalUrls.GetCalendars(twoLines, out var sbErrors);

			var items = calendars
						.GetOccurrences(DateTime.Now.Date, DateTime.Now.AddYears(1))
						.Select(x => x.Source)
						.Cast<CalendarEvent>()
						.Where(x => x.Start.Value.Date >= DateTime.Now.Date)
						.ToArray()
						.OrderBy(x => x.Start.Value)
						.Take(60);

			var options_Date = new TextGraphicsOptions(false)
			{
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				WrapTextWidth = width,
				DpiX = 96,
				DpiY = 96
			};

			var textMeasureOptions_Date = options_Date.ToRendererOptions(font);

			//Start drawing while iterating through items
			result.Mutate(canvas =>
			{
				//Keep track of vertical position
				var y = 0;

				//Draw calender parsing errors (in a red panel) first
				if (sbErrors.Length > 0)
				{
					var errorMessage = sbErrors.ToString();

					var textMeasureOptions_Error = textMeasureOptions_Date.Clone();
					textMeasureOptions_Error.WrappingWidth = width - 4;
					var textDrawOptions_Error = textMeasureOptions_Error.ToTextGraphicsOptions(false);

					var errorMessageHeight = TextMeasurer.MeasureBounds(errorMessage, textMeasureOptions_Error);

					errorMessageHeight.Width = width;
					errorMessageHeight.Height += 4; //Pad 2 px on all sides
					canvas.Fill(Color.Red, errorMessageHeight);

					var pError = new PointF(2, 2);//Adhere to padding
					canvas.DrawText(textDrawOptions_Error, errorMessage, font, Color.White, pError);

					y += (int)errorMessageHeight.Height;
				}

				//Group by day, then show events
				items
					.GroupBy(x => x.Start.Date)
					.OrderBy(x => x.Key)
					.ToList()
					.ForEach(x =>
					{
						//Draw a red line for each day
						y += 4;

						canvas.DrawLines(Color.Red, 2, new[] { new PointF(2, y), new PointF(width - 2, y) });

						y += 1;

						//Write the day part only once for all events within a day
						var day = $"{x.Key:ddd dd MMM} ";

						var indentSize = day.Length;

						var indent = (int)TextMeasurer.MeasureBounds(day, textMeasureOptions_Date).Width
								   + 10; //Space of 10 pixels

						canvas.DrawText(options_Date, day, font, Color.Black, new PointF(0, y));

						var options = options_Date.Clone();
						options.WrapTextWidth = width - indent;

						var textMeasureOptions = options.ToRendererOptions(font);

						//Then write each event, wrap the summary
						x.ToList().ForEach(item =>
						{
							//When summary is very long, cut if off
							var line = DescribeEvent(item).Limit(500, " ...");
							try
							{
								canvas.DrawText(options, line, font, Color.Black, new PointF(indent, y));

								//Invert all day indicator
								if (item.IsAllDay)
								{
									var period = DescribePeriod(item);
									var periodBounds = TextMeasurer.MeasureBounds(period, textMeasureOptions);
									canvas.Invert(new Rectangle(indent - 2, y + 5, (int)periodBounds.Width + 4, (int)periodBounds.Height + 2));
								}

								y += (int)TextMeasurer.MeasureBounds(line, textMeasureOptions).Height - 4 + (int)(font.LineHeight / 200);
							}
							catch (Exception ex)
							{
								//Log?
								try
								{
									var error = $"Error: {ex.Message}".Limit(150);
									y += (int)TextMeasurer.MeasureBounds(error, textMeasureOptions).Height - 4 + (int)(font.LineHeight / 200);
									canvas.DrawText(options, error, font, Color.Red, new PointF(indent, y));
									y += (int)TextMeasurer.MeasureBounds(error, textMeasureOptions).Height - 4 + (int)(font.LineHeight / 200);
								}
								catch
								{
									//Ignore,log?
								}
							}
						});
					});

			});

			//This previously was the source for rendering
			var text = DescribeCalender(characterPerLine, items);
			Trace.WriteLine(text);

			return result;
		}

		private static string DescribeCalender(int characterPerLine, IEnumerable<CalendarEvent> items)
		{
			return string.Join(
							Environment.NewLine,
							items
								.GroupBy(x => x.Start.Date)
								.OrderBy(x => x.Key)
								.Select(x =>
								{
									var dayEvents = new StringBuilder($"{x.Key:ddd dd MMM} ");

									var indentSize = dayEvents.Length;

									dayEvents.Append(
										string.Join(
										Environment.NewLine + "".PadLeft(indentSize), x.Select(item =>
										{
											return DescribeEvent(item, characterPerLine, indentSize);

										})));
									return dayEvents.ToString();
								}));
		}

		private static string DescribeEvent(CalendarEvent item, int? characterPerLine = null, int indentSize = 0)
		{
			var calendarName = item.Calendar.Properties["X-WR-CALNAME"]?.Value;
			var period = DescribePeriod(item);

			var remainingSize = characterPerLine - (period.Length + indentSize + 1);

			var summary = characterPerLine.GetValueOrDefault() > 0
							? remainingSize > 3
								? item.Summary.Limit(remainingSize.Value, " ...")
								: string.Empty
							: item.Summary;

			return $"{period} {summary}";
		}

		private static string DescribePeriod(CalendarEvent item)
		{
			return $@"{(item.Start.HasTime
							? $"{item.Start.Value:HH:mm} - {item.End.Value:HH:mm}"
							: $"All day")}";
		}
	}
}

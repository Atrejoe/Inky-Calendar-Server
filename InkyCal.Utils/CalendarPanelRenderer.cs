using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InkyCal.Utils.Calendar;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using static InkyCal.Utils.FontHelper;

namespace InkyCal.Utils
{

	/// <summary>
	/// A panel that shows one or more calendars
	/// </summary>
	public class CalendarPanelRenderer : IPanelRenderer
	{

		/// <summary>
		/// Shows a single calendar
		/// </summary>
		/// <param name="iCalUrl"></param>
		public CalendarPanelRenderer(Uri iCalUrl) : this(new[] { iCalUrl })
		{
			if (iCalUrl is null)
				throw new ArgumentNullException(nameof(iCalUrl));

		}
		/// <summary>
		/// Show one or more calendars
		/// </summary>
		/// <param name="iCalUrls"></param>
		public CalendarPanelRenderer(Uri[] iCalUrls)
		{
			ICalUrls = new ReadOnlyCollection<Uri>(iCalUrls) ?? throw new ArgumentNullException(nameof(iCalUrls));
		}

		/// <summary>
		/// The calendars to render
		/// </summary>
		public ReadOnlyCollection<Uri> ICalUrls { get; }


		/// <summary>
		/// Renders the calendars in portrait mode (flipping <paramref name="width"/> and <paramref name="height"/>)
		/// </summary>
		/// <param name="width">The height of the panel (in landscape mode).</param>
		/// <param name="height">The width of the panel (in landscape mode).</param>
		/// <param name="colors">The number of color to render in.</param>
		/// <param name="log">A callbac method for logging errors to</param>
		/// <returns></returns>
		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Intentional catch-all")]
		public async Task<Image> GetImage(int width, int height, Color[] colors, IPanelRenderer.Log log)
		{
			if (colors is null)
				colors = new[] { Color.Black, Color.White };

			var primaryColor = colors.FirstOrDefault();
			var supportColor = colors.Length > 2 ? colors[2] : primaryColor;
			var errorColor = supportColor;
			var backgroundColor = colors.Skip(1).First();

			var result = new Image<Rgba32>(new Configuration() { }, width, height, backgroundColor);

			var font = MonteCarlo.CreateFont(12); //Font that works well anti-aliassed

			var characterWidth = font.GetCharacterWidth(); //Works only for some known fixed-width fonts

			var characterPerLine = characterWidth > 0
								? width / characterWidth.Value
								: width / 7; //For now fall back to 100 characters width, which is nonsense

			var sbErrors = new StringBuilder();

			var events = await GetEvents(sbErrors);

			var options_Date = new TextGraphicsOptions(false)
			{
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				WrapTextWidth = width,
				DpiX = 96,
				DpiY = 96
			};

			var textRendererOptions_Date = options_Date.ToRendererOptions(font);

			//Start drawing while iterating through items
			result.Mutate(canvas =>
			{
				//Keep track of vertical position
				var y = 0;

				//Draw calender parsing errors (in a red panel) first
				if (sbErrors.Length > 0)
				{
					var errorMessage = sbErrors.ToString();

					var errorRenderOptions = textRendererOptions_Date.Clone();

					canvas.RenderErrorMessage(
						errorMessage,
						errorColor,
						backgroundColor,
						ref y,
						width, errorRenderOptions);
				}

				//This previously was the source for rendering
				var text = DescribeCalender(characterPerLine, events);
				Trace.WriteLine(text);


				//Group by day, then show events

				var firstEntry = true;
				events
					.GroupBy(x => x.Date)
					.OrderBy(x => x.Key)
					.ToList()
					.ForEach(x =>
					{
						if (y > height)
							return;

						var lineDrawn = false;

						//Write the day part only once for all events within a day
						var day = x.Key.Year == DateTime.Now.Year
							? @$"{x.Key:ddd dd MMM} "
							: @$"{x.Key:ddd dd MMM `yy} ";

						var indentSize = day.Length;

						int indent = (int)TextMeasurer.MeasureBounds(day, textRendererOptions_Date).Width
								   + 10; //Space of 10 pixels

						var options = options_Date.Clone();
						options.WrapTextWidth = width - indent;

						var textMeasureOptions = options.ToRendererOptions(font);

						//Then write each event, wrap the summary
						x.OrderBy(x => x.Start)
							.ThenBy(x => x.End)
							.ThenBy(x => x.Summary)
							.ToList()
							.ForEach(item =>
						{
							//When summary is very long, cut if off
							var line = DescribeEvent(item).Limit(500, " ...");

							//Gain some metadata for logging purposes
							var metaData = item.SerializeToDictionary();
							metaData["DescribedEvent"] = line;
							metaData["DescribedDay"] = day;
							metaData["y"] = $"{y}";

							PerformanceMonitor.Trace("Processing event", metaData);

							try
							{
								var textHeight = (int)TextMeasurer.MeasureBounds(line, textMeasureOptions).Height - 4 + font.LineHeight / 200;

								if (textHeight + y > height)
									return;


								if (!lineDrawn)
								{

									if (firstEntry)
										firstEntry = false;
									else
									{
										//Draw a red line for each day
										y += 4;

										canvas.DrawLines(supportColor, 2, new[] { new PointF(2, y), new PointF(width - 2, y) });

										firstEntry = false;
									}

									canvas.DrawText(options_Date, day, font, primaryColor, new PointF(0, y));

									lineDrawn = true;
								}

								canvas.DrawText(
										options: options,
										text: line.Trim()
											.Replace("é", "e", StringComparison.OrdinalIgnoreCase), //Todo: make diacritics safe
										font: font,
										color: primaryColor,
										location: new PointF(indent, y)
										);

								//Hilight special segments
								if (item.IsAllDay
								|| (item.IsNow() && backgroundColor != supportColor) //Only highlight now when the highlight color is distinct from 'all day'
								)
								{
									//Fill the boundary of the time indication
									var period = DescribePeriod(item);
									var periodBounds = TextMeasurer.MeasureBounds(period, textMeasureOptions);

									var rectangle = new Rectangle(
										indent - 2,
										y + 5,
										(int)periodBounds.Width + 4,
										(int)periodBounds.Height + 2
										);

									canvas.Fill(item.IsAllDay ? primaryColor : supportColor, rectangle);

									// Write the time indication again, but in the background color (this is part of line (== DescribeEvent))
									canvas.DrawText(
										options: options,
										text: period,
										font: font,
										color: backgroundColor,
										location: new PointF(indent, y)
										);
								}

								y += textHeight;
							}
							catch (Exception ex)
							{
								ex.Log();
								try
								{
									var error = $"Error: {(ex.InnerException == null ? ex.Message : ex.InnerException.Message)}".Limit(150);
									y += (int)TextMeasurer.MeasureBounds(error, textMeasureOptions).Height - 4 + font.LineHeight / 200;
									canvas.DrawText(options, error, font, errorColor, new PointF(indent, y));
									y += (int)TextMeasurer.MeasureBounds(error, textMeasureOptions).Height - 4 + font.LineHeight / 200;
								}
								catch (Exception errorDisplayException)
								{
									errorDisplayException.Log();
								}
							}
						});
					});

			});


			return result;
		}

		/// <summary>
		/// Gets the events
		/// </summary>
		/// <param name="sbErrors">The sb errors.</param>
		/// <returns></returns>
		protected virtual async Task<List<Event>> GetEvents(StringBuilder sbErrors)
		{
			if (sbErrors is null)

				throw new ArgumentNullException(nameof(sbErrors));


			return await CalenderExtensions.GetEvents(sbErrors, ICalUrls);
		}


		private static string DescribeCalender(int characterPerLine, IEnumerable<Event> items)
		{
			return string.Join(
							Environment.NewLine,
							items
								.GroupBy(x => x.Date)
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

		private static string DescribeEvent(Event item, int? characterPerLine = null, int indentSize = 0)
		{
			//var calendarName = item.CalendarName;
			var period = DescribePeriod(item);

			var remainingSize = characterPerLine - (period.Length + indentSize + 1);

			var summary = characterPerLine.GetValueOrDefault() > 0
							? remainingSize > 3
								? item.Summary.Limit(remainingSize.Value, " ...")
								: string.Empty
							: item.Summary;

			return $"{period} {summary}";
		}

		private static string DescribePeriod(Event item)
		{
			if (item is null)
				return "No event?";

			return $@"{(item.Start.HasValue
							?
							 item.End.HasValue
								? @$"{item.Start.Value:hh\:mm} - {item.End.Value:hh\:mm}"
								: @$"{item.Start.Value:hh\:mm}"
							: $"All day")}";
		}
	}
}

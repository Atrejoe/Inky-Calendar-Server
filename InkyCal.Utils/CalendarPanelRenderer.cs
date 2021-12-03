using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InkyCal.Models;
using InkyCal.Utils.Calendar;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using StackExchange.Profiling;
using static InkyCal.Utils.FontHelper;

namespace InkyCal.Utils
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="InkyCal.Models.PanelCacheKey" />
	public class CalendarPanelCacheKey : PanelCacheKey
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CalendarPanelCacheKey"/> class.
		/// </summary>
		/// <param name="expiration">The expiration.</param>
		/// <param name="iCalUrls">The i cal urls.</param>
		/// <param name="subscribedCalenders"></param>
		public CalendarPanelCacheKey(TimeSpan expiration, Uri[] iCalUrls, SubscribedGoogleCalender[] subscribedCalenders) : base(expiration)
		{
			ICalUrls = iCalUrls.OrderBy(x => x.ToString()).ToList().AsReadOnly();
			SubscribedGoogleCalenders = (subscribedCalenders?.OrderBy(x => x.IdAccessToken)?.ThenBy(x => x.Calender)?.ToArray() ?? Array.Empty<SubscribedGoogleCalender>()).ToList().AsReadOnly();
		}

		/// <summary>
		/// Gets the iCal urls.
		/// </summary>
		/// <value>
		/// The i cal urls.
		/// </value>
		public ReadOnlyCollection<Uri> ICalUrls { get; }

		/// <summary>
		/// Get the subscribed Google calenders
		/// </summary>
		public ReadOnlyCollection<SubscribedGoogleCalender> SubscribedGoogleCalenders { get; }

		/// <summary>
		/// Indicates whether the current object is equal to another <see cref="T:InkyCal.Models.PanelCacheKey" /> (or derived class))
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
		/// </returns>
		protected override bool Equals(PanelCacheKey other)
		{
			return other is CalendarPanelCacheKey cpc
				&& ICalUrls.SequenceEqual(cpc.ICalUrls)
				&& SubscribedGoogleCalenders.SequenceEqual(cpc.SubscribedGoogleCalenders);
		}
	}

	/// <summary>
	/// A panel that shows one or more calendars
	/// </summary>
	public class CalendarPanelRenderer : IPanelRenderer
	{
		private readonly SubscribedGoogleCalender[] Calendars;

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
		/// <param name="calendars"></param>
		public CalendarPanelRenderer(Uri[] iCalUrls, SubscribedGoogleCalender[] calendars = null)
		{
			ICalUrls = new ReadOnlyCollection<Uri>(iCalUrls) ?? throw new ArgumentNullException(nameof(iCalUrls));
			CacheKey = new CalendarPanelCacheKey(TimeSpan.FromMinutes(1), iCalUrls, calendars);
			Calendars = calendars?.ToArray();
		}

		/// <summary>
		/// The calendars to render
		/// </summary>
		public ReadOnlyCollection<Uri> ICalUrls { get; }

		/// <summary>
		/// Gets the cache key.
		/// </summary>
		public PanelCacheKey CacheKey { get; }


		/// <summary>
		/// Renders the calendars in portrait mode (flipping <paramref name="width"/> and <paramref name="height"/>)
		/// </summary>
		/// <param name="width">The height of the panel (in landscape mode).</param>
		/// <param name="height">The width of the panel (in landscape mode).</param>
		/// <param name="colors">The number of color to render in.</param>
		/// <param name="log">A callbac method for logging errors to</param>
		/// <returns></returns>
		public async Task<Image> GetImage(int width, int height, Color[] colors, IPanelRenderer.Log log)
		{

			var sbErrors = new StringBuilder();

			List<Event> events;
			using (MiniProfiler.Current.Step("Gather events"))
				events = await GetEvents(sbErrors);

			if (!(events?.Any()).GetValueOrDefault())
				sbErrors.AppendLine($"No events listed");

			var result = DrawImage(width, height, colors, events, sbErrors.ToString(), log);

			return result;
		}

		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Intentional catch-all")]
		internal static Image<Rgba32> DrawImage(
			int width,
			int height,
			Color[] colors,
			List<Event> events,
			string calenderParseErrors,
			IPanelRenderer.Log log)
		{
			if (colors is null)
				colors = new[] { Color.Black, Color.White };

			var primaryColor = colors.FirstOrDefault();
			var supportColor = colors.Length > 2 ? colors[2] : primaryColor;
			var errorColor = supportColor;
			var backgroundColor = colors.Skip(1).First();


			//var font = MonteCarlo.CreateFont(12); //Font that works well anti-aliassed
			var font = ProFont.CreateFont(15);

			var characterWidth = font.GetCharacterWidth(); //Works only for some known fixed-width fonts

			var characterPerLine = characterWidth > 0
								? width / characterWidth.Value
								: width / 7; //For now fall back to 100 characters width, which is nonsense




			var options_Date = new TextGraphicsOptions(new GraphicsOptions() { Antialias = false }, new TextOptions()
			{
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				WrapTextWidth = width,
				DpiX = 96,
				DpiY = 96
			});

			var textRendererOptions_Date = options_Date.ToRendererOptions(font);

			var result = new Image<Rgba32>(new Configuration() { }, width, height, backgroundColor);

			//Keep track of vertical position
			var y = 0;

			//Start drawing while iterating through items
			result.Mutate(canvas =>
			{
				//Draw calender parsing errors (in a red panel) first
				if (!string.IsNullOrEmpty(calenderParseErrors))
				{
					var errorMessage = calenderParseErrors;

					var errorRenderOptions = textRendererOptions_Date.Clone();

					canvas.RenderErrorMessage(
						errorMessage,
						errorColor,
						backgroundColor,
						ref y,
						width, errorRenderOptions);
				}
			});

			result.Mutate(canvas =>
			{

				//This previously was the source for rendering
				var text = DescribeCalender(characterPerLine, events);
				Trace.WriteLine(text);

				//Group by day, then show events
				var firstEntry = true;

				using (MiniProfiler.Current.Step("Drawing events"))
				{
					events.ToHashSet()
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
							options.TextOptions.WrapTextWidth = width - indent;

							var textMeasureOptions = options.ToRendererOptions(font);

							using (MiniProfiler.Current.Step($"Drawing {x.Count():n0} events for {x.Key:d}"))

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

											canvas.DrawText(
												options_Date,
												day.ToSafeChars(font),
												font,
												primaryColor,
												new PointF(0, y));

											lineDrawn = true;
										}

										canvas.DrawText(
												options: options,
												text: line.Trim().ToSafeChars(font),
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
													text: period.ToSafeChars(font),
													font: font,
													color: backgroundColor,
													location: new PointF(indent, y)
													);
										}

										y += textHeight;
									}
									catch (Exception ex)
									{
										ex.Data["DescribedEvent_Length"] = line?.Length;
										ex.Data["DescribedEvent"] = line;
										ex.Data["DescribedDay"] = day;
										ex.Data["y"] = y;
										log(ex, true, "Error message is shown, but display is not aborted");

										try
										{
											var error = $"Error: ({ex.GetType().Name}) {(ex.InnerException == null ? ex.Message : ex.InnerException.Message)}".Limit(150);
											canvas.DrawText(options, error.ToSafeChars(font), font, errorColor, new PointF(indent, y));
											y += (int)TextMeasurer.MeasureBounds(error, textMeasureOptions).Height - 4 + font.LineHeight / 200;
										}
										catch (Exception errorDisplayException)
										{
											log(errorDisplayException, false, "Error message is not shown");
										}
									}
								});
						});
				}

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

			var result = new List<Event>();


			if ((ICalUrls?.Any()).GetValueOrDefault())
				result.AddRange(await iCalExtensions.GetEvents(sbErrors, ICalUrls));

			if ((Calendars?.Any()).GetValueOrDefault())
				result.AddRange(await GoogleCalenderExtensions.GetEvents(sbErrors, Calendars));

			if (!(ICalUrls?.Any()).GetValueOrDefault()
				&& !(Calendars?.Any()).GetValueOrDefault())
				sbErrors.AppendLine($"No iCal urls nor Google oAuth calenders were linked.");


			return result;
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

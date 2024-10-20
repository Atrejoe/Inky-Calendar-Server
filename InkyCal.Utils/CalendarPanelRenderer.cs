﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using InkyCal.Models;
using InkyCal.Utils.Calendar;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Images;
using OpenAI.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using StackExchange.Profiling;
using static InkyCal.Utils.FontHelper;
using Event = InkyCal.Utils.Calendar.Event;

namespace InkyCal.Utils
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="PanelCacheKey" />
	/// <remarks>
	/// Initializes a new instance of the <see cref="CalendarPanelCacheKey"/> class.
	/// </remarks>
	/// <param name="expiration">The expiration.</param>
	/// <param name="iCalUrls">The i cal urls.</param>
	/// <param name="subscribedCalenders"></param>
	/// <param name="drawMode"></param>
	public class CalendarPanelCacheKey(TimeSpan expiration, Uri[] iCalUrls, SubscribedGoogleCalender[] subscribedCalenders, CalenderDrawMode drawMode) : PanelCacheKey(expiration)
	{

		/// <summary>
		/// Gets the iCal urls.
		/// </summary>
		/// <value>
		/// The i cal urls.
		/// </value>
		public ReadOnlyCollection<Uri> ICalUrls { get; } = iCalUrls.OrderBy(x => x.ToString()).ToList().AsReadOnly();

		/// <summary>
		/// Get the subscribed Google calenders
		/// </summary>
		public ReadOnlyCollection<SubscribedGoogleCalender> SubscribedGoogleCalenders { get; } = (subscribedCalenders?.OrderBy(x => x.IdAccessToken).ThenBy(x => x.Calender).ToArray() ?? Array.Empty<SubscribedGoogleCalender>()).ToList().AsReadOnly();

		/// <summary>
		/// Draw mode
		/// </summary>
		public CalenderDrawMode DrawMode { get; } = drawMode;

		/// <summary>
		/// Indicates whether the current object is equal to another <see cref="T:InkyCal.Models.PanelCacheKey" /> (or derived class))
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
		/// </returns>
		protected override bool Equals(PanelCacheKey other) => other is CalendarPanelCacheKey cpc
				&& DrawMode.Equals(cpc.DrawMode)
				&& ICalUrls.SequenceEqual(cpc.ICalUrls)
				&& SubscribedGoogleCalenders.SequenceEqual(cpc.SubscribedGoogleCalenders);
	}

	/// <summary>
	/// A panel that shows one or more calendars
	/// </summary>
	public class CalendarPanelRenderer : IPanelRenderer
	{
		private readonly SubscribedGoogleCalender[] Calendars;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="saveToken"></param>
		private CalendarPanelRenderer(Func<GoogleOAuthAccess, CancellationToken, Task> saveToken) => SaveToken = saveToken;

		/// <summary>
		/// Shows a single calendar
		/// </summary>
		/// <param name="saveToken"></param>
		/// <param name="iCalUrl"></param>
		/// <param name="drawMode">Indicates how the image should be drawn</param>
		public CalendarPanelRenderer(Func<GoogleOAuthAccess, CancellationToken, Task> saveToken, Uri iCalUrl, CalenderDrawMode drawMode = CalenderDrawMode.List) : this(saveToken, [iCalUrl], [], drawMode) 
			=> ArgumentNullException.ThrowIfNull(iCalUrl);

		/// <summary>
		/// Show one or more calendars
		/// </summary>
		/// <param name="saveToken"></param>
		/// <param name="iCalUrls"></param>
		/// <param name="calendars"></param>
		/// <param name="drawMode">Indicates how the image should be drawn</param>
		public CalendarPanelRenderer(Func<GoogleOAuthAccess, CancellationToken, Task> saveToken, Uri[] iCalUrls, SubscribedGoogleCalender[] calendars, CalenderDrawMode drawMode) : this(saveToken)
		{
			ICalUrls = new ReadOnlyCollection<Uri>(iCalUrls);

			var cacheExpiration = drawMode switch
			{
				// Cache (way) more aggressively for AI images
				// Next up: OpenAI keys should be personal and not global
				CalenderDrawMode.AIImage => TimeSpan.FromHours(7),
				_ => TimeSpan.FromMinutes(1)
			};

			CacheKey = new CalendarPanelCacheKey(cacheExpiration, iCalUrls, calendars, drawMode);
			Calendars = calendars?.ToArray();
			DrawMode = string.IsNullOrEmpty(Server.Config.Config.OpenAIAPIKey)
						? CalenderDrawMode.List //OpenAPI has not been configured
						: drawMode;
		}

		/// <summary>
		/// The calendars to render
		/// </summary>
		public ReadOnlyCollection<Uri> ICalUrls { get; }

		/// <summary>
		/// Gets the cache key.
		/// </summary>
		public virtual PanelCacheKey CacheKey { get; }

		/// <summary>
		/// The mode of drawing
		/// </summary>
		public CalenderDrawMode DrawMode { get; private set; }


		/// <summary>
		/// Renders the calendars in portrait mode (flipping <paramref name="width"/> and <paramref name="height"/>)
		/// </summary>
		/// <param name="width">The height of the panel (in landscape mode).</param>
		/// <param name="height">The width of the panel (in landscape mode).</param>
		/// <param name="colors">The number of color to render in.</param>
		/// <param name="log">A callback method for logging errors to</param>
		/// <returns></returns>
		public async Task<Image> GetImage(int width, int height, Color[] colors, IPanelRenderer.Log log)
			=> await GetImage(width, height, colors, log, default);
		/// <summary>
		/// Renders the calendars in portrait mode (flipping <paramref name="width"/> and <paramref name="height"/>)
		/// </summary>
		/// <param name="width">The height of the panel (in landscape mode).</param>
		/// <param name="height">The width of the panel (in landscape mode).</param>
		/// <param name="colors">The number of color to render in.</param>
		/// <param name="log">A callback method for logging errors to</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<Image> GetImage(int width, int height, Color[] colors, IPanelRenderer.Log log, CancellationToken cancellationToken)
		{

			var sbErrors = new StringBuilder();

			List<Event> events;
			using (MiniProfiler.Current.Step("Gather events"))
				events = await GetEvents(sbErrors, SaveToken, cancellationToken);

			if (!(events?.Any()).GetValueOrDefault())
				sbErrors.AppendLine($"No events listed");

			using (MiniProfiler.Current.Step($"Rendering image for draw mode: {DrawMode}"))
				return DrawMode switch
				{
					CalenderDrawMode.AIImage => await DrawDallEImage(width, height, colors, events, sbErrors.ToString(), log),
					CalenderDrawMode.List => DrawImage(width, height, colors, events, sbErrors.ToString(), log),
					_ => throw new NotImplementedException($"DrawMode {DrawMode} is not implemented"),
				};
		}

		/// <summary>
		/// 
		/// </summary>
		protected internal readonly Func<GoogleOAuthAccess, CancellationToken, Task> SaveToken;

		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Intentional catch-all")]
		internal static Image<Rgba32> DrawImage(
			int width,
			int height,
			Color[] colors,
			List<Calendar.Event> events,
			string calenderParseErrors,
			IPanelRenderer.Log log)
		{
			colors ??= [Color.Black, Color.White];

			var primaryColor = colors.FirstOrDefault();
			var supportColor = colors.Length > 2 ? colors[2] : primaryColor;
			var errorColor = supportColor;
			var backgroundColor = colors.Skip(1).First();


			//var font = MonteCarlo.CreateFont(15); //Font that works well anti-aliassed
			var font = ProFont.CreateFont(15);

			var characterWidth = font.GetCharacterWidth(); //Works only for some known fixed-width fonts

			var characterPerLine = characterWidth > 0
								? width / characterWidth.Value
								: width / 7; //For now fall back to 100 characters width, which is nonsense


			var textOptions_Date = new RichTextOptions(font)
			{
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				WrappingLength = width,
				Dpi = 96
				//DpiX = 96,
				//DpiY = 96
			};

			//var textRendererOptions_Date = options_Date.ToRendererOptions(font);

			var result = new Image<Rgba32>(width, height, backgroundColor);

			//Keep track of vertical position
			var y = 0;

			//Start drawing while iterating through items
			result.Mutate(canvas =>
			{
				//Draw calender parsing errors (in a red panel) first
				if (!string.IsNullOrEmpty(calenderParseErrors))
				{
					var errorMessage = calenderParseErrors;

					canvas.RenderErrorMessage(
						errorMessage,
						errorColor,
						backgroundColor,
						ref y,
						width, textOptions_Date);
				}
			});

			result.Mutate(canvas =>
			{
				canvas.GetDrawingOptions().GraphicsOptions.Antialias = false;

				//This previously was the source for rendering
				var text = DescribeCalender(characterPerLine, events);
				Trace.TraceInformation(text);

				using (MiniProfiler.Current.Step("Drawing events"))
				{
					events.ToHashSet()
						.GroupBy(x => x.Date)
						.OrderBy(x => x.Key)
						.ToList()
						.ForEach(x =>
						{
							if (y > height || !x.Any())
								return;

							var dayWritten = false;

							//Write the day part only once for all events within a day
							var day = x.Key.Year == DateTime.Now.Year
									? @$"{x.Key:ddd dd MMM} "
									: @$"{x.Key:ddd dd MMM `yy} ";

							//var indentSize = day.Length;

							int indent = (int)TextMeasurer.MeasureBounds(day, textOptions_Date).Width
									   + 5; //Padding of 5 pixels

							var textOptions = new RichTextOptions(textOptions_Date) { WrappingLength = width - indent };

							//var textMeasureOptions = options.ToRendererOptions(font);

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
									//var metaData = item.SerializeToDictionary();
									//metaData["DescribedEvent"] = line;
									//metaData["DescribedDay"] = day;
									//metaData["y"] = $"{y}";

									//PerformanceMonitor.Trace("Processing event", metaData);

									try
									{
										var textHeight = (int)TextMeasurer.MeasureBounds(line, textOptions).Height;// - 4 + font.FontMetrics.VerticalMetrics.LineHeight / 200;

										if (textHeight + y > height)
											return;

										if (!dayWritten)
										{
											canvas.DrawText(
												textOptions: new RichTextOptions(textOptions_Date) { Origin = new PointF(0, y) },
												text: day.ToSafeChars(font),
												color: primaryColor);

											dayWritten = true;
										}

										if (textHeight + y > height)
											return;

										var dateOptions = new RichTextOptions(textOptions_Date) { Origin = new PointF(indent, y), WrappingLength = width - indent };
										canvas.DrawText(
												textOptions: dateOptions,
												text: line.Trim().ToSafeChars(font),
												color: primaryColor
												);

										y += textHeight;

										//Hilight special segments
										if (item.IsAllDay
											|| (item.IsNow() && backgroundColor != supportColor) //Only highlight now when the highlight color is distinct from 'all day'
											)
										{
											//Fill the boundary of the time indication
											var period = DescribePeriod(item);
											var periodBounds = TextMeasurer.MeasureBounds(period, dateOptions);

											var rectangle = new Rectangle(
												new Point((int)periodBounds.X - 1, (int)periodBounds.Y - 1),
												new(
													width: (int)periodBounds.Width + 2,
													height: (int)periodBounds.Height + 3
												));

											canvas.Fill(item.IsAllDay ? primaryColor : supportColor, rectangle)

												// Write the time indication again, but in the background color (this is part of line (== DescribeEvent))
												.DrawText(
													textOptions: dateOptions,
													text: period.ToSafeChars(font),
													color: backgroundColor
													);

											// The wrapper around 
											//y += 2;
										}
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
											canvas.DrawText(error.ToSafeChars(font), font, errorColor, new PointF(indent, y));
											y += (int)TextMeasurer.MeasureBounds(error, textOptions).Height - 4 + font.FontMetrics.VerticalMetrics.LineHeight / 200;
										}
										catch (Exception errorDisplayException)
										{
											log(errorDisplayException, false, "Error message is not shown");
										}
									}
								});

							if (dayWritten // Indicated anyhting happened in the day
							&& (y + 3 + 2 + TextMeasurer.MeasureBounds("A", textOptions).Height) < height) // When a date would fit on the new line
							{
								y += 3;
								canvas.DrawLine(supportColor, 2, new PointF(2, y), new PointF(width - 2, y));
								y += 2;
							}
						});
				}

			});
			return result;
		}


		private static readonly SemaphoreSlim semaphoreOpenAIAPI = new(initialCount: 1, maxCount: 1); //Make rate limits configurable

		[SuppressMessage("Major Code Smell", "S112:General or reserved exceptions should never be thrown", Justification = "<Pending>")]
		private static async Task WaitForImageGenerationSlot()
		{
			var lease = await limiter.AcquireAsync();
			if (!lease.IsAcquired)
				throw new Exception("Could not acquire slot");
		}

		private static readonly RateLimiter limiter =
			new SlidingWindowRateLimiter(
				new SlidingWindowRateLimiterOptions()
				{
					Window = TimeSpan.FromSeconds(30),
					PermitLimit = 2,
					QueueLimit = 100,
					QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
					SegmentsPerWindow = 60
				});


		/// <summary>
		/// AI-powered image describing events, ported from https://turunen.dev/2023/11/20/Kuvastin-Unhinged-AI-eink-display/
		/// </summary>
		/// <returns></returns>
		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Intentional catch-all")]
		internal static async Task<Image<Rgba32>> DrawDallEImage(
			int width,
			int height,
			Color[] colors,
			List<Event> events,
			string calenderParseErrors,
			IPanelRenderer.Log log,
			CancellationToken token = default)
		{


			colors ??= [Color.Black, Color.White];

			var key = Server.Config.Config.OpenAIAPIKey;

			var api = new OpenAIClient(key);

			//events = new List<Event>(new[] {
			//	new Event() { Summary = "All day : Work" },
			//	new Event() { Summary = "15:30 Pick up kids from school, shop for presents" },
			//	new Event() { Summary = "17:00 Robert cooks dinner" },
			//	new Event() { Summary = "20:00 Robert climbing" }
			//});

			var day = events.Select(x => x.Date.Date).Distinct().OrderBy(x => x).FirstOrDefault(DateTime.Now);

			events = events.Where(x => x.Date.Date == day).ToList();

			var prompt = $@"This is todays calendar:
- {string.Join($"{Environment.NewLine}- ", events.Select(x => x.Summary))}
Use the following colors: 
- {string.Join($"{Environment.NewLine}- ", colors.Select(x => x.ColorName()))}";

			var messages = new List<Message>(new[] {
				new Message(Role.System, @$"You are a master prompt maker for Dalle.
You specialize in creating 19th century metal litograph images in the style of a vintage engraving, caravaggesque, flickr, ultrafine detail, neoclassicism based on my calendar entries.
You are creative, hide allegories and details. You provide prompts based on the calender events provided, which all are on {day:D}.
The image should be in a style of 19th century litograph or metal plate print as would be seen in an old book or newspaper and include the colors requested by the user."),
				new Message(Role.User, prompt)
			});

			ImageResult imageResult;

			await semaphoreOpenAIAPI.WaitAsync(token); //Respect rate limits for concurrent access
			try
			{
				var chatRequest = new ChatRequest(messages);

				ChatResponse response;
				try
				{
					using (MiniProfiler.Current.Step($"Getting prompt for image generation"))
						response = await api.ChatEndpoint.GetCompletionAsync(chatRequest, token);
				} catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
				{
					Trace.TraceError("Too many requests to OpenAI");
					throw;
				}

				var imagePrompt = response.FirstChoice.Message;

				Trace.TraceInformation(prompt);
				Trace.TraceInformation(imagePrompt);

				//var imageResult = await api.ImageGenerations.CreateImageAsync(
				//	new ImageGenerationRequest(
				//		prompt: imagePrompt,
				//		numOfImages: 1,
				//		size: ImageSize._1024,
				//		responseFormat: ImageResponseFormat.B64_json));

				var request = new ImageGenerationRequest(imagePrompt, Model.DallE_3, responseFormat: ImageResponseFormat.Url);

				using (MiniProfiler.Current.Step($"Waiting for image generation slot"))
					await WaitForImageGenerationSlot();
				using (MiniProfiler.Current.Step($"Generating image"))
					imageResult = (await api.ImagesEndPoint.GenerateImageAsync(request, token))[0];


			}
			finally
			{
				semaphoreOpenAIAPI.Release();
			}

			Image<Rgba32> result;
			try
			{
				using var client = new System.Net.Http.HttpClient();
				var url = imageResult.Url;
				using var ms = new MemoryStream();
				using (MiniProfiler.Current.Step($"Downloading image url: {url}"))
				{
					using var s = await client.GetStreamAsync(url, token);
					await s.CopyToAsync(ms, token);
				}

				//var i = Image.Load(ms);
				//Console.WriteLine(i.Metadata.ToString());
				//byte[] data = Convert.FromBase64String(imageResult);
				ms.Position = 0;
				result = await Image.LoadAsync<Rgba32>(ms);
			}
			catch (Exception ex)
			{
				await Console.Error.WriteLineAsync(ex.Message)
						.ContinueWith(async x => await Console.Error.WriteLineAsync(ex.StackTrace))
						.ContinueWith(async x => await Console.Error.WriteLineAsync(ex.InnerException?.Message))
						.ContinueWith(async x => await Console.Error.WriteLineAsync(ex.InnerException?.StackTrace));
				throw;
			}

			result.Mutate(x => x
						.EntropyCrop()
						.Resize(new ResizeOptions() { Mode = ResizeMode.Crop, Size = new Size(width, height), Position = AnchorPositionMode.Center })
						.BackgroundColor(Color.Transparent)
						.Quantize(new PaletteQuantizer(colors))
						);

			return result;
		}

		/// <summary>
		/// Gets the events
		/// </summary>
		/// <param name="sbErrors">The sb errors.</param>
		/// <param name="saveToken"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		protected virtual async Task<List<Event>> GetEvents(StringBuilder sbErrors, Func<GoogleOAuthAccess, CancellationToken, Task> saveToken, CancellationToken cancellationToken)
		{
			ArgumentNullException.ThrowIfNull(sbErrors);

			var result = new List<Event>();

			//Get iCal-based events
			if ((ICalUrls?.Any()).GetValueOrDefault())
				using (MiniProfiler.Current.Step($"Gather events for {ICalUrls.Count} iCal based calendars"))
					result.AddRange(await ICalExtensions.GetEvents(sbErrors, ICalUrls));

			//Get Google Calender-based events
			if (InkyCal.Server.Config.GoogleOAuth.Enabled
				&& (Calendars?.Any()).GetValueOrDefault())
				using (MiniProfiler.Current.Step($"Gather events for {Calendars.Length} Google calendars"))
				{
					var events = (await GoogleCalenderExtensions.GetEvents(sbErrors, Calendars, saveToken, cancellationToken)).ToList();
					result.AddRange(events);
				}

			if (!(ICalUrls?.Any()).GetValueOrDefault()
				&& (!InkyCal.Server.Config.GoogleOAuth.Enabled
					|| (!Calendars?.Any()).GetValueOrDefault())
				)
				sbErrors.AppendLine($"No iCal urls nor Google oAuth calenders were linked.");


			return result;
		}

		private static string DescribeCalender(int characterPerLine, IEnumerable<Event> items)
			=> string.Join(
						Environment.NewLine,
						items
							.Where(x => x != null)
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="item"></param>
		/// <param name="characterPerLine"></param>
		/// <param name="indentSize"></param>
		/// <returns></returns>
		public static string DescribeEvent(Event item, int? characterPerLine = null, int indentSize = 0)
		{
			var period = DescribePeriod(item);

			var remainingSize = characterPerLine - (period.Length + indentSize + 1);

			var summary = ReduceSummary(item.Summary, remainingSize);
			return $"{period} {summary}".Trim();
		}

		/// <summary>
		/// Reduces a summary
		/// </summary>
		/// <param name="originalSummary"></param>
		/// <param name="remainingSize"></param>
		/// <returns></returns>
		public static string ReduceSummary(string originalSummary, int? remainingSize)
		{
			if (!remainingSize.HasValue)
				return originalSummary;
			else if (remainingSize > 3)
				return originalSummary.Limit(remainingSize.Value, " ...");
			else
				return string.Empty;
		}

		private static string DescribePeriod(Event item)
		{
			if (item is null)
				return "No event?";

			return DescribePeriod(item.Start, item.End);
		}

		/// <summary>
		/// Describes a period (in english for now)
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		[SuppressMessage("Major Code Smell", "S3358:Ternary operators should not be nested", Justification = "Reads just fine")]
		public static string DescribePeriod(TimeSpan? start, TimeSpan? end)
			=> start.HasValue
					? end.HasValue
						? @$"{start.Value:hh\:mm} - {end.Value:hh\:mm}"
						: @$"{start.Value:hh\:mm}"
					: $"All day";
	}
}

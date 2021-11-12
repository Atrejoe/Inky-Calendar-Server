using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Oauth2.v2;
using Google.Apis.Requests;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Profiling;

namespace InkyCal.Utils.Calendar
{
	/// <summary>
	/// A helper class for obtaining calender info
	/// </summary>
	public static partial class CalenderExtensions
	{
		private static readonly HttpClient client = new HttpClient();

		internal static async Task<List<Event>> GetEvents(StringBuilder sbErrors, IEnumerable<Uri> ICalUrls)
		{
			sbErrors ??= new StringBuilder();

			var urls = ICalUrls.ToArray();
			var items = new List<Event>();

			if (!(urls?.Any()).GetValueOrDefault())
				return items;

			CalendarCollection calendars;
			using (MiniProfiler.Current.Step($"Gather {urls.Length:n0} calendars"))
				calendars = await urls.GetCalendars(sbErrors);

			var date = DateTime.Now.Date;

			const int maxEvents = 60;

			using (MiniProfiler.Current.Step($"Gathering at most {maxEvents} events within 2 years"))
			{
				List<Occurrence> occurrences;
				using (MiniProfiler.Current.Step($"Gathering occurrences between {date:d} and "))
					occurrences = calendars.SelectMany(x =>
							x.GetOccurrences(date, DateTime.Now.AddYears(2))).ToList();

				using (MiniProfiler.Current.Step($"Converting {Math.Min(occurrences.Count, maxEvents):n0} events"))
					items.AddRange(occurrences
								.OrderBy(x => x.Period.StartTime.AsDateTimeOffset)
								.ToArray()
								.SelectMany(x =>
								{
									//For multi-day periods, list each day within the period separately

									if (!(x.Source is CalendarEvent calendarEvent))
										return null;

									var thisDate = date;
									var result = new List<Event>();

									while (result.Count < maxEvents
									&& thisDate < x.Period.EndTime.AsSystemLocal)
									{

										var hasOverlap = thisDate < x.Period.EndTime.AsSystemLocal
										&& thisDate >= x.Period.StartTime.AsSystemLocal.Date;

										if (!hasOverlap)
										{
											thisDate = thisDate.AddDays(1);
											continue;
										}

										var isAllDay = calendarEvent.IsAllDay
										|| (x.Period.StartTime.AsSystemLocal <= thisDate
										 && x.Period.EndTime.AsSystemLocal >= thisDate.AddDays(1)
										 );

										var start = isAllDay
														? (TimeSpan?)null
														: x.Period.StartTime.AsSystemLocal < thisDate
															? TimeSpan.FromHours(0)
															: (x.Period.StartTime.IsUtc
																? x.Period.StartTime.AsDateTimeOffset.LocalDateTime //Convert UTC to local, todo: make timezone of panel configurable?
																: x.Period.StartTime.AsDateTimeOffset               //When timezone has been specified show as local time, do not touch
																)
																.TimeOfDay;
										var end = isAllDay
														? (TimeSpan?)null
														: x.Period.EndTime.AsSystemLocal >= thisDate.AddDays(1)
															? TimeSpan.FromHours(24)
															: (x.Period.EndTime.IsUtc
																? x.Period.EndTime.AsDateTimeOffset.LocalDateTime //Convert UTC to local, todo: make timezone of panel configurable?
																: x.Period.EndTime.AsDateTimeOffset               //When timezone has been specified show as local time, do not touch
																).TimeOfDay;


										result.Add(new Event()
										{
											Date = thisDate,
											//This needs clearer thought:
											//The calender is set to a time zone
											//Events can be in a specific timezone and can have a location
											//Display of events often have the perspective from a specific time zone.
											Start = start,
											End = end,
											Summary = calendarEvent?.Summary,
											CalendarName = (string)calendarEvent?.Properties["X-WR-CALNAME"]?.Value
										});

										thisDate = thisDate.AddDays(1);
									}

									return result;
								})
								.OrderBy(x => x.Date)
								.ThenBy(x => x.Start)
								.ThenBy(x => x.End)
								.Take(maxEvents)
								.ToArray());
			}


			if (!(items?.Any()).GetValueOrDefault())
				sbErrors.AppendLine($"No events in {urls?.Length:n0} calendars");

			return items.Distinct().ToList();
		}

		/// <summary>
		/// Get 
		/// </summary>
		/// <param name="ICalUrls"></param>
		/// <param name="errors"></param>
		/// <returns></returns>
		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Contains catch-all-and-log logic")]
		public static async Task<CalendarCollection> GetCalendars(this IEnumerable<Uri> ICalUrls, StringBuilder errors)
		{
			var sw = Stopwatch.StartNew();

			var calendars = new CalendarCollection();

			var tasks = ICalUrls.AsParallel().Select((iCalUrl, index) =>
				Task.Run(async () =>
				{
					try
					{
						calendars.Add(await LoadCachedCalendar(iCalUrl));
					}
					catch (HttpRequestException ex)
					{
						var messageDisplayed = $"Failed to obtain calender {index + 1} data:\n{ex.Message}";
						errors?.AppendLine(messageDisplayed);
						ex.Data.Add("Message displayed", messageDisplayed);
						ex.Log(severity: Bugsnag.Severity.Warning);
					}
					catch (SerializationException ex)
					{
						var messageDisplayed = $"Failed to parse calender {index + 1} data:\n{ex.Message}";
						errors?.AppendLine(messageDisplayed);
						ex.Data.Add("Message displayed", messageDisplayed);
						ex.Log(severity: Bugsnag.Severity.Warning);
					}
					catch (Exception ex)
					{
						var messageDisplayed = $"Failed to obtain or parse calender {index + 1} data:\n{ex.Message}";
						errors?.AppendLine(messageDisplayed);
						ex.Data.Add("Message displayed", messageDisplayed);
						ex.Log();
					}
				})
			);

			await Task.WhenAll(tasks);

			PerformanceMonitor.Trace($"Obtained calendars in {sw.Elapsed}");
			PerformanceMonitor.Trace(errors?.ToString());

			return calendars;
		}

		private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions()
		{
			SizeLimit = 1024,
		});

		/// <summary>
		/// Returns a cached calender, or loads it using <see cref="LoadCalendarContent"/> and caches it for one minute.
		/// </summary>
		/// <param name="iCalUrl"></param>
		/// <returns></returns>
		private static async Task<Ical.Net.Calendar> LoadCachedCalendar(Uri iCalUrl)
		{

			string content;
			//Cache http response, not the calendar
			using (MiniProfiler.Current.Step($"Getting calendar content from cache"))
				if (!_cache.TryGetValue(iCalUrl.ToString(), out content))// Look for cache key.
				{
					var cacheEntryOptions = new MemoryCacheEntryOptions()
						.SetSize(1)
						// Remove from cache after this time, regardless of sliding expiration
						.SetAbsoluteExpiration(TimeSpan.FromMinutes(1));

					using (MiniProfiler.Current.Step($"Calendar not in cache, getting from url"))
						// Key not in cache, so get data.
						content = await LoadCalendarContent(iCalUrl);

					// Save data in cache.
					_cache.Set(iCalUrl.ToString(), content, cacheEntryOptions);
				}

			try
			{
				return LoadCalendar(content);
			}
			catch (Exception ex)
			{
				ex.Data.Add("RawCalendarContent", content);
				ex.Log(severity: Bugsnag.Severity.Warning);
				throw;
			}
		}

		internal static Ical.Net.Calendar LoadCalendar(string content) => Ical.Net.Calendar.Load(content);

		private static async Task<string> LoadCalendarContent(Uri iCalUrl)
		{
			var request = await client.GetAsync(iCalUrl.ToString());

			request.EnsureSuccessStatusCode();

			return await request.Content.ReadAsStringAsync();
		}
	}
	/// <summary>
	/// 
	/// </summary>
	public static class DateTimeHelper	
	{

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="timeZone"></param>
		/// <returns></returns>
		public static DateTime? ToSpecificTimeZone(this DateTime? source, TimeZoneInfo timeZone)
		{
			if (!source.HasValue)
				return null;

			return source.Value.ToSpecificTimeZone(timeZone);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="timeZone"></param>
		/// <returns></returns>
		public static DateTime ToSpecificTimeZone(this DateTime source, TimeZoneInfo timeZone)
		{

			var offset = timeZone.GetUtcOffset(source);
			var newDt = source.Add(offset);
			return newDt;
		}
	}
}

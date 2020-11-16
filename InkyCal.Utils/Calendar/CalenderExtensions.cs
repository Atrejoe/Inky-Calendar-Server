using Ical.Net;
using Ical.Net.CalendarComponents;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace InkyCal.Utils.Calendar
{
	/// <summary>
	/// A helper class for obtaining calender info
	/// </summary>
	public static class CalenderExtensions
	{
		private static readonly HttpClient client = new HttpClient();

		internal static async Task<List<Event>> GetEvents(StringBuilder sbErrors, IEnumerable<Uri> ICalUrls)
		{
			sbErrors ??= new StringBuilder();

			var urls = ICalUrls.ToArray();
			var items = new List<Event>();

			if (!(urls?.Any()).GetValueOrDefault())
			{
				sbErrors.AppendLine($"No calenders loaded");
				return items;
			}

			var calendars = await urls.GetCalendars(sbErrors);

			var date = DateTime.Now.Date;

			while (items.Count < 60 && date < DateTime.Now.AddYears(2))
			{
				items.AddRange(calendars
								.GetOccurrences(date)
								.Select(x => x.Source)
								.Cast<CalendarEvent>()
								.Take(1000)
								.ToArray()
								.Select(x =>
								new Event()
								{
									Date = date,
									//This needs clearer thought:
									//The calender is set to a time zone
									//Events can be in a specific timezone and can have a location
									//Display of events often have the perspective from a specific time zone.
									Start = x.IsAllDay
											? null
											: (TimeSpan?)(x.Start.IsUtc
												? x.Start.AsDateTimeOffset.LocalDateTime //Convert UTC to local, todo: make timezone of panel configurable?
												: x.Start.AsDateTimeOffset               //When timezone has been specified show as local time, do not touch
												)
												.TimeOfDay,
									End = x.IsAllDay ? null : (TimeSpan?)(x.End.IsUtc
												? x.End.AsDateTimeOffset.LocalDateTime //Convert UTC to local, todo: make timezone of panel configurable?
												: x.End.AsDateTimeOffset               //When timezone has been specified show as local time, do not touch
												).TimeOfDay,
									Summary = x.Summary,
									CalendarName = (string)x.Calendar.Properties["X-WR-CALNAME"]?.Value
								})
								.ToArray());

				date = date.AddDays(1);
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

			var tasks = ICalUrls.Select((iCalUrl, index) =>
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

			//Cache http response, not the calendar
			if (!_cache.TryGetValue(iCalUrl.ToString(), out string content))// Look for cache key.
			{
				var cacheEntryOptions = new MemoryCacheEntryOptions()
					.SetSize(1)
					// Remove from cache after this time, regardless of sliding expiration
					.SetAbsoluteExpiration(TimeSpan.FromMinutes(1));

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

}

using Ical.Net;
using Ical.Net.CalendarComponents;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace InkyCal.Utils
{
	/// <summary>
	/// A helper class for obtaining calender info
	/// </summary>
	public static class CalenderExtensions
	{
		private static readonly HttpClient client = new HttpClient();

		internal static async Task<List<Event>> GetEvents(StringBuilder sbErrors, Uri[] ICalUrls, int lineLength)
		{

			var items = new List<Event>();

			if (!(ICalUrls?.Any()).GetValueOrDefault())
			{
				sbErrors.AppendLine($"No calenders loaded");
				return items;
			}

			var calendars = await ICalUrls.GetCalendars(lineLength * 2, sbErrors);

			var date = DateTime.Now.Date;

			while (items.Count() < 60 && date < DateTime.Now.AddYears(1))
			{
				items.AddRange(calendars
										.GetOccurrences(date, date.AddDays(1))
										.Select(x => x.Source)
										.Cast<CalendarEvent>()
					.Take(1000)
					.Select(x =>
					new Event()
					{
						Date = date,
						Start = x.IsAllDay ? null : (TimeSpan?)x.Start.AsDateTimeOffset.TimeOfDay,
						End = x.IsAllDay ? null : (TimeSpan?)x.End.AsDateTimeOffset.TimeOfDay,
						Summary = x.Summary,
						CalendarName = (string)x.Calendar.Properties["X-WR-CALNAME"]?.Value
					}));

				date = date.AddDays(1);
			}

			if (!(items?.Any()).GetValueOrDefault())
				sbErrors.AppendLine($"No events in {ICalUrls?.Count():n0} calendars");

			return items;
		}

		/// <summary>
		/// Get 
		/// </summary>
		/// <param name="ICalUrls"></param>
		/// <param name="errorDetailsLength"></param>
		/// <param name="errors"></param>
		/// <returns></returns>
		public static async Task<CalendarCollection> GetCalendars(this Uri[] ICalUrls, int errorDetailsLength, StringBuilder errors)
		{
			var sw = Stopwatch.StartNew();

			var calendars = new CalendarCollection();

			var tasks = ICalUrls.Select(iCalUrl =>
				Task.Run(async () =>
				{
					try
					{
						calendars.Add(await LoadCachedCalendar(iCalUrl));
					}
					catch (HttpRequestException ex)
					{
						errors?.AppendLine($"Failed to obtain calender data:\n{ex.Message.Limit(errorDetailsLength, " ...")}");
					}
					catch (SerializationException ex)
					{
						errors?.AppendLine($"Failed to parse calender data:\n{ex.Message.Limit(errorDetailsLength, " ...")}");
					}
					catch (Exception ex)
					{
						errors?.AppendLine($"Failed to obtain or parse calender data:\n{ex.Message.Limit(errorDetailsLength, " ...")}");
					}
				})
			);

			await Task.WhenAll(tasks);

			Trace.WriteLine($"Obtained calendars in {sw.Elapsed}");
			Trace.WriteLine(errors.ToString());

			return calendars;
		}

		private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions()
		{
			SizeLimit = 1024,
		});

		/// <summary>
		/// Returns a cached calender, or loads it using <see cref="LoadCalendar"/> and caches it for one minute.
		/// </summary>
		/// <param name="iCalUrl"></param>
		/// <returns></returns>
		private static async Task<Calendar> LoadCachedCalendar(Uri iCalUrl)
		{

			if (!_cache.TryGetValue(iCalUrl.ToString(), out Calendar cacheEntry))// Look for cache key.
			{
				// Key not in cache, so get data.
				cacheEntry = await LoadCalendar(iCalUrl);

				var cacheEntryOptions = new MemoryCacheEntryOptions()
					.SetSize(1)
					// Remove from cache after this time, regardless of sliding expiration
					.SetAbsoluteExpiration(TimeSpan.FromMinutes(1));

				// Save data in cache.
				_cache.Set(iCalUrl.ToString(), cacheEntry, cacheEntryOptions);
			}

			return cacheEntry;
		}

		private static async Task<Calendar> LoadCalendar(Uri iCalUrl)
		{
			return Calendar.Load(await client.GetStreamAsync(iCalUrl.ToString()));
		}
	}
}

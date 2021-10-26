using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Services;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using InkyCal.Models;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Profiling;

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

		internal static async Task<IEnumerable<Event>> GetEvents(StringBuilder sbErrors, GoogleOAuthAccess[] tokens, SubscribedGoogleCalender[] calendars)
		{
			var result = new List<Event>();

			if (tokens != null)
			{
				await foreach (var token in GetAccessTokens(tokens))
					if (token != default)
						foreach (var calenderId in calendars.Where(x => x.AccessToken == token.Id).Select(x => x.Calender))
							result.AddRange(await GetEvents(sbErrors, token.AccessToken, calenderId));
			}

			return result;
		}

		/// <summary>
		/// Converts refresh tokens into usable access tokens
		/// </summary>
		/// <param name="tokens"></param>
		/// <returns></returns>
		public static async IAsyncEnumerable<(int Id, string AccessToken)> GetAccessTokens(IEnumerable<GoogleOAuthAccess> tokens)
		{
			foreach (var token in tokens)
			{

				if (token.AccessTokenExpiry <= DateTime.UtcNow.AddSeconds(-100))
				{
					var refreshed = await GoogleOAuth.RefreshAccessToken(token.RefreshToken);

					if (refreshed == default)
					{
						yield return default;
						continue;
					}

					token.AccessToken = refreshed.AccessToken;
					token.AccessTokenExpiry = refreshed.AccessTokenExpiry.GetValueOrDefault(DateTime.UtcNow);

					//todo: store updated access token & expiry
					//todo: handle revoked access
				}
				var result = (token.Id, token.AccessToken);

				yield return result;
			}
		}

		/// <summary>
		/// Converts refresh tokens into usable access tokens
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public static async Task<(int Id, string AccessToken)> GetAccessToken(this GoogleOAuthAccess token) {
			if (token.AccessTokenExpiry <= DateTime.UtcNow.AddSeconds(-100))
			{
				var refreshed = await GoogleOAuth.RefreshAccessToken(token.RefreshToken);

				if (refreshed == default)
					return default;

				token.AccessToken = refreshed.AccessToken;
				token.AccessTokenExpiry = refreshed.AccessTokenExpiry.GetValueOrDefault(DateTime.UtcNow);

				//todo: store updated access token & expiry
				//todo: handle revoked access
			}
			return (token.Id, token.AccessToken); ;
		}

		/// <summary>
		/// The Google Calendar API service.
		/// </summary>
		private static readonly Lazy<CalendarService> CalendarService = new Lazy<CalendarService>(() => new CalendarService(new BaseClientService.Initializer()
		{
			ApplicationName = "Inky Calender server"
		}));


		/// <summary>
		/// 
		/// </summary>>
		/// <returns></returns>
		public static async IAsyncEnumerable<(int IdToken, Userinfo profile, CalendarListEntry Calender)> ListGoogleCalendars(IEnumerable<GoogleOAuthAccess> tokens)
		{
			await foreach (var token in GetAccessTokens(tokens))
				if (token != default)
				{
					var profile = await GoogleOAuth.GetProfile(token.AccessToken);

					await foreach (var calender in ListGoogleCalendars(token.AccessToken))
						yield return (token.Id, profile, calender);
				}

		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="accessToken"></param>
		/// <returns></returns>
		public static async IAsyncEnumerable<CalendarListEntry> ListGoogleCalendars(string accessToken)
		{
			var request = CalendarService.Value.CalendarList.List();
			request.OauthToken = accessToken;
			request.ShowHidden = true;
			request.ShowDeleted = false;

			// List calendars.
			var calendars = await request.ExecuteAsync();
			Console.WriteLine("Calendars:");
			if (calendars.Items != null && calendars.Items.Count > 0)
				foreach (var calendar in calendars.Items)
					yield return calendar;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="accessToken"></param>
		/// <param name="calenderId"></param>
		/// <returns></returns>
		public static async Task<CalendarListEntry> GetGoogleCalendar(string accessToken, string calenderId)
		{
			var request = CalendarService.Value.CalendarList.Get(calenderId);
			request.OauthToken = accessToken;

			// List calendars.
			return await request.ExecuteAsync();
		}

		private static async Task<IEnumerable<Event>> GetEvents(StringBuilder sbErrors, string accessToken, string calendarId)
		{
			var result = new List<Event>();

			try
			{
				var calendar = await GetGoogleCalendar(accessToken, calendarId);
				{
					switch (calendar.AccessRole)
					{
						case "freeBusyReader":
						case "reader":
							{
								//Console.WriteLine($"Skipping non-owned calender: {calendar.Id} {calendar.Summary} ({calendar.Description})");
								//return result;
							}
							break;
						default:
							break;
					}
					Console.WriteLine($"{calendar.Id} {calendar.Summary} ({calendar.Description})");
					var itemRequest = CalendarService.Value.Events.List(calendar.Id);

					itemRequest.OauthToken = accessToken;
					itemRequest.TimeMin = DateTime.UtcNow.Date;
					itemRequest.TimeMax = DateTime.UtcNow.AddDays(31);

					// List events.
					var events = await itemRequest.ExecuteAsync();
					foreach (var item in events.Items
						.Where(x => x.Status != "cancelled"
						&& x.Start != null
						//&& x.Start.DateTime.HasValue
						)
						.Take(50))
					{
						if (item.Recurrence == null)
						{
							ConvertEvent(result, item, calendar);
						}
						else
						{
							var instancesRequest = CalendarService.Value.Events.Instances(calendar.Id, item.Id);
							instancesRequest.OauthToken = accessToken;
							instancesRequest.TimeMin = itemRequest.TimeMin;
							instancesRequest.TimeMax = itemRequest.TimeMax;

							var instances = await instancesRequest.ExecuteAsync();

							foreach (var instance in instances.Items
						.Where(x => x.Status != "cancelled"
						&& x.Start != null
						)
						.Take(50))
							{
								ConvertEvent(result, instance, calendar);
							}
						}

					}
				}
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex.ToString());
				sbErrors.AppendLine(ex.ToString());
				throw;
			}

			return result;
		}

		private static void ConvertEvent(List<Event> result, Google.Apis.Calendar.v3.Data.Event item, CalendarListEntry calendar)
		{
			DateTime date;
			if (item.Start.DateTime.HasValue)
			{
				date = item.Start.DateTime.Value.Date;
			}
			else if (!DateTime.TryParse(item.Start.Date, out date))
				return;

			result.Add(new Event()
			{
				CalendarName = calendar.Summary,
				Date = date,
				Start = item.Start?.DateTime?.TimeOfDay,
				End = item.End?.DateTime?.TimeOfDay,
				Summary = item.Summary
			});

			Console.WriteLine($"	{item.Id} [{item.Start?.DateTime} - {item.End?.DateTime}]{item.Summary}");
		}
	}

}

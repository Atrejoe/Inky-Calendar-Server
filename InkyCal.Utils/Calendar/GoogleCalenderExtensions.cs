using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Services;
using InkyCal.Models;
using StackExchange.Profiling;

namespace InkyCal.Utils.Calendar
{
	/// <summary>
	/// 
	/// </summary>
	public static class GoogleCalenderExtensions
	{
		internal static async Task<IEnumerable<Event>> GetEvents(StringBuilder sbErrors, SubscribedGoogleCalender[] calendars, Func<GoogleOAuthAccess, Task> saveToken)
		{
			var result = new List<Event>();

			var tasks = new List<Task>();

			await foreach (var token in GetAccessTokens(calendars.Select(x => x.AccessToken).Distinct(), saveToken))
				if (token != default)
				{
					tasks.AddRange(
						calendars.Where(x => x.IdAccessToken == token.Id).Select(x => x.Calender).Select(
						async calenderId =>
							result.AddRange(await GetEvents(sbErrors, token.AccessToken, calenderId))
						));
				}

			await Task.WhenAll(tasks);

			return result;
		}

		/// <summary>
		/// Checks if <see cref="GoogleOAuthAccess.AccessToken"/> needs to be refreshed.
		/// If so, does so using <see cref="GoogleOAuthAccess.RefreshToken"/>. When this fails, <c>default</c> is returned.
		/// </summary>
		/// <param name="tokens"></param>
		/// <param name="saveToken"></param>
		/// <returns></returns>
		public static async IAsyncEnumerable<(int Id, string AccessToken)> GetAccessTokens(IEnumerable<GoogleOAuthAccess> tokens, Func<GoogleOAuthAccess, Task> saveToken)
		{
			if (saveToken is null)
				throw new ArgumentNullException(nameof(saveToken));

			foreach (var token in tokens)
			{

				if (token.AccessTokenExpiry <= DateTime.UtcNow.AddSeconds(-100))
				{
					(string AccessToken, DateTimeOffset? AccessTokenExpiry) refreshed;

					using (MiniProfiler.Current.Step($"Refreshing access token"))
						refreshed = await GoogleOAuth.RefreshAccessToken(token.RefreshToken);

					if (refreshed == default)
					{
						yield return default;
						continue;
					}

					token.AccessToken = refreshed.AccessToken;
					token.AccessTokenExpiry = refreshed.AccessTokenExpiry.GetValueOrDefault(DateTime.UtcNow);

					using (MiniProfiler.Current.Step($"Saving updated access token"))
						await saveToken(token);
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
		public static async Task<(int Id, string AccessToken, bool Refreshed)> GetAccessToken(this GoogleOAuthAccess token)
		{
			var refreshed = false;
			if (token.AccessTokenExpiry <= DateTime.UtcNow.AddSeconds(-100))
			{
				var refreshedToken = await GoogleOAuth.RefreshAccessToken(token.RefreshToken);

				if (refreshedToken == default)
					return default;

				token.AccessToken = refreshedToken.AccessToken;
				token.AccessTokenExpiry = refreshedToken.AccessTokenExpiry.GetValueOrDefault(DateTime.UtcNow);
				refreshed = true;

				//todo: store updated access token & expiry
				//todo: handle revoked access
			}
			return (token.Id, token.AccessToken, refreshed); ;
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
		public static async IAsyncEnumerable<(int IdToken, Userinfo profile, CalendarListEntry Calender)> ListGoogleCalendars(IEnumerable<GoogleOAuthAccess> tokens, Func<GoogleOAuthAccess, Task> saveToken)
		{
			if (saveToken is null)
				throw new ArgumentNullException(nameof(saveToken));

			await foreach (var token in GetAccessTokens(tokens, saveToken))
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

			// List calendars
			try
			{
				return await request.ExecuteAsync();
			}
			catch (GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
			{
				return null;
			}
		}

		private static async Task<IEnumerable<Event>> GetEvents(StringBuilder sbErrors, string accessToken, string calendarId)
		{
			var result = new List<Event>();
			using (MiniProfiler.Current.Step($"Gather events for calendar {calendarId}"))
				try
				{
					CalendarListEntry calendar;
					using (MiniProfiler.Current.Step($"Gathering calendar details"))
						calendar = await GetGoogleCalendar(accessToken, calendarId);

					if (calendar == null)
						return result;

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
					//Console.WriteLine($"{calendar.Id} {calendar.Summary} ({calendar.Description})");
					var itemRequest = CalendarService.Value.Events.List(calendar.Id);

					itemRequest.OauthToken = accessToken;
					itemRequest.TimeMin = DateTime.UtcNow.Date;
					itemRequest.TimeMax = DateTime.UtcNow.AddDays(31);

					// List events.
					Events events;
					using (MiniProfiler.Current.Step($"Gathering events"))
						events = await itemRequest.ExecuteAsync();

					var tasks = (events.Items
						.Where(x => x.Status != "cancelled"
						&& x.Start != null
						//&& x.Start.DateTime.HasValue
						)
						.Take(50))
						.Select(async item =>
							{
								if (item.Recurrence == null)

									ConvertEvent(result, item, calendar);

								else
								{

									var instancesRequest = CalendarService.Value.Events.Instances(calendar.Id, item.Id);
									instancesRequest.OauthToken = accessToken;
									instancesRequest.TimeMin = itemRequest.TimeMin;
									instancesRequest.TimeMax = itemRequest.TimeMax;

									Events instances;
									using (MiniProfiler.Current.Step($"Gathering instances of recurring event '{item.Summary}'"))
										instances = await instancesRequest.ExecuteAsync();

									foreach (var instance in instances.Items
												.Where(x => x.Status != "cancelled"
														 && x.Start != null
													)
												.Take(50))
									{
										ConvertEvent(result, instance, calendar);
									}
								}
							});

					await Task.WhenAll(tasks);
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
			DateTime start;
			if (item.Start.DateTime.HasValue)
			{
				start = item.Start.DateTime.Value.Date;
			}
			else if (!DateTime.TryParse(item.Start.Date, out start))
				return;


			DateTime end;
			if (item.End.DateTime.HasValue)
			{
				end = item.End.DateTime.Value.Date;
			}
			else if (!DateTime.TryParse(item.End.Date, out end))
				return;

			//date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

			//TimeSpan? start = null;
			//if (item.Start != null) {
			//	start = DateTime.SpecifyKind(item.Start.TimeZone, DateTimeKind.Utc)
			//}

			var multiDay = start.AddDays(1) < end;

			if (multiDay)
			{
				for(var i = 0; i <= 31; i++) {
					if (DateTime.UtcNow.Date > start.AddDays(i))
						continue;

					if (start.AddDays(i) > end)
						break;

					var firstDay = i == 0;
					var lastDay = start.AddDays(i).Date.Equals(end.Date);

					var startTime = firstDay
										? item.Start?.DateTime.ToSpecificTimeZone(DutchTZ)?.TimeOfDay
										: null;

					var endTime = lastDay
										? item.End?.DateTime.ToSpecificTimeZone(DutchTZ)?.TimeOfDay
										: null;

					result.Add(new Event()
					{
						CalendarName = calendar.Summary,
						Date = start.AddDays(i).ToSpecificTimeZone(DutchTZ),
						Start = startTime,
						End = endTime,
						Summary = firstDay
										? $"{item.Summary} >"
										: lastDay
											? $"< {item.Summary}"
											: $"< {item.Summary} >"
					});
				}
			}
			else
				result.Add(new Event()
				{
					CalendarName = calendar.Summary,
					Date = start.ToSpecificTimeZone(DutchTZ),
					Start = item.Start?.DateTime.ToSpecificTimeZone(DutchTZ)?.TimeOfDay,
					End = item.End?.DateTime.ToSpecificTimeZone(DutchTZ)?.TimeOfDay,
					Summary = item.Summary
				});

			Console.WriteLine($"	{item.Id} [{item.Start?.DateTime} - {item.End?.DateTime}]{item.Summary}");
		}

		/// <summary>
		/// The time zone determined to be dutch
		/// </summary>
		public static readonly TimeZoneInfo DutchTZ = GetDutchTZ();

		private static TimeZoneInfo GetDutchTZ()
		{
			TimeZoneInfo result;
			try
			{
				result = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			}
			catch (Exception ex)
			{
				ex.Log();

				var timezones = TimeZoneInfo.GetSystemTimeZones();
				result = timezones.FirstOrDefault(tz => tz.Id.Equals("Europe/Amsterdam"))
					?? timezones.FirstOrDefault(tz => tz.BaseUtcOffset == TimeSpan.FromHours(-1))
					?? TimeZoneInfo.GetSystemTimeZones().FirstOrDefault();
			}

			return result;
		}
	}
}

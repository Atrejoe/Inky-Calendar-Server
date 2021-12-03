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

namespace InkyCal.Utils.Calendar
{
	/// <summary>
	/// 
	/// </summary>
	public static class GoogleCalenderExtensions
	{
		internal static async Task<IEnumerable<Event>> GetEvents(StringBuilder sbErrors, SubscribedGoogleCalender[] calendars)
		{
			var result = new List<Event>();

			await foreach (var token in GetAccessTokens(calendars.Select(x => x.AccessToken).Distinct()))
				if (token != default)
					foreach (var calenderId in calendars.Where(x => x.IdAccessToken == token.Id).Select(x => x.Calender))
						result.AddRange(await GetEvents(sbErrors, token.AccessToken, calenderId));


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

			try
			{
				var calendar = await GetGoogleCalendar(accessToken, calendarId);

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

			//date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

			//TimeSpan? start = null;
			//if (item.Start != null) {
			//	start = DateTime.SpecifyKind(item.Start.TimeZone, DateTimeKind.Utc)
			//}

			result.Add(new Event()
			{
				CalendarName = calendar.Summary,
				Date = date.ToSpecificTimeZone(DutchTZ),
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

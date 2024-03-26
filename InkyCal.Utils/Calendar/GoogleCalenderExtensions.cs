// Ignore Spelling: Utils

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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
		internal static async Task<IEnumerable<Event>> GetEvents(StringBuilder sbErrors, SubscribedGoogleCalender[] calendars, Func<GoogleOAuthAccess, Task> saveToken, CancellationToken cancellationToken)
		{
			var result = new List<Event>();

			var tokens = GetAccessTokens(calendars.Select(x => x.AccessToken).Distinct(), saveToken);

			await foreach (var token in tokens)
				if (token != default)
				{
					//foreach (var calenderId in calendars.Where(x => x.IdAccessToken == token.Id).Select(x => x.Calender))
					//{
					//	var events = await GetEvents(sbErrors, token.AccessToken, calenderId);
					//	result.AddRange(events);
					//}

					// When using parallel foreach stuff broke, method appeared to exit beforre results were complete ... or so

					await Parallel.ForEachAsync(
						calendars
						.Where(x => x.IdAccessToken == token.Id)
						.Select(x => x.Calender),
						cancellationToken,
						async (calenderId, cancellationToken) =>
						{
							var events = await GetEvents(sbErrors, token.AccessToken, calenderId, cancellationToken);
							result.AddRange(events);
						});
				}
				else
					sbErrors.Append("No access token");

			return result;
		}

		/// <summary>
		/// Checks if <see cref="GoogleOAuthAccess.AccessToken"/> needs to be refreshed.
		/// If so, does so using <see cref="GoogleOAuthAccess.RefreshToken"/>. When this fails, <c>default</c> is returned.
		/// </summary>
		/// <param name="tokens"></param>
		/// <param name="saveToken"></param>
		/// <returns></returns>
		public static async IAsyncEnumerable<(int Id, string AccessToken)> GetAccessTokens([NotNull] IEnumerable<GoogleOAuthAccess> tokens, Func<GoogleOAuthAccess, Task> saveToken)
		{
			Validate(tokens, saveToken);

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

		private static void Validate([NotNull] IEnumerable<GoogleOAuthAccess> tokens, [NotNull] Func<GoogleOAuthAccess, Task> saveToken)
		{
			if (tokens is null)
				throw new ArgumentNullException(nameof(tokens));

			if (saveToken is null)
				throw new ArgumentNullException(nameof(saveToken));
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
			return (token.Id, token.AccessToken, refreshed);
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
		public static async IAsyncEnumerable<(int IdToken, Userinfo profile, CalendarListEntry Calender)> ListGoogleCalendars([NotNull]IEnumerable<GoogleOAuthAccess> tokens, [NotNull] Func<GoogleOAuthAccess, Task> saveToken)
		{
			if (!Server.Config.GoogleOAuth.Enabled)
				yield break;

			Validate(tokens, saveToken);

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
			if (calendars.Items != null && calendars.Items.Count > 0)
				foreach (var calendar in calendars.Items)
					yield return calendar;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="accessToken"></param>
		/// <param name="calenderId"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task<CalendarListEntry> GetGoogleCalendar(string accessToken, string calenderId, CancellationToken cancellationToken = default)
		{
			var request = CalendarService.Value.CalendarList.Get(calenderId);
			request.OauthToken = accessToken;

			// List calendars
			try
			{
				return await request.ExecuteAsync(cancellationToken);
			}
			catch (GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
			{
				return null;
			}
		}

		private static async Task<IEnumerable<Event>> GetEvents(StringBuilder sbErrors, string accessToken, string calendarId, CancellationToken cancellationToken)
		{
			var result = new List<Event>();
			using (MiniProfiler.Current.Step($"Gather events for calendar {calendarId}"))
				try
				{
					CalendarListEntry calendar;
					using (MiniProfiler.Current.Step($"Gathering calendar details"))
						calendar = await GetGoogleCalendar(accessToken, calendarId, cancellationToken);

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

					var itemRequest = CalendarService.Value.Events.List(calendar.Id);

					itemRequest.OauthToken = accessToken;
					itemRequest.TimeMinDateTimeOffset = DateTime.UtcNow.Date;
					itemRequest.TimeMaxDateTimeOffset = DateTime.UtcNow.AddDays(7);
					itemRequest.ETagAction = Google.Apis.ETagAction.IfNoneMatch;

					itemRequest.SingleEvents = true;


					// List events.
					Events events;
					using (MiniProfiler.Current.Step($"Gathering events"))
						events = await itemRequest.ExecuteAsync();

					foreach (var item in events.Items
						.Where(x => x.Status != "cancelled"
						&& x.Start != null
						//&& x.Start.DateTime.HasValue
						)
						.Take(50))
					{
						if (item.Recurrence == null)

							ConvertEvent(result, item, calendar);

						else
						{

							var instancesRequest = CalendarService.Value.Events.Instances(calendar.Id, item.Id);
							instancesRequest.OauthToken = accessToken;
							instancesRequest.TimeMinDateTimeOffset = itemRequest.TimeMinDateTimeOffset;
							instancesRequest.TimeMaxDateTimeOffset = itemRequest.TimeMaxDateTimeOffset;

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
			if (item.Start.DateTimeDateTimeOffset.HasValue)
			{
				date = item.Start.DateTimeDateTimeOffset.Value.Date;
			}
			else if (!DateTime.TryParseExact(item.Start.Date, "yyyy-MM-dd",
					   CultureInfo.InvariantCulture,
					   DateTimeStyles.None, 
					   out date))
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
				Start = item.Start?.DateTimeDateTimeOffset.ToSpecificTimeZone(DutchTZ)?.TimeOfDay,
				End = item.End?.DateTimeDateTimeOffset.ToSpecificTimeZone(DutchTZ)?.TimeOfDay,
				Summary = item.Summary
			});

			Trace.TraceInformation($"\t{item.Id} [{item.Start?.DateTimeDateTimeOffset} - {item.End?.DateTimeDateTimeOffset}]{item.Summary}");
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

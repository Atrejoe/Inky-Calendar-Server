using System;
using System.Linq;
using System.Text;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using InkyCal.Utils.Calendar;
using Xunit;

namespace InkyCal.Utils.Tests.Calender
{
	/// <summary>
	/// Tests <see cref="ICalExtensions"/>
	/// </summary>
	public class CalenderExtensionsTest
	{
		//Demo calender
		internal const string demoCalender = @"BEGIN:VCALENDAR
PRODID:-//Google Inc//Google Calendar 70.9054//EN
VERSION:2.0
CALSCALE:GREGORIAN
METHOD:PUBLISH
X-WR-CALNAME:InkyCal-test-calendar
X-WR-TIMEZONE:Europe/Berlin
X-WR-CALDESC:This is a test calendar for the Inky-Calendar project on Githu
 b (copyright by aceisace). It is meant for debugging purposes.
BEGIN:VTIMEZONE
TZID:Europe/Berlin
X-LIC-LOCATION:Europe/Berlin
BEGIN:DAYLIGHT
TZOFFSETFROM:+0100
TZOFFSETTO:+0200
TZNAME:CEST
DTSTART:19700329T020000
RRULE:FREQ=YEARLY;BYMONTH=3;BYDAY=-1SU
END:DAYLIGHT
BEGIN:STANDARD
TZOFFSETFROM:+0200
TZOFFSETTO:+0100
TZNAME:CET
DTSTART:19701025T030000
RRULE:FREQ=YEARLY;BYMONTH=10;BYDAY=-1SU
END:STANDARD
END:VTIMEZONE
BEGIN:VEVENT
DTSTART:20200527T150000Z
DTEND:20200527T200000Z
DTSTAMP:20201114T114528Z
UID:0rd0bnoklln3o344sogfft9fhc@google.com
CREATED:20200519T000009Z
DESCRIPTION:
LAST-MODIFIED:20200527T161913Z
LOCATION:
SEQUENCE:0
STATUS:CONFIRMED
SUMMARY:27 May 5 pm - 10pm
TRANSP:OPAQUE
BEGIN:VALARM
ACTION:NONE
TRIGGER;VALUE=DATE-TIME:19760401T005545Z
END:VALARM
END:VEVENT
BEGIN:VEVENT
DTSTART;VALUE=DATE:20200523
DTEND;VALUE=DATE:20200525
DTSTAMP:20201114T114528Z
UID:1smg6ij14cubiq72j2m9htqdaf@google.com
CREATED:20200519T000035Z
DESCRIPTION:
LAST-MODIFIED:20200519T000035Z
LOCATION:
SEQUENCE:0
STATUS:CONFIRMED
SUMMARY:23 May - 24 may
TRANSP:TRANSPARENT
END:VEVENT
BEGIN:VEVENT
DTSTART:20200521T160000Z
DTEND:20200521T170000Z
DTSTAMP:20201114T114528Z
UID:37s02v9ccq6kbnqas8jf8ths66@google.com
CREATED:20200518T235942Z
DESCRIPTION:
LAST-MODIFIED:20200518T235942Z
LOCATION:
SEQUENCE:0
STATUS:CONFIRMED
SUMMARY:21 May 6pm-7pm
TRANSP:OPAQUE
END:VEVENT
BEGIN:VEVENT
DTSTART;VALUE=DATE:20200413
DTEND;VALUE=DATE:20200418
DTSTAMP:20201114T114528Z
UID:068CCF73-F3B0-4F84-85E0-7D336982E5FD
CREATED:20200304T055213Z
DESCRIPTION:
LAST-MODIFIED:20200304T055213Z
LOCATION:
SEQUENCE:0
STATUS:CONFIRMED
SUMMARY:Multi-day-event 13apr-17apr
TRANSP:OPAQUE
BEGIN:VALARM
ACTION:NONE
TRIGGER;VALUE=DATE-TIME:19760401T005545Z
END:VALARM
END:VEVENT
BEGIN:VEVENT
DTSTART;TZID=Europe/Berlin:20200303T150000
DTEND;TZID=Europe/Berlin:20200303T160000
RRULE:FREQ=WEEKLY;UNTIL=20200703T215959Z
DTSTAMP:20201114T114528Z
UID:2C620ADD-0F4F-4B1F-9582-A2C42415101B
CREATED:20200303T143647Z
DESCRIPTION:This Event is a recurring event on Tuesdays at 3 pm (Europe/Ber
 lin) time and is shedules weekly until 3 July 2020
LAST-MODIFIED:20200303T143647Z
LOCATION:
SEQUENCE:0
STATUS:CONFIRMED
SUMMARY:Recurring event 1 hour
TRANSP:OPAQUE
BEGIN:VALARM
ACTION:NONE
TRIGGER;VALUE=DATE-TIME:19760401T005545Z
END:VALARM
END:VEVENT
END:VCALENDAR
";

		[Fact]
		public void TestCalender()
		{

			//arrange & act
			var actual = ICalExtensions
							.LoadCalendar(demoCalender);

			var calendars = new CalendarCollection
			{
				actual
			};

			//assert
			Assert.NotNull(calendars.GetOccurrences(new CalDateTime(DateTime.SpecifyKind(DateTime.Now.Date, DateTimeKind.Unspecified), null, false)));
		}

		/// <summary>
		/// A test calendar with known future-dated events for verifying occurrence retrieval.
		/// Events are placed far in the future to remain valid for testing indefinitely.
		/// </summary>
		internal const string futureEventCalendar = @"BEGIN:VCALENDAR
PRODID:-//Test//Test//EN
VERSION:2.0
CALSCALE:GREGORIAN
METHOD:PUBLISH
X-WR-CALNAME:Test-future-calendar
X-WR-TIMEZONE:Europe/Berlin

BEGIN:VTIMEZONE
TZID:Europe/Berlin
BEGIN:DAYLIGHT
TZOFFSETFROM:+0100
TZOFFSETTO:+0200
TZNAME:CEST
DTSTART:19700329T020000
RRULE:FREQ=YEARLY;BYMONTH=3;BYDAY=-1SU
END:DAYLIGHT
BEGIN:STANDARD
TZOFFSETFROM:+0200
TZOFFSETTO:+0100
TZNAME:CET
DTSTART:19701025T030000
RRULE:FREQ=YEARLY;BYMONTH=10;BYDAY=-1SU
END:STANDARD
END:VTIMEZONE

BEGIN:VEVENT
DTSTART:20500101T140000Z
DTEND:20500101T150000Z
DTSTAMP:20240101T000000Z
UID:utc-event-1@test
SUMMARY:UTC event 14:00-15:00
END:VEVENT
BEGIN:VEVENT
DTSTART;TZID=Europe/Berlin:20500102T150000
DTEND;TZID=Europe/Berlin:20500102T160000
DTSTAMP:20240101T000000Z
UID:berlin-event-1@test
SUMMARY:Berlin TZ event 15:00-16:00
END:VEVENT

BEGIN:VEVENT
DTSTART;VALUE=DATE:20500103
DTEND;VALUE=DATE:20500104
DTSTAMP:20240101T000000Z
UID:allday-event-1@test
SUMMARY:All-day event on Jan 3
END:VEVENT
BEGIN:VEVENT
DTSTART;VALUE=DATE:20500105
DTEND;VALUE=DATE:20500108
DTSTAMP:20240101T000000Z
UID:multiday-event-1@test
SUMMARY:Multi-day event 5-7 Jan
END:VEVENT

END:VCALENDAR
";

		[Fact]
		public void LoadCalendar_ParsesSuccessfully()
		{
			// Verify calendar loads without exceptions
			var calendar = ICalExtensions.LoadCalendar(futureEventCalendar);

			Assert.NotNull(calendar);
			Assert.Equal(4, calendar.Events.Count);
		}

		[Fact]
		public void GetOccurrences_InRange_ReturnsExpectedCount()
		{
			// Arrange
			var calendar = ICalExtensions.LoadCalendar(futureEventCalendar);
			var calendars = new CalendarCollection
			{
				calendar
			};

			var startDate = new DateTime(2050, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
			var endDate = new DateTime(2050, 1, 8, 0, 0, 0, DateTimeKind.Unspecified);

			// Act
			var occurrences = calendars.GetOccurrences(new CalDateTime(startDate, null, false)).TakeWhileBefore(new CalDateTime(endDate, null, false)).ToList();

			// Assert: 1 UTC event + 1 Berlin TZ event + 1 all-day + 1 multi-day (whole period) = 4
			Assert.Equal(4, occurrences.Count);
		}

		[Fact]
		public void GetOccurrences_UtcEvent_HasCorrectProperties()
		{
			// Arrange
			var calendar = ICalExtensions.LoadCalendar(futureEventCalendar);
			var calendars = new CalendarCollection
			{
				calendar
			};

			// Act
			var occurrences = calendars.GetOccurrences(new CalDateTime(new DateTime(2050, 1, 1, 0, 0, 0, DateTimeKind.Unspecified), null, false)).TakeWhileBefore(new CalDateTime(new DateTime(2050, 1, 2, 0, 0, 0, DateTimeKind.Unspecified), null, false)).ToList();

			// Assert: UTC event at 14:00-15:00 UTC
			Assert.Single(occurrences);
			var o = occurrences[0];
			var src = Assert.IsType<CalendarEvent>(o.Source);
			Assert.Equal("UTC event 14:00-15:00", src.Summary);
			Assert.True(o.Period.StartTime.IsUtc, "Start time should be UTC");
			Assert.Equal(new TimeSpan(14, 0, 0), o.Period.StartTime.Value.TimeOfDay);
		}

		[Fact]
		public void GetOccurrences_NamedTimezoneEvent_HasCorrectProperties()
		{
			// Arrange
			var calendar = ICalExtensions.LoadCalendar(futureEventCalendar);
			var calendars = new CalendarCollection
			{
				calendar
			};

			// Act
			var occurrences = calendars
								.GetOccurrences(new CalDateTime(new DateTime(2050, 1, 2, 0, 0, 0, DateTimeKind.Unspecified), null, false))
								.TakeWhileBefore(new CalDateTime(new DateTime(2050, 1, 3, 0, 0, 0, DateTimeKind.Unspecified), null, false))
								.ToList();

			// Assert: Berlin TZ event at 15:00-16:00 local time
			Assert.Single(occurrences);
			var o = occurrences[0];
			var src = Assert.IsType<CalendarEvent>(o.Source);
			Assert.Equal("Berlin TZ event 15:00-16:00", src.Summary);
			Assert.False(o.Period.StartTime.IsUtc, "Start time should not be UTC");
			// Value holds the local time in the named timezone
			Assert.Equal(new TimeSpan(15, 0, 0), o.Period.StartTime.Value.TimeOfDay);
		}

		[Fact]
		public void GetOccurrences_AllDayEvent_HasNoTime()
		{
			// Arrange
			var calendar = ICalExtensions.LoadCalendar(futureEventCalendar);
			var calendars = new CalendarCollection
			{
				calendar
			};

			// Act
			var occurrences = calendars
								.GetOccurrences(new CalDateTime(new DateTime(2050, 1, 3, 0, 0, 0, DateTimeKind.Unspecified), null, false))
								.TakeWhileBefore(new CalDateTime(new DateTime(2050, 1, 4, 0, 0, 0, DateTimeKind.Unspecified), null, false))
								.ToList();

			// Assert: all-day event should have no time component
			Assert.Single(occurrences);
			var o = occurrences[0];
			var src = Assert.IsType<CalendarEvent>(o.Source);
			Assert.Equal("All-day event on Jan 3", src.Summary);
			Assert.True(src.IsAllDay, "Event should be all-day");
			Assert.False(o.Period.StartTime.HasTime, "Start time should have no time component");
		}

		// ---------------------------------------------------------------------------
		// Helpers for building inline iCal strings with dates relative to a given day
		// ---------------------------------------------------------------------------

		private static string BuildTimedUtcEventCalendar(DateTime eventDate, string summary, string calendarName = null)
		{
			var startUtc = new DateTime(eventDate.Year, eventDate.Month, eventDate.Day, 14, 0, 0, DateTimeKind.Utc);
			var endUtc = startUtc.AddHours(1);
			var calNameLine = calendarName != null ? $@"
X-WR-CALNAME:{calendarName}" : string.Empty;

			return @$"BEGIN:VCALENDAR
PRODID:-//Test//Test//EN
VERSION:2.0{calNameLine}
BEGIN:VEVENT
DTSTART:{startUtc:yyyyMMdd'T'HHmmss'Z'}
DTEND:{endUtc:yyyyMMdd'T'HHmmss'Z'}
DTSTAMP:20240101T000000Z
UID:test-utc@test
SUMMARY:{summary}
END:VEVENT
END:VCALENDAR";
		}

		private static string BuildTimedBerlinEventCalendar(DateTime eventDate, string summary)
		{
			return @$"BEGIN:VCALENDAR
PRODID:-//Test//Test//EN
VERSION:2.0
BEGIN:VTIMEZONE
TZID:Europe/Berlin
BEGIN:STANDARD
TZOFFSETFROM:+0200
TZOFFSETTO:+0100
TZNAME:CET
DTSTART:19701025T030000
RRULE:FREQ=YEARLY;BYMONTH=10;BYDAY=-1SU
END:STANDARD
BEGIN:DAYLIGHT
TZOFFSETFROM:+0100
TZOFFSETTO:+0200
TZNAME:CEST
DTSTART:19700329T020000
RRULE:FREQ=YEARLY;BYMONTH=3;BYDAY=-1SU
END:DAYLIGHT
END:VTIMEZONE
BEGIN:VEVENT
DTSTART;TZID=Europe/Berlin:{eventDate:yyyyMMdd'T'}150000
DTEND;TZID=Europe/Berlin:{eventDate:yyyyMMdd'T'}160000
DTSTAMP:20240101T000000Z
UID:test-berlin@test
SUMMARY:{summary}
END:VEVENT
END:VCALENDAR";
		}

		private static string BuildAllDayEventCalendar(DateTime eventDate, string summary)
		{
			var nextDay = eventDate.AddDays(1);
			return @$"BEGIN:VCALENDAR
PRODID:-//Test//Test//EN
VERSION:2.0
BEGIN:VEVENT
DTSTART;VALUE=DATE:{eventDate:yyyyMMdd}
DTEND;VALUE=DATE:{nextDay:yyyyMMdd}
DTSTAMP:20240101T000000Z
UID:test-allday@test
SUMMARY:{summary}
END:VEVENT
END:VCALENDAR";
		}

		private static string BuildMultiDayEventCalendar(DateTime startDate, int durationDays, string summary)
		{
			var endDate = startDate.AddDays(durationDays);
			return @$"BEGIN:VCALENDAR
PRODID:-//Test//Test//EN
VERSION:2.0
BEGIN:VEVENT
DTSTART;VALUE=DATE:{startDate:yyyyMMdd}
DTEND;VALUE=DATE:{endDate:yyyyMMdd}
DTSTAMP:20240101T000000Z
UID:test-multiday@test
SUMMARY:{summary}
END:VEVENT
END:VCALENDAR";
		}

		// ---------------------------------------------------------------------------
		// Tests for GetEvents(StringBuilder, CalendarCollection, DateTime)
		// ---------------------------------------------------------------------------

		[Fact]
		public void GetEvents_WithSingleUtcEvent_ReturnsOneEvent()
		{
			// Arrange
			var today = DateTime.Today;
			var eventDate = today.AddDays(1); // tomorrow, safely within the 2-year window
			var calendar = ICalExtensions.LoadCalendar(BuildTimedUtcEventCalendar(eventDate, "UTC timed event"));
			var calendars = new CalendarCollection { calendar };
			var sbErrors = new StringBuilder();

			// Act
			var events = ICalExtensions.GetEvents(calendars, today, sbErrors);

			// Assert
			Assert.Single(events);
			Assert.Equal("UTC timed event", events[0].Summary);
			Assert.Equal(eventDate.Date, events[0].Date);
			Assert.False(events[0].IsAllDay, "Timed event should not be all-day");
			Assert.True(events[0].Start.HasValue, "Timed event should have a start time");
			Assert.True(events[0].End.HasValue, "Timed event should have an end time");
		}

		[Fact]
		public void GetEvents_WithNamedTimezoneEvent_ReturnsOneEvent()
		{
			// Arrange
			var today = DateTime.Today;
			var eventDate = today.AddDays(1);
			var calendar = ICalExtensions.LoadCalendar(BuildTimedBerlinEventCalendar(eventDate, "Berlin TZ event"));
			var calendars = new CalendarCollection { calendar };
			var sbErrors = new StringBuilder();

			// Act
			var events = ICalExtensions.GetEvents(calendars, today, sbErrors);

			// Assert
			Assert.Single(events);
			Assert.Equal("Berlin TZ event", events[0].Summary);
			Assert.Equal(eventDate.Date, events[0].Date);
			Assert.False(events[0].IsAllDay, "Timed event should not be all-day");
			Assert.True(events[0].Start.HasValue, "Timed event should have a start time");
			Assert.True(events[0].End.HasValue, "Timed event should have an end time");
		}

		[Fact]
		public void GetEvents_WithAllDayEvent_ReturnsAllDayEvent()
		{
			// Arrange
			var today = DateTime.Today;
			var eventDate = today.AddDays(1);
			var calendar = ICalExtensions.LoadCalendar(BuildAllDayEventCalendar(eventDate, "All-day event"));
			var calendars = new CalendarCollection { calendar };
			var sbErrors = new StringBuilder();

			// Act
			var events = ICalExtensions.GetEvents(calendars, today, sbErrors);

			// Assert
			Assert.Single(events);
			Assert.Equal("All-day event", events[0].Summary);
			Assert.Equal(eventDate.Date, events[0].Date);
			Assert.True(events[0].IsAllDay, "Event should be all-day");
			Assert.False(events[0].Start.HasValue, "All-day event should have no start time");
			Assert.False(events[0].End.HasValue, "All-day event should have no end time");
		}

		[Fact]
		public void GetEvents_WithMultiDayEvent_ReturnsOneEventPerDay()
		{
			// Arrange
			var today = DateTime.Today;
			var eventStart = today.AddDays(1);
			const int durationDays = 3;
			var calendar = ICalExtensions.LoadCalendar(BuildMultiDayEventCalendar(eventStart, durationDays, "Multi-day event"));
			var calendars = new CalendarCollection { calendar };
			var sbErrors = new StringBuilder();

			// Act
			var events = ICalExtensions.GetEvents(calendars, today, sbErrors);

			// Assert: one Event entry per day
			Assert.Equal(durationDays, events.Count);
			for (var i = 0; i < durationDays; i++)
			{
				Assert.Equal("Multi-day event", events[i].Summary);
				Assert.Equal(eventStart.AddDays(i).Date, events[i].Date);
				Assert.True(events[i].IsAllDay, $"Day {i + 1} of multi-day event should be all-day");
			}
		}

		[Fact]
		public void GetEvents_WithEmptyCalendar_ReturnsEmptyListAndPopulatesError()
		{
			// Arrange
			var today = DateTime.Today;
			var emptyCalendarContent = @"BEGIN:VCALENDAR
PRODID:-//Test//Test//EN
VERSION:2.0
END:VCALENDAR";
			var calendar = ICalExtensions.LoadCalendar(emptyCalendarContent);
			var calendars = new CalendarCollection { calendar };
			var sbErrors = new StringBuilder();

			// Act
			var events = ICalExtensions.GetEvents(calendars, today, sbErrors);

			// Assert
			Assert.Empty(events);
			Assert.Contains("No events", sbErrors.ToString());
		}

		[Fact]
		public void GetEvents_EventBeforeDate_IsNotReturned()
		{
			// Arrange – event is yesterday, date is today
			var today = DateTime.Today;
			var eventDate = today.AddDays(-1); // yesterday
			var calendar = ICalExtensions.LoadCalendar(BuildTimedUtcEventCalendar(eventDate, "Past event"));
			var calendars = new CalendarCollection { calendar };
			var sbErrors = new StringBuilder();

			// Act
			var events = ICalExtensions.GetEvents(calendars, today, sbErrors);

			// Assert: past event is not in the results
			Assert.DoesNotContain(events, e => e.Summary == "Past event");
		}

		[Fact]
		public void GetEvents_WithCalendarName_CalendarNameIsNull()
		{
			// X-WR-CALNAME is a VCALENDAR-level property; the Event.CalendarName field
			// uses calendarEvent.Properties["X-WR-CALNAME"] which reads from VEVENT properties,
			// so it is always null unless explicitly set on each individual VEVENT.
			var today = DateTime.Today;
			var eventDate = today.AddDays(1);
			const string calendarName = "My Test Calendar";
			var calendar = ICalExtensions.LoadCalendar(BuildTimedUtcEventCalendar(eventDate, "Named calendar event", calendarName));
			var calendars = new CalendarCollection { calendar };
			var sbErrors = new StringBuilder();

			// Act
			var events = ICalExtensions.GetEvents(calendars, today, sbErrors);

			// Assert
			Assert.Single(events);
			Assert.Null(events[0].CalendarName);
		}

	}
}

using System;

namespace InkyCal.Utils.Calendar
{
	/// <summary>
	/// An event that may take place on a calendar.
	/// </summary>
	public class Event
	{
		/// <summary>
		/// The (start) date
		/// </summary>
		/// <value>
		/// The date.
		/// </value>
		public DateTime Date { get; set; }
		/// <summary>
		/// The start time <c>null</c> when all-day event
		/// </summary>
		/// <value>
		/// The start.
		/// </value>
		public TimeSpan? Start { get; set; }
		/// <summary>
		/// The end time <c>null</c> when all-day event
		/// </summary>
		/// <value>
		/// The end time
		/// </value>
		public TimeSpan? End { get; set; }
		/// <summary>
		/// Gets or sets the name of the calendar the vent belongs to
		/// </summary>
		/// <value>
		/// The name of the calendar.
		/// </value>
		public string CalendarName { get; set; }
		/// <summary>
		/// Gets the summary, a.k.a. the description.
		/// </summary>
		/// <value>
		/// The summary.
		/// </value>
		public string Summary { get; internal set; }
		/// <summary>
		/// Gets a value indicating whether this instance is all day.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is all day; otherwise, <c>false</c>.
		/// </value>
		public bool IsAllDay => !Start.HasValue && !End.HasValue;
	}
}

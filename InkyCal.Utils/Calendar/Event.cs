// Ignore Spelling: Utils

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace InkyCal.Utils.Calendar
{
	/// <summary>
	/// An event that may take place on a calendar.
	/// </summary>
	[SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "It's a calendar event")]
	public class Event : IEquatable<Event>
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

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj) => Equals(obj as Event);

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
		/// </returns>
		public virtual bool Equals(Event other) => other != null &&
				   Date.Date == other.Date.Date &&
				   Date.Hour == other.Date.Hour &&
				   Date.Minute == other.Date.Minute &&
				   EqualityComparer<TimeSpan?>.Default.Equals(Start, other.Start) &&
				   EqualityComparer<TimeSpan?>.Default.Equals(End, other.End) &&
				   //CalendarName == other.CalendarName &&
				   string.Equals(Summary?.Trim(), other.Summary?.Trim(), StringComparison.InvariantCultureIgnoreCase);

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode() => HashCode.Combine(
			 Date.Date
			, Date.Hour
			, Date.Minute
			, Start
			, End
			, Summary
			);

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator ==(Event left, Event right)
		{
			return EqualityComparer<Event>.Default.Equals(left, right);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator !=(Event left, Event right)
		{
			return !(left == right);
		}

		/// <summary>
		/// Indicated if the event is now.
		/// </summary>
		/// <returns></returns>
		public bool IsNow() => Date.Add(Start.GetValueOrDefault()) <= DateTime.Now
				&& Date.Add(End.GetValueOrDefault()) >= DateTime.Now;
	}
}

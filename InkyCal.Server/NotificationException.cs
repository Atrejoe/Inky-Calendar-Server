using System;

namespace InkyCal.Server
{
	/// <summary>
	/// A simple exception class for notifications only.
	/// </summary>
	[Serializable]
	public class NotificationException : Exception
	{
		/// <inheritdoc/>
		public NotificationException() { }
		/// <inheritdoc/>
		public NotificationException(string message) : base(message) { }
		/// <inheritdoc/>
		public NotificationException(string message, Exception inner) : base(message, inner) { }
		/// <inheritdoc/>
		protected NotificationException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}

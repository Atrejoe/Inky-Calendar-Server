using System;

namespace InkyCal.Server
{
	/// <summary>
	/// A simple exception class for notifications only.
	/// </summary>
	public class NotificationException : Exception
	{
		/// <inheritdoc/>
		public NotificationException() { }
		/// <inheritdoc/>
		public NotificationException(string message) : base(message) { }
		/// <inheritdoc/>
		public NotificationException(string message, Exception inner) : base(message, inner) { }
	}

	/// <summary>
	/// These are the namespace comments for <c>Company.Product.Widgets</c>.
	/// </summary>
	[System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
	class NamespaceDoc
	{
	}
}

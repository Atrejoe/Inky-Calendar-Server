// Ignore Spelling: Utils

using System;

namespace InkyCal.Utils
{
	/// <summary>
	/// A failure when a panel renderer could not be obtained.
	/// </summary>
	public class GetRendererException : Exception
	{
		/// <inheritdoc/>
		public GetRendererException() { }
		/// <inheritdoc/>
		public GetRendererException(string message) : base(message) { }
		/// <inheritdoc/>
		public GetRendererException(string message, Exception inner) : base(message, inner) { }
	}
}

using System;

namespace InkyCal.Utils
{
	/// <summary>
	/// A exception while attempting to download a file
	/// </summary>
	public class DownloadException : Exception
	{
		/// <inheritdoc/>
		public DownloadException() { }

		/// <inheritdoc/>
		public DownloadException(string message) : base(message) { }

		/// <inheritdoc/>
		public DownloadException(string message, Exception inner) : base(message, inner) { }
	}
}

using System;
using Bugsnag;

namespace InkyCal.Utils
{
	/// <summary>
	/// A helper class for logging and tracing
	/// </summary>
	public static class PerformanceMonitor
	{
		private static readonly Client _bugsnag;

		static PerformanceMonitor()
		{
			if (!string.IsNullOrWhiteSpace(Server.Config.Config.BugSnagAPIKey))
				_bugsnag = new Client(new Configuration(Server.Config.Config.BugSnagAPIKey));
		}

		/// <summary>
		/// Logs the specified exception to all registered exception handlers.
		/// </summary>
		/// <param name="ex">The ex.</param>
		public static void Log(this Exception ex) {
			_bugsnag?.Notify(ex);
		}
	}
}

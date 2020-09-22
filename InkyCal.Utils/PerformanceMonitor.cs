using System;
using System.Diagnostics.CodeAnalysis;
using Bugsnag;

namespace InkyCal.Utils
{
	/// <summary>
	/// A helper class for logging and tracing
	/// </summary>
	public static class PerformanceMonitor
	{
		private static readonly Client _bugsnag;

		[SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "Conditional initializatiop")]
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
			if (ex is null)
				return;

			if (_bugsnag is null)
			{
				Console.Error.WriteLine($"Bugsnag not configured (API key: {Server.Config.Config.BugSnagAPIKey})");
				Console.Error.WriteLine(ex.ToString());
			}
			else
			{
				Console.WriteLine($"Logging error to bugsnag : {ex.Message}");
				_bugsnag.Notify(ex,Severity.Error);
			}
		}
	}
}

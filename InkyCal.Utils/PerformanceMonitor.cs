﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Bugsnag;
using Bugsnag.Payload;

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
		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
		public static void Log(this System.Exception ex)
		{
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
				_bugsnag.Notify(ex, Severity.Error, (report) =>
				{
					//List exception properties (this is not done by BugSnag for some reason)
					foreach (var p
					in ex
						.GetType()
						.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty)
						//.Where(x => !x.DeclaringType.Equals(typeof(System.Exception)))
						.Where(x => typeof(System.Exception).IsAssignableFrom(x.DeclaringType))
					)
					{
						try
						{
							report.Event.Metadata.AddToPayload(p.Name, p.GetValue(ex));
						}
						catch (System.Exception pv) {
							try
							{
								report.Event.Metadata.AddToPayload(p.Name, $"Failure to obtain value: {pv.Message}");
							}
							finally {
							}
						}
					}

					//Identify authenticated user (this is a gamble)
					var identity = System.Threading.Thread.CurrentPrincipal?.Identity;
					if (identity != null) {
						report.Event.User = new Bugsnag.Payload.User
						{
							Name = identity.Name
						};
					}
				});
			}
		}
	}
}

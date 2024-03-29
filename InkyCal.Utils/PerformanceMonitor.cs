﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Bugsnag;
using Bugsnag.Payload;
using Newtonsoft.Json;
using Sentry;
using StackExchange.Profiling;

namespace InkyCal.Utils
{

	/// <summary>
	/// A helper class for logging and tracing
	/// </summary>
	public static class PerformanceMonitor
	{
		/// <summary>
		/// Serialized any object to a <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/>, <see cref="string"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj">The object.</param>
		/// <returns></returns>
		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
		public static Dictionary<string, string> SerializeToDictionary<T>(this T obj) => typeof(T)
					.GetProperties()
					.Where(x => x.CanRead)
					.ToDictionary(
						x => x.Name,
						x =>
						{
							try
							{
								return x.GetValue(obj)?.ToString() ?? "null";
							}
							catch (System.Exception ex)
							{
								return $"Failed to obtaiConsole.Writen value: {ex.Message}";
							}
						});

		private static  Client __bugsnag;
		
		private static Client BugsnagClient { get {
				if (string.IsNullOrWhiteSpace(Server.Config.Config.BugSnagAPIKey))
					return null;

				return __bugsnag ??= new Client(new Configuration(Server.Config.Config.BugSnagAPIKey));
			}
		}


		/// <summary>
		/// Logs the specified exception to all registered exception handlers.
		/// </summary>
		/// <param name="ex">The ex.</param>
		/// <param name="user">The authenticated user, if any</param>
		/// <param name="severity"></param>
		public static void Log(this System.Exception ex, Models.User user = null, Severity severity = Severity.Error)
		{
			if (ex is null)
				return;

			var fgColor = severity switch
			{
				Severity.Info => ConsoleColor.DarkGreen,
				Severity.Warning => ConsoleColor.Yellow,
				Severity.Error => ConsoleColor.Red,
				_ => ConsoleColor.Red,
			};

			var severityAsString = $"{severity}".ToLower(CultureInfo.CurrentUICulture);

			using (fgColor.TempForegroundColor())
				Console.Write(severityAsString);

			if (BugsnagClient is null && !SentrySdk.IsEnabled)
			{
				Console.Error.WriteLine($"Bugsnag nor Sentry have been configured.");
				Console.Error.WriteLine(ex.ToString());
			}

			if (BugsnagClient != null)
			{
				Console.Error.WriteLine($": Logging {severityAsString} to Bugsnag : {ex.Message}");

				using (MiniProfiler.Current.Step("Reporting error to BugSnag"))
					BugsnagClient.Notify(ex, severity.MapToBugSnag(), (report) => FillReport(report, user));

			}

			if (SentrySdk.IsEnabled)
				SentrySdk.CaptureException(ex);
		}

		/// <summary>
		/// Traces a message to the logger
		/// </summary>
		public static void Trace(string message, Dictionary<string, string> metaData = null)
		{
			//Write to console
			if (metaData is null || !metaData.Any())
				Console.WriteLine($"{message}");
			else
				Console.WriteLine($"{message} : {JsonConvert.SerializeObject(metaData, Formatting.Indented)}");

			//Write as BugSnag breadcurmbs
			BugsnagClient?.Breadcrumbs
					.Leave(message, BreadcrumbType.Process, metaData);

			//Write as Sentry message (without metaData)
			if (SentrySdk.IsEnabled)
				SentrySdk.CaptureMessage(message);
		}

		/// <summary>
		/// Fills the report.
		/// </summary>
		/// <param name="report">The report.</param>
		/// <param name="user"></param>

		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
		public static void FillReport(Report report, Models.User user = null)
		{
			if (report is null)
				return;

			//List exception properties (this is not done by BugSnag for some reason)
			foreach (var p
			in report.OriginalException
				.GetType()
				.GetProperties()
				.Where(p => p.CanRead && !p.Name.Equals(nameof(System.Exception.StackTrace)))
				.Where(x => typeof(System.Exception).IsAssignableFrom(x.DeclaringType))
			)
			{
				try
				{
					var value = p.GetValue(report.OriginalException);
					object serialiazableValue;
					if (value is null)
						serialiazableValue = "null";
					else if (value is IDictionary d)
					{
						var sd = new Dictionary<string, string>();
						var e = d.GetEnumerator();
						while (e.MoveNext())
							sd[e.Key.ToString()] = e.Value?.ToString() ?? "null";

						serialiazableValue = sd;
					}
					else
						serialiazableValue = value.ToString();

					report.Event.Metadata.AddToPayload(p.Name, serialiazableValue);
				}
				catch (System.Exception pv)
				{
					try
					{
						report.Event.Metadata.AddToPayload(p.Name, $"Failure to obtain value: {pv.Message}");
					}
					catch (System.Exception) { 
						//ignore
					}
				}
			}

			if (!(user is null))
				report.Event.User = new Bugsnag.Payload.User
				{
					Id = $"{user.Id}"
				};
			else
			{
				//Identify authenticated user (this is a gamble)
				var identity = System.Threading.Thread.CurrentPrincipal?.Identity;
				if (identity != null)
				{
					report.Event.User = new Bugsnag.Payload.User
					{
						Name = identity.Name
					};
				}
			}

			Console.Error.WriteLine($"Extended report");
		}
	}
}

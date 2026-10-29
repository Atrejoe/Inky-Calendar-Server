using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Sentry.AspNetCore;

namespace InkyCal.Server
{

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public static class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					if (!string.IsNullOrWhiteSpace(Config.Config.SentryDSN))
					{
						webBuilder.UseSentry(o =>
						{
							o.Dsn = Config.Config.SentryDSN;
							// Panel rendering is low-volume and the part we want to see, sample it (much) more than the rest,
							// which is mostly page views and vulnerability scanners.
							o.TracesSampler = context =>
								context.TryGetHttpPath()?.StartsWith("/panel", StringComparison.OrdinalIgnoreCase) == true
									? Config.Config.SentryPanelTracesSampleRate
									: Config.Config.SentryTracesSampleRate;
						});
					}

					webBuilder.UseStartup<Startup>();
				});
	}
}

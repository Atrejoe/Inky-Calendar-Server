using System;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using Bugsnag.AspNet.Core;
using InkyCal.Data;
using InkyCal.Server.Areas.Identity;
using InkyCal.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using StackExchange.Profiling.Storage;

namespace InkyCal.Server
{

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

	public class Startup
	{
		//public Startup(IConfiguration configuration)
		//{
		//	Configuration = configuration;
		//}

		//public IConfiguration Configuration { get; }

		public static readonly string Intro = @"A web API for <a href=""https://github.com/aceisace/Inky-Calendar"" target=""github"">InkyCal</a>, allows to offload panel-generating complexity to an easier to maintain web service.";

		// This method gets called by the runtime. Use this method to add services to the container.
		public static void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();
			services.AddHealthChecks()
				.AddSqlServer(Config.Config.ConnectionString,failureStatus: HealthStatus.Degraded); // Some functions may work, non-user configured (or otherwise cached) methods

			//services.AddMvc().AddJsonOptions(options =>
			//{
			//    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
			//    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
			//});

			if (!string.IsNullOrEmpty(Config.Config.BugSnagAPIKey))
			{
				services.AddBugsnag(configuration =>
				{
					configuration.ApiKey = Config.Config.BugSnagAPIKey;
				});

				Bugsnag.InternalMiddleware.AttachGlobalMetadata = (report) => PerformanceMonitor.FillReport(report);
			}

			services.AddMvc()
				.AddJsonOptions(options =>
					options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
				);


			//Migrate on startup
			using (var db = new ApplicationDbContext())
			{

				Console.WriteLine($@"Applied migrations:");
				foreach (var migration in db.Database.GetAppliedMigrations())
					Console.WriteLine(migration);
				Console.WriteLine();

				Console.WriteLine(@"Available migrations:");
				foreach (var migration in db.Database.GetMigrations())
					Console.WriteLine(migration);
				Console.WriteLine();

				Console.WriteLine(@"Pending migrations:");
				foreach (var migration in db.Database.GetPendingMigrations())
					Console.WriteLine(migration);
				Console.WriteLine();

				bool isMigrationNeeded = db.Database.GetPendingMigrations().Any();
				if (isMigrationNeeded)
					db.Database.Migrate();
				else
					Console.WriteLine(@"No migrations required");
			}

			services.AddDatabaseDeveloperPageExceptionFilter();
			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(
					Config.Config.ConnectionString,
					options =>
					{
						options.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name);
					}));

			services.AddDefaultIdentity<IdentityUser>(
				options =>
				{
					options.SignIn.RequireConfirmedAccount = false;
					options.Password.RequiredLength = 10;
				}
				)
				.AddEntityFrameworkStores<ApplicationDbContext>();

			services.AddRazorPages();
			services.AddServerSideBlazor();
			services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
			//services.AddSingleton<WeatherForecastService>();

			// Register the Swagger generator, defining 1 or more Swagger documents
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo
				{
					Title = $"InkyCal Server",
					Version = "v1",
					Description = Intro,
					Contact = new OpenApiContact
					{
						Name = "Atrejoe",
						//Email = "devlog@cs.nl",
						Url = new Uri("https://github.com/Atrejoe")
					}
				});

				//c.DescribeAllEnumsAsStrings();

				Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly).ToList().ForEach(
					xmlFile =>
					c.IncludeXmlComments(xmlFile, includeControllerXmlComments: true)
				);

			});

			try
			{
				throw new NotificationException("Application has started");
			}
			catch (NotificationException ex)
			{
				ex.Log(severity: Severity.Info);
			}

			// Note .AddMiniProfiler() returns a IMiniProfilerBuilder for easy intellisense
			services.AddMiniProfiler(options =>
			{
				// All of this is optional. You can simply call .AddMiniProfiler() for all defaults

				// (Optional) Path to use for profiler URLs, default is /mini-profiler-resources
				options.RouteBasePath = "/profiler";

				// (Optional) Control storage
				// (default is 30 minutes in MemoryCacheStorage)
				// Note: MiniProfiler will not work if a SizeLimit is set on MemoryCache!
				//   See: https://github.com/MiniProfiler/dotnet/issues/501 for details
				(options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(60);

				// (Optional) Control which SQL formatter to use, InlineFormatter is the default
				options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.VerboseSqlServerFormatter(includeMetaData: true);

				// (Optional) To control authorization, you can use the Func<HttpRequest, bool> options:
				// (default is everyone can access profilers)
				//options.ResultsAuthorize = request => MyGetUserFunction(request).CanSeeMiniProfiler;
				options.ResultsAuthorizeAsync = async request =>
				{
					var context = request.HttpContext;

					var authResult = await context.AuthenticateAsync();
					if (!authResult.Succeeded)
						return false;

					var claimsPrincipal = authResult.Principal;
					return claimsPrincipal.Identity.IsAuthenticated;
				};
				//options.ResultsListAuthorize = request => MyGetUserFunction(request).CanSeeMiniProfiler;
				// Or, there are async versions available:
				//options.ResultsListAuthorize = async request => (await MyGetUserFunctionAsync(request)).CanSeeMiniProfiler;
				//options.ResultsListAuthorizeAsync = async request => (await MyGetUserFunctionAsync(request)).CanSeeMiniProfilerLists;

				options.ResultsListAuthorizeAsync = async request =>
				{
					var context = request.HttpContext;

					var authResult = await context.AuthenticateAsync();
					if (!authResult.Succeeded)
						return false;

					var claimsPrincipal = authResult.Principal;
					return claimsPrincipal.Identity.IsAuthenticated;
				};

				// (Optional)  To control which requests are profiled, use the Func<HttpRequest, bool> option:
				// (default is everything should be profiled)
				//options.ShouldProfile = request => MyShouldThisBeProfiledFunction(request);

				// (Optional) Profiles are stored under a user ID, function to get it:
				// (default is null, since above methods don't use it by default)
				//options.UserIdProvider = request => MyGetUserIdFunction(request);

				// (Optional) Swap out the entire profiler provider, if you want
				// (default handles async and works fine for almost all applications)
				//options.ProfilerProvider = new MyProfilerProvider();

				// (Optional) You can disable "Connection Open()", "Connection Close()" (and async variant) tracking.
				// (defaults to true, and connection opening/closing is tracked)
				options.TrackConnectionOpenClose = true;

				// (Optional) Use something other than the "light" color scheme.
				// (defaults to "light")
				options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;

				// The below are newer options, available in .NET Core 3.0 and above:

				// (Optional) You can disable MVC filter profiling
				// (defaults to true, and filters are profiled)
				options.EnableMvcFilterProfiling = true;
				// ...or only save filters that take over a certain millisecond duration (including their children)
				// (defaults to null, and all filters are profiled)
				// options.MvcFilterMinimumSaveMs = 1.0m;

				// (Optional) You can disable MVC view profiling
				// (defaults to true, and views are profiled)
				options.EnableMvcViewProfiling = true;
				// ...or only save views that take over a certain millisecond duration (including their children)
				// (defaults to null, and all views are profiled)
				// options.MvcViewMinimumSaveMs = 1.0m;

				// (Optional) listen to any errors that occur within MiniProfiler itself
				// options.OnInternalError = e => MyExceptionLogger(e);

				// (Optional - not recommended) You can enable a heavy debug mode with stacks and tooltips when using memory storage
				// It has a lot of overhead vs. normal profiling and should only be used with that in mind
				// (defaults to false, debug/heavy mode is off)
				//options.EnableDebugMode = true;
			}).AddEntityFramework();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseMiniProfiler();

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseRouting();

			if (!string.IsNullOrEmpty(Config.Config.SentryDSN))
			{
				// Enable automatic tracing integration.
				// If running with .NET 5 or below, make sure to put this middleware
				// right after `UseRouting()`.
				app.UseSentryTracing();
			}

			app.UseForwardedHeaders(new ForwardedHeadersOptions
			{
				ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
			});

			app.UseAuthorization();
			app.UseAuthentication();

			//app.UseEndpoints(endpoints =>
			//{
			//	endpoints.MapControllers();
			//	endpoints.MapBlazorHub();
			//});

			// Enable middleware to serve generated Swagger as a JSON endpoint.
			app.UseSwagger();

			// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
			// specifying the Swagger JSON endpoint.
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inky Calender service V1");
				c.EnableDeepLinking();
			});

			//var option = new RewriteOptions();
			//option.AddRedirect("^$", "swagger");

			//app.UseRewriter(option);

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapBlazorHub();
				endpoints.MapFallbackToPage("/_Host");
				endpoints.MapHealthChecks("/health"
				, new HealthCheckOptions
				{
					ResultStatusCodes =
						{
							[HealthStatus.Healthy] = StatusCodes.Status200OK,
							[HealthStatus.Degraded] = StatusCodes.Status417ExpectationFailed,
							[HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
						}
				});
			});
		}
	}
}

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
using Microsoft.OpenApi;
using StackExchange.Profiling.Storage;

namespace InkyCal.Server
{

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

	public class Startup
	{
		public static readonly string Intro = @"A web API for <a href=""https://github.com/aceisace/Inky-Calendar"" target=""github"">InkyCal</a>, allows to offload panel-generating complexity to an easier to maintain web service.";

		// This method gets called by the runtime. Use this method to add services to the container.
		public static void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();
			
			var databaseEnabled = Config.Config.DatabaseEnabled;
			Console.WriteLine($"Database support: {(databaseEnabled ? "ENABLED" : "DISABLED")}");

			// Configure health checks
			if (databaseEnabled && !string.IsNullOrEmpty(Config.Config.ConnectionString))
			{
				services.AddHealthChecks()
					.AddSqlServer(Config.Config.ConnectionString, failureStatus: HealthStatus.Degraded);
			}
			else
			{
				services.AddHealthChecks();
			}

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

			// Configure database and identity only if database is enabled
			if (databaseEnabled)
			{
				if (string.IsNullOrEmpty(Config.Config.ConnectionString))
				{
					Console.WriteLine("WARNING: Database is enabled but no connection string is provided. Database features will not work.");
				}
				else
				{
					// Migrate on startup
					try
					{
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
							{
								Console.WriteLine(@"Running database migrations...");
								db.Database.Migrate();
								Console.WriteLine(@"Migrations completed successfully");
							}
							else
								Console.WriteLine(@"No migrations required");
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine($"WARNING: Failed to connect to database or run migrations: {ex.Message}");
						Console.WriteLine("The application will continue without database support.");
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

					// Register database-backed panel repository
					services.AddScoped<IPanelRepository, DatabasePanelRepository>();
				}
			}
			else
			{
				Console.WriteLine("Running in database-less mode with in-memory storage.");
				Console.WriteLine("Note: User authentication and panel persistence are disabled.");
				
				// Register in-memory panel repository
				services.AddSingleton<IPanelRepository, InMemoryPanelRepository>();
			}

			services.AddRazorPages();
			services.AddServerSideBlazor();
			
			if (databaseEnabled && !string.IsNullOrEmpty(Config.Config.ConnectionString))
			{
				services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
			}

			// Register the Swagger generator, defining 1 or more Swagger documents
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo
				{
					Title = $"InkyCal Server",
					Version = "v1",
					Description = Intro + (databaseEnabled ? "" : " (Running in database-less mode)"),
					Contact = new OpenApiContact
					{
						Name = "Atrejoe",
						Url = new Uri("https://github.com/Atrejoe")
					}
				});

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

			// Configure MiniProfiler
			var miniProfilerBuilder = services.AddMiniProfiler(options =>
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

					if (!databaseEnabled)
						return false;

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

				options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.VerboseSqlServerFormatter(includeMetaData: true);

				options.ResultsAuthorizeAsync = async request =>
				{
					if (!databaseEnabled)
						return false;

					var context = request.HttpContext;
					var authResult = await context.AuthenticateAsync();
					if (!authResult.Succeeded)
						return false;
					var claimsPrincipal = authResult.Principal;
					return claimsPrincipal.Identity.IsAuthenticated;
				};

				options.ResultsListAuthorizeAsync = async request =>
				{

					if (!databaseEnabled)
						return false;

					var context = request.HttpContext;
					var authResult = await context.AuthenticateAsync();
					if (!authResult.Succeeded)
						return false;
					var claimsPrincipal = authResult.Principal;
					return claimsPrincipal.Identity.IsAuthenticated;
				};

				options.TrackConnectionOpenClose = true;
				options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;
				options.EnableMvcFilterProfiling = true;
				options.EnableMvcViewProfiling = true;
				options.EnableServerTimingHeader = true;

				options.IgnoredPaths.Add("/health");
				options.IgnoredPaths.Add("/_blazor");
				options.IgnoredPaths.Add("/_Host");
				options.IgnoredPaths.Add("/_host");
				options.IgnoredPaths.Add("/css");
				options.IgnoredPaths.Add(".js");
				options.IgnoredPaths.Add(".css");
			});

			// Only add Entity Framework profiling if database is enabled
			if (databaseEnabled && !string.IsNullOrEmpty(Config.Config.ConnectionString))
			{
				miniProfilerBuilder.AddEntityFramework();
			}
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

			app.UseForwardedHeaders(new ForwardedHeadersOptions
			{
				ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
			});

			if (Config.Config.DatabaseEnabled && !string.IsNullOrEmpty(Config.Config.ConnectionString))
			{
				app.UseAuthentication();
			}
			
			app.UseAuthorization();

			// Enable middleware to serve generated Swagger as a JSON endpoint.
			app.UseSwagger(options => {
				options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
			});

			// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
			// specifying the Swagger JSON endpoint.
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inky Calender service V1");
				c.EnableDeepLinking();
			});

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
						},
					ResponseWriter = async (context, health) =>
					{
						await context.Response.WriteAsync($"[{health.Status}] - ");
						switch (health.Status)
						{
							case HealthStatus.Unhealthy:
								await context.Response.WriteAsync("I don't feel too well, please restart me.");
								break;
							case HealthStatus.Degraded:
								await context.Response.WriteAsync("I don't feel too well, but it's not my fault. Do not restart me.");
								break;
							case HealthStatus.Healthy:
								await context.Response.WriteAsync("I'm super, thanks for asking!");
								break;
						}

						// Authenticated users see more details
						if (context.User != null && (context.User.Identity?.IsAuthenticated).GetValueOrDefault())
						{
							foreach (var check in health.Entries)
								await context.Response.WriteAsync($"\n - [{check.Value.Duration:c}] \"{check.Key}\" : {check.Value.Status} {(
									string.IsNullOrWhiteSpace(check.Value.Description) || string.Equals(check.Value.Description, check.Value.ToString())
										? ""
										: $"- {check.Value.Description} "
										)}{(
									check.Value.Tags.Any()
										? $"[{string.Join(",", check.Value.Tags)}] "
										: "")}{
									check.Value.Exception?.Message}");
						}
					}
				});
			});
		}
	}
}

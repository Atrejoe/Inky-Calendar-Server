using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Bugsnag;
using Bugsnag.AspNet.Core;
using InkyCal.Data;
using InkyCal.Server.Areas.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

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

		public static readonly string Intro = @"A web API for <a href=""https://github.com/aceisace/Inky-Calendar"">InkyCal</a>, allows to offload panel-generating complexity to an easier to maintain web service.";

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();

			//services.AddMvc().AddJsonOptions(options =>
			//{
			//    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
			//    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
			//});

			if (!string.IsNullOrEmpty(Config.Config.BugSnagAPIKey))
				services.AddBugsnag(configuration =>
				{
					configuration.ApiKey = Config.Config.BugSnagAPIKey;
				});

			services.AddMvc()
				.AddJsonOptions(options =>
					options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
				);

			//Migrate on startup
			using (var db = new ApplicationDbContext())
			{

				Console.WriteLine("Applied migrations:");
				foreach (var migration in db.Database.GetAppliedMigrations())
					Console.WriteLine(migration);
				Console.WriteLine();

				Console.WriteLine("Available migrations:");
				foreach (var migration in db.Database.GetMigrations())
					Console.WriteLine(migration);
				Console.WriteLine();

				Console.WriteLine("Pending migrations:");
				foreach (var migration in db.Database.GetPendingMigrations())
					Console.WriteLine(migration);
				Console.WriteLine();

				bool isMigrationNeeded = db.Database.GetPendingMigrations().Any();
				if (isMigrationNeeded)
					db.Database.Migrate();
				else
					Console.WriteLine("No migrations required");
			}

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
					Title = $"InkyCal",
					Version = "v1",
					Description = Intro,
					Contact = new OpenApiContact
					{
						Name = "Atrejoe",
						//Email = "devlog@cs.nl",
						Url = new Uri("https://github.com/Atrejoe"),
					}
				});

				//c.DescribeAllEnumsAsStrings();

				var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

				//... and tell Swagger to use those XML comments.
				c.IncludeXmlComments(xmlPath, true);
			});

			Utils.PerformanceMonitor.Log(new Exception("Application has started"));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseDatabaseErrorPage();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseRouting();

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
			});
		}
	}
}

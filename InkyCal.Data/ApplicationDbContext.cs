using InkyCal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InkyCal.Data
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public class ApplicationDbContext : IdentityDbContext
	{

		public ApplicationDbContext() : base()
		{
		}

		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{
		}

		//private static StreamWriter writer = File.AppendText("QueryLog.txt");

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{

			optionsBuilder.UseSqlServer(
				InkyCal.Server.Config.Config.ConnectionString,
				options =>
				options.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name));

			if (InkyCal.Server.Config.Config.TraceQueries)

				optionsBuilder.LogTo(msg =>
					{
						System.Console.WriteLine(msg);
						//System.Console.Error.WriteLine(msg);
						System.Diagnostics.Debug.WriteLine(msg);
						//await writer.WriteLineAsync(msg);
					});

			base.OnConfiguring(optionsBuilder);
		}
		private static readonly object writelock = new object();

		protected override void OnModelCreating(ModelBuilder builder)
		{
			if (builder is null)
				throw new System.ArgumentNullException(nameof(builder));


			base.OnModelCreating(builder);

			builder.Entity<Panel>()
				.HasOne(x => x.Owner)
				.WithMany(x => x.Panels)
				.HasForeignKey("OwnerId")
				.OnDelete(DeleteBehavior.NoAction);

			builder.Entity<Panel>()
				.HasDiscriminator();

			builder.Entity<CalendarPanel>();
			builder.Entity<NewYorkTimesPanel>();

			builder.Entity<CalendarPanelUrl>()
				.HasOne(x => x.Panel)
				.WithMany(x => x.CalenderUrls)
				.HasForeignKey(x => x.IdPanel)
				.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<PanelOfPanels>()
				   .HasMany(x => x.Panels)
				   .WithOne()
				   .HasForeignKey(x => x.IdParent)
				   .OnDelete(DeleteBehavior.Cascade);

			builder.Entity<SubPanel>()
				   .HasOne(x => x.Panel)
				   .WithMany()
				   .HasForeignKey(x => x.IdPanel)
				   .OnDelete(DeleteBehavior.NoAction);

			builder.Entity<SubPanel>()
				   .HasKey(x => new { x.IdParent, x.IdPanel, x.SortIndex });

			builder.Entity<CalendarPanelUrl>()
				.Property(x => x.Url)
				.HasConversion<string>();

			builder.Entity<ImagePanel>()
				.Property(x => x.Path)
				.HasConversion<string>();

			builder.Entity<User>()
				.HasIndex(x => x.IdentityUserId).IsUnique();


			builder.Entity<WeatherPanel>();

			builder.Entity<User>()
				.HasOne(typeof(IdentityUser));

			builder.Entity<GoogleOAuthAccess>();

			builder.Entity<SubscribedGoogleCalender>()
				.HasOne(x => x.AccessToken)
				.WithMany()
				.HasForeignKey(nameof(SubscribedGoogleCalender.IdAccessToken))
				.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<SubscribedGoogleCalender>()
				.HasKey(x => new { x.Calender, x.IdAccessToken, x.Panel });

			builder.Entity<SubscribedGoogleCalender>()
				.HasAlternateKey(x => new { x.Calender, x.IdAccessToken, x.Panel });

			builder.Entity<SubscribedGoogleCalender>()
				.HasOne<CalendarPanel>()
				.WithMany(x => x.SubscribedGoogleCalenders)
				.HasForeignKey(x => x.Panel)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}

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

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{

			optionsBuilder.UseSqlServer(
				InkyCal.Server.Config.Config.ConnectionString,
				options =>
				options.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name));

			base.OnConfiguring(optionsBuilder);
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			if (builder is null)
				throw new System.ArgumentNullException(nameof(builder));


			base.OnModelCreating(builder);

			builder.Entity<Panel>()
				.HasOne(x=>x.Owner)
				.WithMany(x=>x.Panels);

			builder.Entity<CalendarPanel>();
			builder.Entity<NewYorkTimesPanel>();

			builder.Entity<CalendarPanelUrl>()
				.HasOne(x => x.Panel)
				.WithMany(x => x.CalenderUrls)
				.HasForeignKey(x => x.IdPanel);

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
				.HasOne(typeof(GoogleOAuthAccess))
				.WithMany()
				.HasForeignKey(nameof(SubscribedGoogleCalender.AccessToken));

			builder.Entity<SubscribedGoogleCalender>()
				.HasOne(typeof(CalendarPanel))
				.WithMany()
				.HasForeignKey(nameof(SubscribedGoogleCalender.Panel));

			builder.Entity<SubscribedGoogleCalender>()
				.HasKey(x => new { x.Calender, x.AccessToken, x.Panel });

			builder.Entity<SubscribedGoogleCalender>()
				.HasAlternateKey(x => new { x.Calender, x.AccessToken, x.Panel });

			builder.Entity<SubscribedGoogleCalender>()
				.HasOne<CalendarPanel>()
				.WithMany(x=>x.SubscribedGoogleCalenders)
				.HasForeignKey(x=>x.Panel);
		}
	}
}

﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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
			ArgumentNullException.ThrowIfNull(optionsBuilder);

			optionsBuilder
				.EnableSensitiveDataLogging()
				.EnableDetailedErrors();

			optionsBuilder.UseSqlServer(
				Server.Config.Config.ConnectionString,
				options =>
				options.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name));

			if (Server.Config.Config.TraceQueries)

				optionsBuilder.LogTo(msg =>
					{
						Console.WriteLine(msg);
						Debug.WriteLine(msg);
					});

			base.OnConfiguring(optionsBuilder);
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			ArgumentNullException.ThrowIfNull(builder);

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

			builder.Entity<NewsPaperPanel>();
		}

		public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
		{

			var now = DateTime.UtcNow;

			foreach (var changedEntity in ChangeTracker.Entries())
			{
				if (changedEntity.Entity is ITimeStampable entity)
				{
					switch (changedEntity.State)
					{
						case EntityState.Added:
							entity.Created = now;
							entity.Modified = now;
							break;

						case EntityState.Modified:
							Entry(entity).Property(x => x.Created).IsModified = false;

							if (!entity.SkipModificationTimestamp)
								entity.Modified = now;

							break;
					}
				}
			}

			return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
		}
	}
}

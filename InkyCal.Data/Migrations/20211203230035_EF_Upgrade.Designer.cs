﻿// <auto-generated />
using System;
using InkyCal.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace InkyCal.Data.Migrations
{

	/// <summary>
	/// Upgrades to new Entity Framework
	/// </summary>
	/// <seealso cref="Microsoft.EntityFrameworkCore.Migrations.Migration" />
	[DbContext(typeof(ApplicationDbContext))]
	[Migration("20211203230035_EF_Upgrade")]
	partial class EF_Upgrade
	{
		/// <inheritdoc/>
		protected override void BuildTargetModel(ModelBuilder modelBuilder)
		{
#pragma warning disable 612, 618
			modelBuilder
				.HasAnnotation("ProductVersion", "6.0.0")
				.HasAnnotation("Relational:MaxIdentifierLength", 128);

			SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

			modelBuilder.Entity("InkyCal.Models.CalendarPanelUrl", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd()
						.HasColumnType("int");

					SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

					b.Property<Guid>("IdPanel")
						.HasColumnType("uniqueidentifier");

					b.Property<string>("Url")
						.IsRequired()
						.HasColumnType("nvarchar(max)");

					b.HasKey("Id");

					b.HasIndex("IdPanel");

					b.ToTable("CalendarPanelUrl");
				});

			modelBuilder.Entity("InkyCal.Models.GoogleOAuthAccess", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd()
						.HasColumnType("int");

					SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

					b.Property<string>("AccessToken")
						.IsRequired()
						.HasMaxLength(200)
						.HasColumnType("nvarchar(200)");

					b.Property<DateTimeOffset>("AccessTokenExpiry")
						.HasColumnType("datetimeoffset");

					b.Property<string>("RefreshToken")
						.IsRequired()
						.HasMaxLength(200)
						.HasColumnType("nvarchar(200)");

					b.Property<int>("UserId")
						.HasColumnType("int");

					b.HasKey("Id");

					b.HasIndex("UserId");

					b.ToTable("User_GoogleOAuthAccess", "InkyCal");
				});

			modelBuilder.Entity("InkyCal.Models.Panel", b =>
				{
					b.Property<Guid>("Id")
						.ValueGeneratedOnAdd()
						.HasColumnType("uniqueidentifier")
						.HasColumnOrder(0);

					b.Property<long>("AccessCount")
						.HasColumnType("bigint");

					b.Property<DateTime>("Accessed")
						.ValueGeneratedOnAdd()
						.HasColumnType("datetime2");

					b.Property<DateTime>("Created")
						.ValueGeneratedOnAdd()
						.HasColumnType("datetime2");

					b.Property<string>("Discriminator")
						.IsRequired()
						.HasColumnType("nvarchar(max)");

					b.Property<int?>("Height")
						.HasColumnType("int");

					b.Property<int>("Model")
						.HasColumnType("int");

					b.Property<DateTime>("Modified")
						.ValueGeneratedOnAddOrUpdate()
						.HasColumnType("datetime2");

					b.Property<string>("Name")
						.IsRequired()
						.HasMaxLength(255)
						.HasColumnType("nvarchar(255)")
						.HasColumnOrder(1);

					b.Property<int?>("OwnerId")
						.HasColumnType("int");

					b.Property<short>("Rotation")
						.HasColumnType("smallint");

					b.Property<bool>("Starred")
						.HasColumnType("bit");

					b.Property<int?>("Width")
						.HasColumnType("int");

					b.HasKey("Id");

					b.HasIndex("OwnerId");

					b.ToTable("Panel", "InkyCal");

					b.HasDiscriminator<string>("Discriminator").HasValue("Panel");
				});

			modelBuilder.Entity("InkyCal.Models.SubPanel", b =>
				{
					b.Property<Guid>("IdParent")
						.HasColumnType("uniqueidentifier");

					b.Property<Guid>("IdPanel")
						.HasColumnType("uniqueidentifier");

					b.Property<byte>("SortIndex")
						.HasColumnType("tinyint");

					b.Property<short>("Ratio")
						.HasColumnType("smallint");

					b.HasKey("IdParent", "IdPanel", "SortIndex");

					b.HasIndex("IdPanel");

					b.ToTable("SubPanel", "InkyCal");
				});

			modelBuilder.Entity("InkyCal.Models.SubscribedGoogleCalender", b =>
				{
					b.Property<string>("Calender")
						.HasMaxLength(255)
						.HasColumnType("nvarchar(255)");

					b.Property<int>("IdAccessToken")
						.HasColumnType("int")
						.HasColumnName("AccessToken");

					b.Property<Guid>("Panel")
						.HasColumnType("uniqueidentifier");

					b.HasKey("Calender", "IdAccessToken", "Panel");

					b.HasIndex("IdAccessToken");

					b.HasIndex("Panel");

					b.ToTable("CalendarPanel_GoogleCalender", "InkyCal");
				});

			modelBuilder.Entity("InkyCal.Models.User", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd()
						.HasColumnType("int");

					SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

					b.Property<string>("IdentityUserId")
						.HasColumnType("nvarchar(450)");

					b.HasKey("Id");

					b.HasIndex("IdentityUserId")
						.IsUnique()
						.HasFilter("[IdentityUserId] IS NOT NULL");

					b.ToTable("User", "InkyCal");
				});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
				{
					b.Property<string>("Id")
						.HasColumnType("nvarchar(450)");

					b.Property<string>("ConcurrencyStamp")
						.IsConcurrencyToken()
						.HasColumnType("nvarchar(max)");

					b.Property<string>("Name")
						.HasMaxLength(256)
						.HasColumnType("nvarchar(256)");

					b.Property<string>("NormalizedName")
						.HasMaxLength(256)
						.HasColumnType("nvarchar(256)");

					b.HasKey("Id");

					b.HasIndex("NormalizedName")
						.IsUnique()
						.HasDatabaseName("RoleNameIndex")
						.HasFilter("[NormalizedName] IS NOT NULL");

					b.ToTable("AspNetRoles", (string)null);
				});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd()
						.HasColumnType("int");

					SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

					b.Property<string>("ClaimType")
						.HasColumnType("nvarchar(max)");

					b.Property<string>("ClaimValue")
						.HasColumnType("nvarchar(max)");

					b.Property<string>("RoleId")
						.IsRequired()
						.HasColumnType("nvarchar(450)");

					b.HasKey("Id");

					b.HasIndex("RoleId");

					b.ToTable("AspNetRoleClaims", (string)null);
				});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUser", b =>
				{
					b.Property<string>("Id")
						.HasColumnType("nvarchar(450)");

					b.Property<int>("AccessFailedCount")
						.HasColumnType("int");

					b.Property<string>("ConcurrencyStamp")
						.IsConcurrencyToken()
						.HasColumnType("nvarchar(max)");

					b.Property<string>("Email")
						.HasMaxLength(256)
						.HasColumnType("nvarchar(256)");

					b.Property<bool>("EmailConfirmed")
						.HasColumnType("bit");

					b.Property<bool>("LockoutEnabled")
						.HasColumnType("bit");

					b.Property<DateTimeOffset?>("LockoutEnd")
						.HasColumnType("datetimeoffset");

					b.Property<string>("NormalizedEmail")
						.HasMaxLength(256)
						.HasColumnType("nvarchar(256)");

					b.Property<string>("NormalizedUserName")
						.HasMaxLength(256)
						.HasColumnType("nvarchar(256)");

					b.Property<string>("PasswordHash")
						.HasColumnType("nvarchar(max)");

					b.Property<string>("PhoneNumber")
						.HasColumnType("nvarchar(max)");

					b.Property<bool>("PhoneNumberConfirmed")
						.HasColumnType("bit");

					b.Property<string>("SecurityStamp")
						.HasColumnType("nvarchar(max)");

					b.Property<bool>("TwoFactorEnabled")
						.HasColumnType("bit");

					b.Property<string>("UserName")
						.HasMaxLength(256)
						.HasColumnType("nvarchar(256)");

					b.HasKey("Id");

					b.HasIndex("NormalizedEmail")
						.HasDatabaseName("EmailIndex");

					b.HasIndex("NormalizedUserName")
						.IsUnique()
						.HasDatabaseName("UserNameIndex")
						.HasFilter("[NormalizedUserName] IS NOT NULL");

					b.ToTable("AspNetUsers", (string)null);
				});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
				{
					b.Property<int>("Id")
						.ValueGeneratedOnAdd()
						.HasColumnType("int");

					SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

					b.Property<string>("ClaimType")
						.HasColumnType("nvarchar(max)");

					b.Property<string>("ClaimValue")
						.HasColumnType("nvarchar(max)");

					b.Property<string>("UserId")
						.IsRequired()
						.HasColumnType("nvarchar(450)");

					b.HasKey("Id");

					b.HasIndex("UserId");

					b.ToTable("AspNetUserClaims", (string)null);
				});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
				{
					b.Property<string>("LoginProvider")
						.HasMaxLength(128)
						.HasColumnType("nvarchar(128)");

					b.Property<string>("ProviderKey")
						.HasMaxLength(128)
						.HasColumnType("nvarchar(128)");

					b.Property<string>("ProviderDisplayName")
						.HasColumnType("nvarchar(max)");

					b.Property<string>("UserId")
						.IsRequired()
						.HasColumnType("nvarchar(450)");

					b.HasKey("LoginProvider", "ProviderKey");

					b.HasIndex("UserId");

					b.ToTable("AspNetUserLogins", (string)null);
				});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
				{
					b.Property<string>("UserId")
						.HasColumnType("nvarchar(450)");

					b.Property<string>("RoleId")
						.HasColumnType("nvarchar(450)");

					b.HasKey("UserId", "RoleId");

					b.HasIndex("RoleId");

					b.ToTable("AspNetUserRoles", (string)null);
				});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
				{
					b.Property<string>("UserId")
						.HasColumnType("nvarchar(450)");

					b.Property<string>("LoginProvider")
						.HasMaxLength(128)
						.HasColumnType("nvarchar(128)");

					b.Property<string>("Name")
						.HasMaxLength(128)
						.HasColumnType("nvarchar(128)");

					b.Property<string>("Value")
						.HasColumnType("nvarchar(max)");

					b.HasKey("UserId", "LoginProvider", "Name");

					b.ToTable("AspNetUserTokens", (string)null);
				});

			modelBuilder.Entity("InkyCal.Models.CalendarPanel", b =>
				{
					b.HasBaseType("InkyCal.Models.Panel");

					b.ToTable("Panel", "InkyCal");

					b.HasDiscriminator().HasValue("CalendarPanel");
				});

			modelBuilder.Entity("InkyCal.Models.ImagePanel", b =>
				{
					b.HasBaseType("InkyCal.Models.Panel");

					b.Property<string>("Body")
						.HasColumnType("nvarchar(max)");

					b.Property<short>("ImageRotation")
						.HasColumnType("smallint");

					b.Property<string>("Path")
						.IsRequired()
						.HasColumnType("nvarchar(max)");

					b.ToTable("Panel", "InkyCal");

					b.HasDiscriminator().HasValue("ImagePanel");
				});

			modelBuilder.Entity("InkyCal.Models.NewYorkTimesPanel", b =>
				{
					b.HasBaseType("InkyCal.Models.Panel");

					b.ToTable("Panel", "InkyCal");

					b.HasDiscriminator().HasValue("NewYorkTimesPanel");
				});

			modelBuilder.Entity("InkyCal.Models.PanelOfPanels", b =>
				{
					b.HasBaseType("InkyCal.Models.Panel");

					b.ToTable("Panel", "InkyCal");

					b.HasDiscriminator().HasValue("PanelOfPanels");
				});

			modelBuilder.Entity("InkyCal.Models.WeatherPanel", b =>
				{
					b.HasBaseType("InkyCal.Models.Panel");

					b.Property<string>("Location")
						.IsRequired()
						.HasMaxLength(255)
						.HasColumnType("nvarchar(255)");

					b.Property<string>("Token")
						.IsRequired()
						.HasMaxLength(255)
						.HasColumnType("nvarchar(255)");

					b.ToTable("Panel", "InkyCal");

					b.HasDiscriminator().HasValue("WeatherPanel");
				});

			modelBuilder.Entity("InkyCal.Models.CalendarPanelUrl", b =>
				{
					b.HasOne("InkyCal.Models.CalendarPanel", "Panel")
						.WithMany("CalenderUrls")
						.HasForeignKey("IdPanel")
						.OnDelete(DeleteBehavior.Cascade)
						.IsRequired();

					b.Navigation("Panel");
				});

			modelBuilder.Entity("InkyCal.Models.GoogleOAuthAccess", b =>
				{
					b.HasOne("InkyCal.Models.User", "User")
						.WithMany()
						.HasForeignKey("UserId")
						.OnDelete(DeleteBehavior.Cascade)
						.IsRequired();

					b.Navigation("User");
				});

			modelBuilder.Entity("InkyCal.Models.Panel", b =>
				{
					b.HasOne("InkyCal.Models.User", "Owner")
						.WithMany("Panels")
						.HasForeignKey("OwnerId");

					b.Navigation("Owner");
				});

			modelBuilder.Entity("InkyCal.Models.SubPanel", b =>
				{
					b.HasOne("InkyCal.Models.Panel", "Panel")
						.WithMany()
						.HasForeignKey("IdPanel")
						.OnDelete(DeleteBehavior.NoAction)
						.IsRequired();

					b.HasOne("InkyCal.Models.PanelOfPanels", null)
						.WithMany("Panels")
						.HasForeignKey("IdParent")
						.OnDelete(DeleteBehavior.Cascade)
						.IsRequired();

					b.Navigation("Panel");
				});

			modelBuilder.Entity("InkyCal.Models.SubscribedGoogleCalender", b =>
				{
					b.HasOne("InkyCal.Models.GoogleOAuthAccess", "AccessToken")
						.WithMany()
						.HasForeignKey("IdAccessToken")
						.OnDelete(DeleteBehavior.Cascade)
						.IsRequired();

					b.HasOne("InkyCal.Models.CalendarPanel", null)
						.WithMany("SubscribedGoogleCalenders")
						.HasForeignKey("Panel")
						.OnDelete(DeleteBehavior.Cascade)
						.IsRequired();

					b.Navigation("AccessToken");
				});

			modelBuilder.Entity("InkyCal.Models.User", b =>
				{
					b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
						.WithMany()
						.HasForeignKey("IdentityUserId");
				});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
				{
					b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
						.WithMany()
						.HasForeignKey("RoleId")
						.OnDelete(DeleteBehavior.Cascade)
						.IsRequired();
				});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
				{
					b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
						.WithMany()
						.HasForeignKey("UserId")
						.OnDelete(DeleteBehavior.Cascade)
						.IsRequired();
				});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
				{
					b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
						.WithMany()
						.HasForeignKey("UserId")
						.OnDelete(DeleteBehavior.Cascade)
						.IsRequired();
				});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
				{
					b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
						.WithMany()
						.HasForeignKey("RoleId")
						.OnDelete(DeleteBehavior.Cascade)
						.IsRequired();

					b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
						.WithMany()
						.HasForeignKey("UserId")
						.OnDelete(DeleteBehavior.Cascade)
						.IsRequired();
				});

			modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
				{
					b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
						.WithMany()
						.HasForeignKey("UserId")
						.OnDelete(DeleteBehavior.Cascade)
						.IsRequired();
				});

			modelBuilder.Entity("InkyCal.Models.User", b =>
				{
					b.Navigation("Panels");
				});

			modelBuilder.Entity("InkyCal.Models.CalendarPanel", b =>
				{
					b.Navigation("CalenderUrls");

					b.Navigation("SubscribedGoogleCalenders");
				});

			modelBuilder.Entity("InkyCal.Models.PanelOfPanels", b =>
				{
					b.Navigation("Panels");
				});
#pragma warning restore 612, 618
		}
	}
}

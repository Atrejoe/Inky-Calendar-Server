using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InkyCal.Data.Migrations
{
    /// <inheritdoc />
    public partial class CorrectPanelTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			// Alternations based on fix
			migrationBuilder.AlterColumn<DateTime>(
				name: "Accessed",
				schema: "InkyCal",
				table: "Panel",
				defaultValueSql: "GetUTCDate()");

			migrationBuilder.AlterColumn<DateTime>(
				name: "Created",
				schema: "InkyCal",
				table: "Panel",
				defaultValueSql: "GetUTCDate()");

			migrationBuilder.AlterColumn<DateTime>(
				name: "Modified",
				schema: "InkyCal",
				table: "Panel",
				defaultValueSql: "GetUTCDate()");

			// Alternations based on package upgrade

			migrationBuilder.DropPrimaryKey("PK_AspNetUserTokens", "AspNetUserTokens");

			migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

			migrationBuilder.AddPrimaryKey("PK_AspNetUserTokens", "AspNetUserTokens", new string[] { "UserId", "LoginProvider", "Name" });

			migrationBuilder.DropPrimaryKey("PK_AspNetUserLogins", "AspNetUserLogins");

			migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

			migrationBuilder.AddPrimaryKey("PK_AspNetUserLogins", "AspNetUserLogins", new string[] { "LoginProvider", "ProviderKey" });
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			// Alternations based on fix
			migrationBuilder.AlterColumn<DateTime>(
				name: "Accessed",
				schema: "InkyCal",
				table: "Panel",
				defaultValue: DateTime.UtcNow);

			migrationBuilder.AlterColumn<DateTime>(
				name: "Created",
				schema: "InkyCal",
				table: "Panel",
				defaultValue: DateTime.UtcNow);

			migrationBuilder.AlterColumn<DateTime>(
				name: "Modified",
				schema: "InkyCal",
				table: "Panel",
				defaultValue: DateTime.UtcNow);

			// Alternations based on package upgrade
			migrationBuilder.DropPrimaryKey("PK_AspNetUserTokens", "AspNetUserTokens");

			migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

			migrationBuilder.AddPrimaryKey("PK_AspNetUserTokens", "AspNetUserTokens", new string[] { "UserId", "LoginProvider", "Name" });

			migrationBuilder.DropPrimaryKey("PK_AspNetUserLogins", "AspNetUserLogins");

			migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

			migrationBuilder.AddPrimaryKey("PK_AspNetUserLogins", "AspNetUserLogins", new string[] { "LoginProvider", "ProviderKey" });
		}
    }
}

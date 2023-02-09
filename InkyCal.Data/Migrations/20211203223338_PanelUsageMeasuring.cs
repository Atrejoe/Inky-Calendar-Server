using System;
using InkyCal.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InkyCal.Data.Migrations
{

	/// <summary>
	/// Introduces measuring of <see cref="Panel"/> usage.
	/// </summary>
	/// <seealso cref="Migration" />
	public partial class PanelUsageMeasuring : Migration
	{
		/// <inheritdoc/>
		protected override void Up(MigrationBuilder migrationBuilder)
		{

			migrationBuilder.AddColumn<long>(
				name: "AccessCount",
				schema: "InkyCal",
				table: "Panel",
				type: "bigint",
				nullable: false,
				defaultValue: 0L);

			migrationBuilder.AddColumn<DateTime>(
				name: "Accessed",
				schema: "InkyCal",
				table: "Panel",
				type: "datetime2",
				nullable: false,
				defaultValue: DateTime.UtcNow);

			migrationBuilder.AddColumn<DateTime>(
				name: "Created",
				schema: "InkyCal",
				table: "Panel",
				type: "datetime2",
				nullable: false,
				defaultValue: DateTime.UtcNow);

			migrationBuilder.AddColumn<DateTime>(
				name: "Modified",
				schema: "InkyCal",
				table: "Panel",
				type: "datetime2",
				nullable: false,
				defaultValue: DateTime.UtcNow);

		}

		/// <inheritdoc/>
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "AccessCount",
				schema: "InkyCal",
				table: "Panel");

			migrationBuilder.DropColumn(
				name: "Accessed",
				schema: "InkyCal",
				table: "Panel");

			migrationBuilder.DropColumn(
				name: "Body",
				schema: "InkyCal",
				table: "Panel");

			migrationBuilder.DropColumn(
				name: "Created",
				schema: "InkyCal",
				table: "Panel");
		}
	}
}

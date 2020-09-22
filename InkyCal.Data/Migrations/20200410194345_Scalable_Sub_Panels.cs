using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace InkyCal.Data.Migrations
{
	[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Naming not bad enough to tackle")]
	public partial class Scalable_Sub_Panels : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<byte>(
				name: "Ratio",
				schema: "InkyCal",
				table: "SubPanel",
				nullable: false,
				defaultValue: (byte)100);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "Ratio",
				schema: "InkyCal",
				table: "SubPanel");
		}
	}
}

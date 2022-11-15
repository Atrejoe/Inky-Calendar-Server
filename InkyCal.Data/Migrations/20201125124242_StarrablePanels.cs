using Microsoft.EntityFrameworkCore.Migrations;

namespace InkyCal.Data.Migrations
{


	/// <summary>
	/// Introduction of <see cref="Models.Panel.Starred"/>.
	/// </summary>
	/// <seealso cref="Microsoft.EntityFrameworkCore.Migrations.Migration" />
	public partial class StarrablePanels : Migration
	{
		/// <inheritdoc/>
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<bool>(
				name: "Starred",
				schema: "InkyCal",
				table: "Panel",
				nullable: false,
				defaultValue: false);
		}

		/// <inheritdoc/>
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "Starred",
				schema: "InkyCal",
				table: "Panel");
		}
	}
}

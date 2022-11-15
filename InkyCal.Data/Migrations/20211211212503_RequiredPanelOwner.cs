using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InkyCal.Data.Migrations
{

	/// <summary>
	/// Requires panel owner
	/// </summary>
	/// <seealso cref="Microsoft.EntityFrameworkCore.Migrations.Migration" />
	public partial class RequiredPanelOwner : Migration
	{
		/// <inheritdoc/>
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<int>(
				name: "OwnerId",
				schema: "InkyCal",
				table: "Panel",
				type: "int",
				nullable: false,
				defaultValue: 0,
				oldClrType: typeof(int),
				oldType: "int",
				oldNullable: true);
		}

		/// <inheritdoc/>
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<int>(
				name: "OwnerId",
				schema: "InkyCal",
				table: "Panel",
				type: "int",
				nullable: true,
				oldClrType: typeof(int),
				oldType: "int");
		}
	}
}

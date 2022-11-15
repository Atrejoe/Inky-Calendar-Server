using Microsoft.EntityFrameworkCore.Migrations;

namespace InkyCal.Data.Migrations
{

	/// <summary>
	/// Increases sub-panel ratios (<see cref="Models.SubPanel.Ratio"/>)
	/// </summary>
	/// <seealso cref="Microsoft.EntityFrameworkCore.Migrations.Migration" />
	public partial class EnlargeSubPanelRatio : Migration
    {
		/// <inheritdoc/>
		protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "Ratio",
                schema: "InkyCal",
                table: "SubPanel",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");
        }

		/// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "Ratio",
                schema: "InkyCal",
                table: "SubPanel",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(short));
        }
    }
}

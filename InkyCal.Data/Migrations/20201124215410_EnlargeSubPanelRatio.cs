using Microsoft.EntityFrameworkCore.Migrations;

namespace InkyCal.Data.Migrations
{
    public partial class EnlargeSubPanelRatio : Migration
    {
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

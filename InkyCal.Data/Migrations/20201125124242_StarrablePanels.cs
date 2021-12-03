using Microsoft.EntityFrameworkCore.Migrations;

namespace InkyCal.Data.Migrations
{
    public partial class StarrablePanels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Starred",
                schema: "InkyCal",
                table: "Panel",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Starred",
                schema: "InkyCal",
                table: "Panel");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InkyCal.Data.Migrations
{
    /// <inheritdoc />
    public partial class GenericNewsPaperPanel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NewsPaperId",
                schema: "InkyCal",
                table: "Panel",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewsPaperId",
                schema: "InkyCal",
                table: "Panel");
        }
    }
}

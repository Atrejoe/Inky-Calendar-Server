using Microsoft.EntityFrameworkCore.Migrations;

namespace InkyCal.Data.Migrations
{

	/// <summary>
	/// Maintenance: renames index
	/// </summary>
	/// <seealso cref="Migration" />
	public partial class RenameToCalendarPanelUrl : Migration
    {
		/// <inheritdoc/>
		protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImagePanelUrl_Panel_IdPanel",
                schema: "InkyCal",
                table: "ImagePanelUrl");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ImagePanelUrl",
                schema: "InkyCal",
                table: "ImagePanelUrl");

            migrationBuilder.RenameTable(
                name: "ImagePanelUrl",
                schema: "InkyCal",
                newName: "CalendarPanelUrl",
                newSchema: "InkyCal");

            migrationBuilder.RenameIndex(
                name: "IX_ImagePanelUrl_IdPanel",
                schema: "InkyCal",
                table: "CalendarPanelUrl",
                newName: "IX_CalendarPanelUrl_IdPanel");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CalendarPanelUrl",
                schema: "InkyCal",
                table: "CalendarPanelUrl",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarPanelUrl_Panel_IdPanel",
                schema: "InkyCal",
                table: "CalendarPanelUrl",
                column: "IdPanel",
                principalSchema: "InkyCal",
                principalTable: "Panel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

		/// <inheritdoc/>
		protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalendarPanelUrl_Panel_IdPanel",
                schema: "InkyCal",
                table: "CalendarPanelUrl");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CalendarPanelUrl",
                schema: "InkyCal",
                table: "CalendarPanelUrl");

            migrationBuilder.RenameTable(
                name: "CalendarPanelUrl",
                schema: "InkyCal",
                newName: "ImagePanelUrl",
                newSchema: "InkyCal");

            migrationBuilder.RenameIndex(
                name: "IX_CalendarPanelUrl_IdPanel",
                schema: "InkyCal",
                table: "ImagePanelUrl",
                newName: "IX_ImagePanelUrl_IdPanel");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImagePanelUrl",
                schema: "InkyCal",
                table: "ImagePanelUrl",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImagePanelUrl_Panel_IdPanel",
                schema: "InkyCal",
                table: "ImagePanelUrl",
                column: "IdPanel",
                principalSchema: "InkyCal",
                principalTable: "Panel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

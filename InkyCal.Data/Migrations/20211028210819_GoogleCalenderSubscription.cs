using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace InkyCal.Data.Migrations
{
    public partial class GoogleCalenderSubscription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalendarPanel_GoogleCalender",
                schema: "InkyCal",
                columns: table => new
                {
                    Calender = table.Column<string>(maxLength: 255, nullable: false),
                    Panel = table.Column<Guid>(nullable: false),
                    AccessToken = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarPanel_GoogleCalender", x => new { x.Calender, x.AccessToken, x.Panel });
                    table.ForeignKey(
                        name: "FK_CalendarPanel_GoogleCalender_User_GoogleOAuthAccess_AccessToken",
                        column: x => x.AccessToken,
                        principalSchema: "InkyCal",
                        principalTable: "User_GoogleOAuthAccess",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalendarPanel_GoogleCalender_Panel_Panel",
                        column: x => x.Panel,
                        principalSchema: "InkyCal",
                        principalTable: "Panel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarPanel_GoogleCalender_AccessToken",
                schema: "InkyCal",
                table: "CalendarPanel_GoogleCalender",
                column: "AccessToken");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarPanel_GoogleCalender_Panel",
                schema: "InkyCal",
                table: "CalendarPanel_GoogleCalender",
                column: "Panel");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarPanel_GoogleCalender",
                schema: "InkyCal");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace InkyCal.Data.Migrations
{
    public partial class ModelImprovements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Panel_User_OwnerId",
                schema: "InkyCal",
                table: "Panel");

            migrationBuilder.AlterColumn<int>(
                name: "OwnerId",
                schema: "InkyCal",
                table: "Panel",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<short>(
                name: "ImageRotation",
                schema: "InkyCal",
                table: "Panel",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "InkyCal",
                table: "Panel",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<short>(
                name: "Rotation",
                schema: "InkyCal",
                table: "Panel",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateTable(
                name: "SubPanel",
                schema: "InkyCal",
                columns: table => new
                {
                    IdParent = table.Column<Guid>(nullable: false),
                    IdPanel = table.Column<Guid>(nullable: false),
                    SortIndex = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubPanel", x => new { x.IdParent, x.IdPanel, x.SortIndex });
                    table.ForeignKey(
                        name: "FK_SubPanel_Panel_IdPanel",
                        column: x => x.IdPanel,
                        principalSchema: "InkyCal",
                        principalTable: "Panel",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SubPanel_Panel_IdParent",
                        column: x => x.IdParent,
                        principalSchema: "InkyCal",
                        principalTable: "Panel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubPanel_IdPanel",
                schema: "InkyCal",
                table: "SubPanel",
                column: "IdPanel");

            migrationBuilder.AddForeignKey(
                name: "FK_Panel_User_OwnerId",
                schema: "InkyCal",
                table: "Panel",
                column: "OwnerId",
                principalSchema: "InkyCal",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Panel_User_OwnerId",
                schema: "InkyCal",
                table: "Panel");

            migrationBuilder.DropTable(
                name: "SubPanel",
                schema: "InkyCal");

            migrationBuilder.DropColumn(
                name: "ImageRotation",
                schema: "InkyCal",
                table: "Panel");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "InkyCal",
                table: "Panel");

            migrationBuilder.DropColumn(
                name: "Rotation",
                schema: "InkyCal",
                table: "Panel");

            migrationBuilder.AlterColumn<int>(
                name: "OwnerId",
                schema: "InkyCal",
                table: "Panel",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Panel_User_OwnerId",
                schema: "InkyCal",
                table: "Panel",
                column: "OwnerId",
                principalSchema: "InkyCal",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

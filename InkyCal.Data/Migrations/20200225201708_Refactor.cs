using Microsoft.EntityFrameworkCore.Migrations;

namespace InkyCal.Data.Migrations
{
	public partial class Refactor : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_User_AspNetUsers_IdIdentityUser",
				schema: "InkyCal",
				table: "User");

			migrationBuilder.DropIndex(
				name: "IX_User_IdIdentityUser",
				schema: "InkyCal",
				table: "User");

			migrationBuilder.RenameColumn(
				name: "IdIdentityUser",
				newName: "IdentityUserId",
				schema: "InkyCal",
				table: "User");

			migrationBuilder.CreateIndex(
				name: "IX_User_IdentityUserId",
				schema: "InkyCal",
				table: "User",
				column: "IdentityUserId",
				unique: true,
				filter: "[IdentityUserId] IS NOT NULL");

			migrationBuilder.AddForeignKey(
				name: "FK_User_AspNetUsers_IdentityUserId",
				schema: "InkyCal",
				table: "User",
				column: "IdentityUserId",
				principalTable: "AspNetUsers",
				principalColumn: "Id",
				onDelete: ReferentialAction.Restrict);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_User_AspNetUsers_IdentityUserId",
				schema: "InkyCal",
				table: "User");

			migrationBuilder.DropIndex(
				name: "IX_User_IdentityUserId",
				schema: "InkyCal",
				table: "User");

			migrationBuilder.RenameColumn(
				name: "IdentityUserId",
				newName: "IdIdentityUser",
				schema: "InkyCal",
				table: "User");

			migrationBuilder.CreateIndex(
				name: "IX_User_IdIdentityUser",
				schema: "InkyCal",
				table: "User",
				column: "IdIdentityUser",
				unique: true,
				filter: "[IdIdentityUser] IS NOT NULL");

			migrationBuilder.AddForeignKey(
				name: "FK_User_AspNetUsers_IdIdentityUser",
				schema: "InkyCal",
				table: "User",
				column: "IdIdentityUser",
				principalTable: "AspNetUsers",
				principalColumn: "Id",
				onDelete: ReferentialAction.Restrict);
		}
	}
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace InkyCal.Data.Migrations
{


	/// <summary>
	/// Introduces <see cref="Models.GoogleOAuthAccess"/>
	/// </summary>
	/// <seealso cref="Microsoft.EntityFrameworkCore.Migrations.Migration" />
	public partial class GoogleOAuth : Migration
	{
		/// <inheritdoc/>
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "User_GoogleOAuthAccess",
				schema: "InkyCal",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					UserId = table.Column<int>(nullable: false),
					AccessToken = table.Column<string>(maxLength: 200, nullable: false),
					AccessTokenExpiry = table.Column<DateTimeOffset>(nullable: false),
					RefreshToken = table.Column<string>(maxLength: 200, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_User_GoogleOAuthAccess", x => x.Id);
					table.ForeignKey(
						name: "FK_User_GoogleOAuthAccess_User_UserId",
						column: x => x.UserId,
						principalSchema: "InkyCal",
						principalTable: "User",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_User_GoogleOAuthAccess_UserId",
				schema: "InkyCal",
				table: "User_GoogleOAuthAccess",
				column: "UserId");
		}

		/// <inheritdoc/>
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "User_GoogleOAuthAccess",
				schema: "InkyCal");
		}
	}
}

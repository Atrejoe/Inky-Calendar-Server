using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InkyCal.Data.Migrations
{
    /// <inheritdoc />
    public partial class EFUpgrade702 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			//No model changes, just version number and update to Panel.Modified
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			//No model changes
		}
	}
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace InkyCal.Data.Migrations
{

	/// <summary>
	/// Introduces <see cref="Models.NewYorkTimesPanel"/>.
	/// </summary>
	/// <seealso cref="Migration" />
	public partial class NewYorkTimes : Migration
	{
		/// <inheritdoc/>
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			// No table was introduced, only a discriminator
		}

		/// <inheritdoc/>
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			// No table was introduced, only a discriminator
		}
	}
}

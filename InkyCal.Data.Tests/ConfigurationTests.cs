using Xunit;

namespace InkyCal.Data.Tests
{
	/// <summary>
	/// Validates that the application configuration is present.
	/// These tests do not require a database connection.
	/// </summary>
	/// <remarks>
	/// At least one passing test must exist in this project.
	/// When all other tests are skipped (e.g., because no database is available),
	/// the Microsoft Testing Platform runner would otherwise report "Zero tests ran"
	/// and return a non-zero exit code, causing CI to fail.
	/// </remarks>
	public class ConfigurationTests
	{
		[Fact]
		public void ConnectionStringIsConfigured()
		{
			var connectionString = Server.Config.Config.ConnectionString;
			Assert.NotNull(connectionString);
			Assert.NotEmpty(connectionString);
		}
	}
}

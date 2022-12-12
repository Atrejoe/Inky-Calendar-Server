using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Xunit;

namespace InkyCal.Data.Tests
{
	public static class SqlTestHelper
	{

		internal static async Task SkipConnectionException(this Task task)
		{
			try
			{
				//act
				await task;
			}
			catch (SqlException ex) when (ex.Number == -2) //https://learn.microsoft.com/en-us/sql/relational-databases/errors-events/database-engine-events-and-errors?view=sql-server-ver16
			{
				throw new SkipException("Connection timeout", ex);
			}
		}

		internal static async Task<T> SkipConnectionException<T>(this Task<T> task)
		{
			try
			{
				//act
				return await task;
			}
			catch (SqlException ex) when (ex.Number == -2) //https://learn.microsoft.com/en-us/sql/relational-databases/errors-events/database-engine-events-and-errors?view=sql-server-ver16
			{
				throw new SkipException("Connection timeout", ex);
			}
		}
	}
}

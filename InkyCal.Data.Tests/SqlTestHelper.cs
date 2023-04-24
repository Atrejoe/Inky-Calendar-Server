using System;
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
			catch (SqlException ex) when (ex.IsMissingSQLServerException()) //https://learn.microsoft.com/en-us/sql/relational-databases/errors-events/database-engine-events-and-errors?view=sql-server-ver16
			{
				throw new SkipException("Connection timeout", ex);
			}
			catch (Xunit.Sdk.ThrowsException ex) when (ex .InnerException is SqlException sqlEx && sqlEx.IsMissingSQLServerException())
			{
				throw new SkipException("Connection timeout (handled ThrowsException)", ex);
			}
			catch (SqlException ex)
			{
				throw new Exception($"Unhandled SQL exception (Type: {ex.GetType().Name}, Number: {ex.Number}/ Error code: {ex.ErrorCode}, Message: {ex.Message}, {ex})", ex);
			}
		}

		internal static async Task<T> SkipConnectionException<T>(this Task<T> task)
		{
			try
			{
				//act
				return await task;
			}
			catch (SqlException ex) when (ex.IsMissingSQLServerException()) //https://learn.microsoft.com/en-us/sql/relational-databases/errors-events/database-engine-events-and-errors?view=sql-server-ver16
			{
				throw new SkipException("Connection timeout", ex);
			}
			catch (Xunit.Sdk.ThrowsException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.IsMissingSQLServerException())
			{
				throw new SkipException("Connection timeout", ex);
			}
			catch (SqlException ex)
			{
				throw new Exception($"Unhandled exception (Number: {ex.Number}/ Error code: {ex.ErrorCode}, Message: {ex.Message})", ex);
			}
		}
	}
}

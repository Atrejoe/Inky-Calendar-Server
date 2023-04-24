using System;
using Microsoft.Data.SqlClient;

namespace InkyCal.Data.Tests
{
	internal static class ExceptionAnalysis
	{

		internal static bool IsMissingSQLServerException(this Exception ex)
		{

			if (ex is SqlException sqlEx
				&& (
					sqlEx.Number == 53
					||
					(sqlEx.Number == 0 && sqlEx.ErrorCode== -2146232060 && sqlEx.InnerException is System.Net.Sockets.SocketException)
				))
				return true;

			return false;
		}
	}
}

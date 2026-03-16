using System;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using StackExchange.Profiling;

namespace InkyCal.Data.Tests
{
	public abstract class RepositoryTestBase : IDisposable
	{
		private readonly TransactionScope _t;
		private bool disposedValue;

		protected RepositoryTestBase()
		{
			var options = MiniProfiler.DefaultOptions;
			options.AddEntityFramework();

			MiniProfiler.StartNew("My Profiler Name");

			new DbContextOptionsBuilder()
				.EnableDetailedErrors()
				.EnableSensitiveDataLogging();

			_t = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
		}

		protected virtual void Dispose(bool disposing)
		{

			if (!disposedValue)
			{
				if (disposing)
				{
					_t.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}

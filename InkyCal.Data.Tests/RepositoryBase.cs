using System;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using StackExchange.Profiling;
using Xunit.Abstractions;

namespace InkyCal.Data.Tests
{
	public abstract class RepositoryBase : IDisposable
	{

		protected readonly ITestOutputHelper output;
		private readonly TransactionScope _t;
		private bool disposedValue;

		protected RepositoryBase(ITestOutputHelper output)
		{
			var options = MiniProfiler.DefaultOptions;
			options.AddEntityFramework();

			MiniProfiler.StartNew("My Profiler Name");

			new DbContextOptionsBuilder()
				.EnableDetailedErrors()
				.EnableSensitiveDataLogging();

			_t = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
			this.output = output;
		}

		protected virtual void Dispose(bool disposing)
		{
			output.WriteLine(MiniProfiler.Current.RenderPlainText());

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

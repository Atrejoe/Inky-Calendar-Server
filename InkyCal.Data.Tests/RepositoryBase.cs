using System;
using System.Linq;
using System.Text;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using StackExchange.Profiling;
using Xunit.Abstractions;

namespace InkyCal.Data.Tests
{
	public abstract class RepositoryBase : IDisposable
	{

		protected readonly ITestOutputHelper output;
		private TransactionScope _t;
		private bool disposedValue;

		public RepositoryBase(ITestOutputHelper output)
		{
			var options = MiniProfiler.DefaultOptions;
			options.AddEntityFramework();

			MiniProfiler.StartNew("My Profiler Name");


			new DbContextOptionsBuilder().EnableDetailedErrors();

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

		// TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		~RepositoryBase()
		{
			//output.WriteLine(_profiler.RenderPlainText());

			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}

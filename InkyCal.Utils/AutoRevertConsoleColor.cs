using System;

namespace InkyCal.Utils
{
	/// <summary>
	/// An auto-reverting console color
	/// </summary>
	/// <seealso cref="IDisposable" />
	/// <see cref="ConsoleExtension"/>
	public class AutoRevertConsoleColor : IDisposable
	{

		private readonly ConsoleColor _originalColor;
		private readonly Action<ConsoleColor> _restore;
		private bool disposedValue;

		/// <summary>
		/// Sets and auto-reverts <see cref="Console.ForegroundColor"/>
		/// </summary>
		/// <param name="color">The color to set to the foreground.</param>
		/// <returns></returns>
		public static AutoRevertConsoleColor Foreground(ConsoleColor color) => 
			new(
				color: color,
				getter: () => Console.ForegroundColor,
				setter: (ConsoleColor c) => Console.ForegroundColor = c);

		/// <summary>
		/// Sets and auto-reverts <see cref="Console.BackgroundColor"/>
		/// </summary>
		/// <param name="color">The color to set to the background.</param>
		/// <returns></returns>
		public static AutoRevertConsoleColor Background(ConsoleColor color) => 
			new(
				color: color,
				getter: () => Console.BackgroundColor,
				setter: (ConsoleColor c) => Console.BackgroundColor = c);

		/// <summary>
		/// Initializes a new instance of the <see cref="AutoRevertConsoleColor"/> class.
		/// </summary>
		/// <param name="color">The color.</param>
		/// <param name="getter">The p1.</param>
		/// <param name="setter">The p2.</param>
		public AutoRevertConsoleColor(ConsoleColor color, Func<ConsoleColor> getter, Action<ConsoleColor> setter)
		{
			ArgumentNullException.ThrowIfNull(getter);
			ArgumentNullException.ThrowIfNull(setter);

			_originalColor = getter.Invoke();
			setter.Invoke(color);
			_restore = setter;
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources. Restores color
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
					_restore(_originalColor);

				disposedValue = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}

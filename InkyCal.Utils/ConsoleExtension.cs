using System;

namespace InkyCal.Utils
{
	/// <summary>
	/// A helper class for console methods
	/// </summary>
	public static class ConsoleExtension
	{

		/// <summary>
		/// Sets the foreground color. Color is restored upon disposal
		/// </summary>
		/// <param name="color">The color.</param>
		/// <returns></returns>
		/// <seealso cref="Console.ResetColor"/>
		public static IDisposable TempForegroundColor(this ConsoleColor color) => AutoRevertConsoleColor.Foreground(color);


		/// <summary>
		/// Sets the background color. Color is restored upon disposal
		/// </summary>
		/// <param name="color">The color.</param>
		/// <returns></returns>
		/// <seealso cref="Console.ResetColor"/>
		public static IDisposable TempBackgroundColor(this ConsoleColor color) => AutoRevertConsoleColor.Background(color);

	}
}

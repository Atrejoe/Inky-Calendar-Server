using System;

namespace InkyCal.Utils
{
	/// <summary>
	/// Helper methods for formatting byte sizes in human-readable format
	/// </summary>
	public static class ByteSizeFormatter
	{
		private static readonly string[] SizeUnits = { "B", "KB", "MB", "GB", "TB", "PB" };

		/// <summary>
		/// Formats a byte size into a human-readable string
		/// </summary>
		/// <param name="bytes">The size in bytes</param>
		/// <param name="decimalPlaces">Number of decimal places (default: 2)</param>
		/// <returns>Formatted string like "1.5 MB" or "512 KB"</returns>
		public static string FormatBytes(long bytes, int decimalPlaces = 2)
		{
			if (bytes == 0)
				return "0 B";

			// Handle negative values
			var sign = bytes < 0 ? "-" : "";
			var absoluteBytes = Math.Abs(bytes);

			// Determine the appropriate unit
			var unitIndex = 0;
			var size = (double)absoluteBytes;

			while (size >= 1024 && unitIndex < SizeUnits.Length - 1)
			{
				size /= 1024;
				unitIndex++;
			}

			// Format with appropriate decimal places
			var formattedSize = Math.Round(size, decimalPlaces);
			
			// Remove unnecessary decimal places for whole numbers
#pragma warning disable S1244 // Floating point comparison is intentional for formatting
			var format = Math.Abs(formattedSize % 1) < 0.001 ? "F0" : string.Format("F{0}", decimalPlaces);
#pragma warning restore S1244

			return $"{sign}{formattedSize.ToString(format)} {SizeUnits[unitIndex]}";
		}

		/// <summary>
		/// Formats a byte size into a human-readable string with automatic precision
		/// </summary>
		/// <param name="bytes">The size in bytes</param>
		/// <returns>Formatted string with appropriate precision</returns>
		public static string FormatBytesAuto(long bytes)
		{
			if (bytes == 0)
				return "0 B";

			var absoluteBytes = Math.Abs(bytes);
			var sign = bytes < 0 ? "-" : "";

			// Determine the appropriate unit
			var unitIndex = 0;
			var size = (double)absoluteBytes;

			while (size >= 1024 && unitIndex < SizeUnits.Length - 1)
			{
				size /= 1024;
				unitIndex++;
			}

			// Use more precision for smaller values
#pragma warning disable S3358 // Nested ternary is clearer in this context
			int decimalPlaces;
			if (size < 10)
				decimalPlaces = 2;
			else if (size < 100)
				decimalPlaces = 1;
			else
				decimalPlaces = 0;
#pragma warning restore S3358

			var formattedSize = Math.Round(size, decimalPlaces);
			var format = string.Format("F{0}", decimalPlaces);

			return $"{sign}{formattedSize.ToString(format)} {SizeUnits[unitIndex]}";
		}

		/// <summary>
		/// Parses a human-readable byte size string back to bytes
		/// </summary>
		/// <param name="sizeString">String like "1.5 MB" or "512 KB"</param>
		/// <returns>Size in bytes</returns>
		/// <exception cref="FormatException">If the string format is invalid</exception>
		public static long ParseBytes(string sizeString)
		{
			if (string.IsNullOrWhiteSpace(sizeString))
				throw new ArgumentException("Size string cannot be null or empty", nameof(sizeString));

			var parts = sizeString.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length != 2)
				throw new FormatException($"Invalid size format: {sizeString}. Expected format: '1.5 MB'");

			if (!double.TryParse(parts[0], out var value))
				throw new FormatException($"Invalid numeric value: {parts[0]}");

			var unit = parts[1].ToUpperInvariant();
			var unitIndex = Array.FindIndex(SizeUnits, u => u.Equals(unit, StringComparison.OrdinalIgnoreCase));

			if (unitIndex == -1)
				throw new FormatException($"Invalid unit: {unit}. Valid units: {string.Join(", ", SizeUnits)}");

			var bytes = value * Math.Pow(1024, unitIndex);
			return (long)Math.Round(bytes);
		}
	}
}

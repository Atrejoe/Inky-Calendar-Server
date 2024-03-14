using System;
using System.ComponentModel.DataAnnotations;

namespace InkyCal.Utils
{
	/// <summary>
	/// Helper class for repetetive formatting tasks
	/// </summary>
	public static class FormattingHelper
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="maxLength"></param>
		/// <param name="truncateIndicator"></param>
		/// <returns></returns>
		public static string Limit(this string value, [Range(1, int.MaxValue)] int maxLength, string truncateIndicator = "...")
		{
			if (truncateIndicator?.Length > maxLength)
				throw new ArgumentOutOfRangeException(nameof(truncateIndicator), truncateIndicator, $"The length of the truncation indicator cannot exceed `{nameof(maxLength)}`");

			if (maxLength == 0)
				return string.Empty;

			return (value is null || value.Length <= maxLength)
			? value
			: value[..(maxLength - (truncateIndicator?.Length).GetValueOrDefault())] + truncateIndicator;
		}
	}
}

using System;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Processing;

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
		/// <returns></returns>
		public static string Limit(this string value, int maxLength)
		{
			return value?.Substring(0, Math.Min(value.Length, maxLength));
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="maxLength"></param>
		/// <param name="truncateIndicator"></param>
		/// <returns></returns>
		public static string Limit(this string value, int maxLength, string truncateIndicator = "...")
		{
			if (truncateIndicator?.Length > maxLength)
				throw new ArgumentOutOfRangeException(nameof(truncateIndicator), truncateIndicator, $"The length of the truncation indicator cannot exceed `{nameof(maxLength)}`");

			return (value?.Length).GetValueOrDefault() <= maxLength
				? value
				: value.Substring(0, maxLength - (truncateIndicator?.Length).GetValueOrDefault()) + truncateIndicator;
		}

		/// <summary>
		/// Clones a <see cref="TextGraphicsOptions"/>, allowing changes during cloning.
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		public static TextGraphicsOptions Clone(this TextGraphicsOptions options)
		{
			var result = new TextGraphicsOptions(options.Antialias)
			{
				HorizontalAlignment = options.HorizontalAlignment,
				VerticalAlignment = options.VerticalAlignment,
				WrapTextWidth = options.WrapTextWidth,
				DpiX = options.DpiX,
				DpiY = options.DpiY
			};

			return result;
		}

		/// <summary>
		/// Clones a <see cref="RendererOptions"/>, allowing changes during cloning.
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		public static RendererOptions Clone(this RendererOptions options)
		{
			var result = new RendererOptions(options.Font)
			{
				HorizontalAlignment = options.HorizontalAlignment,
				VerticalAlignment = options.VerticalAlignment,
				WrappingWidth = options.WrappingWidth,
				DpiX = options.DpiX,
				DpiY = options.DpiY
			};

			return result;
		}

		/// <summary>
		/// Converts a <see cref="TextGraphicsOptions"/> to <see cref="RendererOptions"/>.
		/// </summary>
		/// <param name="options"></param>
		/// <param name="enableAntialiasing"></param>
		/// <returns></returns>
		public static TextGraphicsOptions ToTextGraphicsOptions(this RendererOptions options, bool enableAntialiasing)
		{
			var result = new TextGraphicsOptions(enableAntialiasing)
			{
				HorizontalAlignment = options.HorizontalAlignment,
				VerticalAlignment = options.VerticalAlignment,
				WrapTextWidth = options.WrappingWidth,
				DpiX = options.DpiX,
				DpiY = options.DpiY,
			};

			return result;
		}

		/// <summary>
		/// Converts a <see cref="TextGraphicsOptions"/> to <see cref="RendererOptions"/>.
		/// </summary>
		/// <param name="options"></param>
		/// <param name="font"></param>
		/// <returns></returns>
		public static RendererOptions ToRendererOptions(this TextGraphicsOptions options, Font font)
		{
			var result = new RendererOptions(font)
			{
				HorizontalAlignment = options.HorizontalAlignment,
				VerticalAlignment = options.VerticalAlignment,
				WrappingWidth = options.WrapTextWidth,
				DpiX = options.DpiX,
				DpiY = options.DpiY,
			};

			return result;
		}
	}
}

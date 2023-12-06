using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;

namespace InkyCal.Models
{
	/// <summary>
	/// A class for helping to specify color ranges
	/// </summary>
	public static class ColorHelper
	{
		/// <summary>
		/// Returns a range of gray scale colors
		/// </summary>
		/// <param name="levels"></param>
		/// <returns></returns>
		public static IEnumerable<Color> GrayScales([Range(1, byte.MaxValue)] byte levels = 16)
		{
			yield return Color.Black;
			yield return Color.White;

			if (levels <= 2)
				yield break;

			int step = 256 / (levels-1);
			foreach (var color in Enumerable
				.Range(1, levels - 2)
				.Select(x => (step * x) > byte.MaxValue ? byte.MaxValue : (byte)(step * x))
				.Select(x => Color.FromArgb(x, x, x))
				.Distinct())
				yield return color;
		}

		/// <summary>
		/// Returns a range of gray scale colors, with a support color at the third position
		/// </summary>
		/// <param name="levels"></param>
		/// <param name="supportColor"></param>
		/// <returns></returns>
		public static Color[] GrayScalesWithSupportColor(byte levels = 16, Color? supportColor = null)
		{
			var result = GrayScales(levels).ToList();
			result.Insert(2, supportColor ?? Color.Red);
			return result.ToArray();
		}
	}
}

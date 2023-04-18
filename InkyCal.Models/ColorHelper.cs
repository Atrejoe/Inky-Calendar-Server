using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;

namespace InkyCal.Models
{
	/// <summary>
	/// A class for helping to specify colour ranges
	/// </summary>
	public static class ColorHelper
	{
		public static Color[] GrayScales([Range(1, byte.MaxValue)] byte levels = 16)
		{
			if (levels <= 1)
				return new[] { Color.Black };

			int step = 256 / (levels - 1);
			return Enumerable
				.Range(0, levels)
				.Select(x => (step * x) > byte.MaxValue ? byte.MaxValue : (byte)(step * x))
				.Select(x => Color.FromArgb(x, x, x))
				.Distinct()
				.ToArray();
		}

		public static Color[] GrayScalesWithSupportColor(byte levels = 16, Color? suppportColor = null)
		{
			return GrayScales(levels)
				.Append(suppportColor ?? Color.Red)
				.ToArray();
		}
	}
}

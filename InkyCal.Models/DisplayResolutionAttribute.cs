using System;
using System.Drawing;
using System.Linq;

namespace InkyCal.Models
{
	/// <summary>
	/// Attribute for marking up display attributes
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class DisplayResolutionAttribute : Attribute
	{
		/// <summary>
		/// Basic constructor, specifying dimensions
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public DisplayResolutionAttribute(
			ushort width,
			ushort height)
		{
			Width = width;
			Height = height;
		}

		/// <summary>
		/// Overload, allowing specification of a color palette
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="colors"></param>
		public DisplayResolutionAttribute(ushort width, ushort height, Color[] colors) : this(width, height)
			=> Colors = colors;

		/// <summary>
		/// Overload, allowing specification of a color palette
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="colors"></param>
		public DisplayResolutionAttribute(ushort width, ushort height, params KnownColor[] colors) : this(width, height)
			=> Colors = colors.Select(x => Color.FromKnownColor(x)).ToArray();

		/// <summary>
		/// Overload, allowing specification of a grayscale levels
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="grayscales"></param>
		public DisplayResolutionAttribute(ushort width, ushort height, byte grayscales) : this(width, height)
			=> Colors = ColorHelper.GrayScales(grayscales).ToArray();

		/// <summary>
		/// Overload, allowing specification of a grayscale levels, with a support color at the third position
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="grayscales"></param>
		/// <param name="supportColor"></param>
		public DisplayResolutionAttribute(ushort width, ushort height, byte grayscales, KnownColor supportColor) : this(width, height)
			=> Colors = ColorHelper.GrayScalesWithSupportColor(grayscales, Color.FromKnownColor(supportColor));

		/// <summary>
		/// Overload, allowing specification of a blakc-and-white palette, with a support color at the third position
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="supportColor"></param>
		public DisplayResolutionAttribute(ushort width, ushort height, KnownColor supportColor) : this(width, height)
			=> Colors = ColorHelper.GrayScalesWithSupportColor(2, Color.FromKnownColor(supportColor));

		/// <summary>
		/// The width of the display, in pixels
		/// </summary>
		public ushort Width { get; }
		
		/// <summary>
		/// The height of the display, in pixels
		/// </summary>
		public ushort Height { get; }

		/// <summary>
		/// The color palette for the display
		/// </summary>
		public Color[] Colors { get; } = ColorHelper.GrayScales(2).ToArray();
	}
}

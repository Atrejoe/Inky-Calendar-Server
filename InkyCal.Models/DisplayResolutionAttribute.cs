using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace InkyCal.Models
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class DisplayResolutionAttribute : Attribute
	{
		public DisplayResolutionAttribute(
			[Range(1, ushort.MaxValue)] ushort width,
			[Range(1, ushort.MaxValue)] ushort height)
		{
			Width = width;
			Height = height;
		}

		public DisplayResolutionAttribute(ushort width, ushort height, Color[] colors) : this(width, height)
		{
			Colors = colors;
		}

		public DisplayResolutionAttribute(ushort width, ushort height, byte grayscales) : this(width, height)
		{
			Colors = ColorHelper.GrayScales(grayscales);
		}

		public DisplayResolutionAttribute(ushort width, ushort height, byte grayscales, KnownColor supportColor) : this(width, height)
		{
			Colors = ColorHelper.GrayScalesWithSupportColor(grayscales, Color.FromKnownColor(supportColor));
		}

		public DisplayResolutionAttribute(ushort width, ushort height, KnownColor supportColor) : this(width, height)
		{
			Colors = ColorHelper.GrayScalesWithSupportColor(2, Color.FromKnownColor(supportColor));
		}

		public ushort Width { get; }
		public ushort Height { get; }
		public Color[] Colors { get; } = ColorHelper.GrayScales(2);
	}
}

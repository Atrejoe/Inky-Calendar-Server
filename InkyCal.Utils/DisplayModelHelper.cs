using System;
using System.Collections.ObjectModel;
using System.Linq;
using InkyCal.Models;
using SixLabors.ImageSharp;

namespace InkyCal.Utils
{

	/// <summary>
	/// A helper class for obtaining display model properties
	/// </summary>
	public static class DisplayModelHelper
	{

		/// <summary>
		/// Gets the specs for the <paramref name="model"/>.
		/// </summary>
		/// <param name="model"></param>
		public static DisplayModelSpecs? GetSpecs(this DisplayModel? model)
		{
			if (!model.HasValue)
				return null;

			model.Value.GetSpecs(out var width, out var height, out var colors);

			return new DisplayModelSpecs()
			{
				Width = width,
				Height = height,
				Colors = new ReadOnlyCollection<Color>(colors)
			};
		}

		/// <summary>
		/// Gets the specs for the <paramref name="model"/>.
		/// </summary>
		/// <param name="model"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="colors"></param>
		public static void GetSpecs(this DisplayModel model, out int width, out int height, out Color[] colors)
		{

			var attr = model.GetAttribute<DisplayResolutionAttribute>()
				?? throw new ArgumentOutOfRangeException(nameof(model), model, $"Model `{model}` is not supported, dimensions & colors unknown.");

			width = attr.Width;
			height = attr.Height;
			colors = attr.Colors.Select(x => Color.FromRgba(r: x.R, g: x.G, b: x.B, a: x.A)).ToArray();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static T GetAttribute<T>(this Enum value) where T : Attribute
		{
			var type = value.GetType();
			var memberInfo = type.GetMember(value.ToString());
			var attributes = memberInfo[0].GetCustomAttributes(typeof(T), false);
			return attributes.Length > 0
			  ? (T)attributes[0]
			  : null;
		}
	}
}

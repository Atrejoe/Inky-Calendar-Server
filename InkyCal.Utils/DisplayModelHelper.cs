using InkyCal.Models;
using SixLabors.ImageSharp;
using System;

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
				Colors = colors
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

			switch (model)
			{
				case DisplayModel.epd_7_in_5_v3_colour:
				case DisplayModel.epd_7_in_5_v3:
					width = 880;
					height = 528;
					break;
				case DisplayModel.epd_7_in_5_v2_colour:
				case DisplayModel.epd_7_in_5_v2:
					width = 800;
					height = 480;
					break;
				case DisplayModel.epd_7_in_5_colour:
				case DisplayModel.epd_7_in_5:
					width = 640;
					height = 384;
					break;
				case DisplayModel.epd_5_in_83_colour:
				case DisplayModel.epd_5_in_83:
					width = 600;
					height = 448;
					break;
				case DisplayModel.epd_4_in_2_colour:
				case DisplayModel.epd_4_in_2:
					width = 400;
					height = 300;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(model), model, $"Model `{model}` is not supported, dimensions unknown.");
			}

			switch (model)
			{
				case DisplayModel.epd_7_in_5_v3_colour:
				case DisplayModel.epd_7_in_5_v2_colour:
				case DisplayModel.epd_7_in_5_colour:
					colors = new[] { Color.Black, Color.White, Color.Red }; //Could be yellow too, maybe introduce a new panel type
					break;
				case DisplayModel.epd_7_in_5_v3:
				case DisplayModel.epd_7_in_5_v2:
				case DisplayModel.epd_7_in_5:
					colors = new[] { Color.Black, Color.White };
					break;
				case DisplayModel.epd_5_in_83_colour:
				case DisplayModel.epd_4_in_2_colour:
					colors = new[] { Color.Black, Color.White, Color.Red };
					break;
				case DisplayModel.epd_5_in_83:
				case DisplayModel.epd_4_in_2:
					colors = new[] { Color.Black, Color.White };
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(model), model, $"Model `{model}` is not supported, colors unknown.");
			}
		}
	}
}

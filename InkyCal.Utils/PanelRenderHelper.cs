using InkyCal.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Linq;

namespace InkyCal.Utils
{
	/// <summary>
	/// A helper class for mapping a <see cref="Panel"/> to a <see cref="IPanelRenderer"/>.
	/// </summary>
	public static class PanelRenderHelper
	{

		/// <summary>
		/// Gets a <see cref="IPanelRenderer"/> for the spcified <paramref name="panel"/>.
		/// </summary>
		/// <param name="panel"></param>
		/// <returns></returns>
		public static IPanelRenderer GetRenderer(this Models.Panel panel)
		{
			IPanelRenderer renderer;

			switch (panel)
			{
				case CalendarPanel cp:

					var urls = cp.CalenderUrls.Select(x => new Uri(x.Url));
					renderer = new CalendarPanelRenderer(iCalUrls: urls.ToArray());

					break;
				case ImagePanel ip:

					var imageRotation = (RotateMode)(((int)ip.ImageRotation - (int)panel.Rotation + 360) % 360);
					renderer = new ImagePanelRenderer(new Uri(ip.Path), imageRotation);
					break;

				case PanelOfPanels pp:
					renderer = new PanelOfPanelRenderer(pp);
					break;

				case WeatherPanel wp:
					renderer = new WeatherPanelRenderer(wp);
					break;

				default:

					throw new NotImplementedException($"Rendering of {panel.GetType().Name} has not yet been implemented");
			}

			return renderer;
		}

		/// <summary>
		/// Returns meaningful colors for drawing purposes
		/// </summary>
		/// <param name="colors"></param>
		/// <param name="primaryColor"></param>
		/// <param name="supportColor"></param>
		/// <param name="errorColor"></param>
		/// <param name="backgroundColor"></param>
		public static void ExtractMeaningFullColors(
			this Color[] colors,
			out Color primaryColor,
			out Color supportColor,
			out Color errorColor,
			out Color backgroundColor)
		{

			primaryColor = colors.FirstOrDefault();
			supportColor = (colors.Count() > 2) ? colors[2] : primaryColor;
			errorColor = supportColor;
			backgroundColor = colors.Skip(1).FirstOrDefault();
		}

		/// <summary>
		/// Creates an image, caller needs to dispose
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="backgroundColor"></param>
		/// <returns></returns>
		public static Image CreateImage(int width, int height, Color? backgroundColor = null)
		{
			return new Image<Rgba32>(new Configuration() { }, width, height, backgroundColor.GetValueOrDefault(Color.White));
		}

		internal static void RenderErrorMessage(
			this IImageProcessingContext canvas,
			string errorMessage,
			Color errorColor,
			Color backgroundColor,
			ref int y,
			int width,
			Font font)
		{
			var rendererOptions = new TextGraphicsOptions(false)
			{
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				WrapTextWidth = width,
				DpiX = 96,
				DpiY = 96
			}.ToRendererOptions(font);

			canvas.RenderErrorMessage(errorMessage, errorColor, backgroundColor, ref y, width, rendererOptions);
		}

		internal static void RenderErrorMessage(
		this IImageProcessingContext canvas,
		string errorMessage,
		Color errorColor,
		Color backgroundColor,
		ref int y,
		int width,
		RendererOptions renderOptions)
		{

			renderOptions.WrappingWidth = width - 4;
			var textDrawOptions_Error = renderOptions.ToTextGraphicsOptions(false);

			var errorMessageHeight = TextMeasurer.MeasureBounds(errorMessage, renderOptions);

			errorMessageHeight.Width = width;
			errorMessageHeight.Height += 4; //Pad 2 px on all sides

			canvas.Fill(errorColor, errorMessageHeight);

			var pError = new PointF(2, 2);//Adhere to padding
			canvas.DrawText(textDrawOptions_Error, errorMessage, renderOptions.Font, backgroundColor, pError);

			y += (int)errorMessageHeight.Height;
		}
	}
}

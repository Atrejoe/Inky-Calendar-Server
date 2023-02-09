﻿using System;
using System.IO;
using System.Threading.Tasks;
using InkyCal.Models;
using InkyCal.Utils;
using Microsoft.AspNetCore.Mvc;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Processing;

namespace InkyCal.Server.Controllers
{
	/// <summary>
	/// A helper class for converting images to <see cref="ActionResult"/>s.
	/// </summary>
	internal static class ImageExtensions
	{

		/// <summary>
		/// Returns a <see cref="FileContentResult"/> from an image, generated by <see cref="IPanelRenderer.GetImage"/>
		/// </summary>
		/// <param name="controller"></param>
		/// <param name="image"></param>
		/// <returns></returns>
		public static ActionResult Image(this ControllerBase controller, Image image)
		{
			if (controller is null)
				throw new ArgumentNullException(nameof(controller));
			if (image is null)
				throw new ArgumentNullException(nameof(image));


			using var stream = new MemoryStream();
			image.SaveAsGif(stream, new GifEncoder() { ColorTableMode = GifColorTableMode.Global });
			return controller.File(
				fileContents: stream.ToArray(),
				contentType: "image/gif");
		}

		/// <summary>
		/// Returns a <see cref="FileContentResult"/> from a <see cref="IPanelRenderer"/> for the specified <paramref name="model"/>.
		/// </summary>
		/// <param name="controller"></param>
		/// <param name="renderer"></param>
		/// <param name="model"></param>
		/// <param name="requestedWidth"></param>
		/// <param name="requestedHeight"></param>
		/// <param name="rotateMode"></param>
		/// <returns></returns>
		public static async Task<ActionResult> Image(this ControllerBase controller, IPanelRenderer renderer, DisplayModel model, int? requestedWidth = null, int? requestedHeight = null,
			RotateMode rotateMode = RotateMode.None)
		{
			model.GetSpecs(out var defaultWidth, out var defaultHeight, out var colors);

			var flip = false;

			int width, height;


			switch (rotateMode)
			{
				case RotateMode.Rotate270:
				case RotateMode.Rotate90:
					height = requestedWidth ?? defaultWidth;
					width = requestedHeight ?? defaultHeight;
					break;
				default:
					width = requestedWidth ?? defaultWidth;
					height = requestedHeight ?? defaultHeight;
					break;
			}

			switch (rotateMode)
			{
				case RotateMode.Rotate270:
				case RotateMode.Rotate180:
					flip = true;
					break;
			}

			//NB: Web-based requests will specify portrait oriented dimensions.
			//    GetSpecs get landscape-oriented dimensions
			//    Therefore flip-em!
			return await controller.Image(renderer, width, height, colors, flip);
		}


		/// <summary>
		/// Returns a <see cref="FileContentResult"/> from <see cref="IPanelRenderer.GetImage"/> with specific dimensions and color space.
		/// </summary>
		/// <param name="controller"></param>
		/// <param name="panelRenderer"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="colors"></param>
		/// <param name="flip"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
		public static async Task<ActionResult> Image(
			this ControllerBase controller,
			IPanelRenderer panelRenderer,
			int width,
			int height,
			Color[] colors,
			bool flip)
		{
			if (panelRenderer is null)
				throw new ArgumentNullException(nameof(panelRenderer));

			try
			{
				var bytes = await panelRenderer.GetCachedImage(width, height, colors, log: controller.ConditionalLog);

				if (flip)
				{
					using var image = SixLabors.ImageSharp.Image.Load(bytes);
					image.Mutate(x => x.Rotate(RotateMode.Rotate180));
					return controller.Image(image);
				}
				else
					return controller.Gif(bytes);
			}
			catch (Exception ex)
			{

				ex.Data["PanelRendererType"] = panelRenderer.GetType().Name;
				ex.Log();

				colors.ExtractMeaningFullColors(
					 out var primaryColor
					, out var supportColor
					, out var errorColor
					, out var backgroundColor
					);

				using var image = PanelRenderingHelper.CreateImage(width, height, backgroundColor);
				image.Mutate(x =>
					{
						var messageFont = FontHelper.NotoSans.CreateFont(16);
						var errorTextOptions =
						new TextGraphicsOptions(
							new GraphicsOptions() { Antialias = false },
							new TextOptions() { WrapTextWidth = width }
							);
						var errorMessageRenderOptions = errorTextOptions.ToRendererOptions(messageFont);
						var errorMessage = ex.Message.ToSafeChars(FontHelper.NotoSans).Trim();

						x.DrawText(
							errorTextOptions,
							errorMessage,
							messageFont,
							errorColor, new Point(0, 0));

						var start = (int)TextMeasurer.MeasureBounds(errorMessage, errorMessageRenderOptions).Height
						+ 10;

						x.DrawText(
							errorTextOptions,
							ex.StackTrace,
							FontHelper.NotoSans.CreateFont(14),
							primaryColor,
							new Point(0, start));
					});
				return controller.Image(image);
			}
		}

		/// <summary>
		/// Returns a <see cref="FileContentResult"/> from , generated by <see cref="IPanelRenderer.GetImage"/>
		/// </summary>
		/// <param name="controller"></param>
		/// <param name="image"></param>
		/// <returns></returns>
		public static ActionResult Gif(this ControllerBase controller, byte[] image)
		{
			if (controller is null)
				throw new ArgumentNullException(nameof(controller));
			if (image is null)
				throw new ArgumentNullException(nameof(image));

			return controller.File(
				fileContents: image,
				contentType: "image/gif");
		}

		private static async Task ConditionalLog(this ControllerBase controller, Exception ex, bool handled = false, string explanation = null)
		{
			if (handled)
				Console.WriteLine($"{explanation ?? "Handled exception"}: {ex.Message}");
			else
			{
				var user = await controller.GetAuthenticatedUser();
				ex.Log(user);
			}
		}
	}
}

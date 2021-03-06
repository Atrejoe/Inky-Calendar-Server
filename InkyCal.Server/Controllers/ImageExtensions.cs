﻿using System;
using System.IO;
using System.Threading.Tasks;
using InkyCal.Models;
using InkyCal.Utils;
using Microsoft.AspNetCore.Mvc;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace InkyCal.Server.Controllers
{
	/// <summary>
	/// A helper class for converting images to <see cref="ActionResult"/>s.
	/// </summary>
	public static class ImageExtensions
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
			model.GetSpecs(out var width, out var height, out var colors);

			//NB: Web-based requests will specify portrait oriented dimensions.
			//    GetSpecs get landscape-oriented dimensions
			//    Therefore flip-em!
			return await controller.Image(renderer, requestedWidth ?? height, requestedHeight ?? width, colors, rotateMode);
		}


		/// <summary>
		/// Returns a <see cref="FileContentResult"/> from <see cref="IPanelRenderer.GetImage"/> with specific dimensions and color space.
		/// </summary>
		/// <param name="controller"></param>
		/// <param name="panelRenderer"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="colors"></param>
		/// <param name="rotateMode"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
		public static async Task<ActionResult> Image(
			this ControllerBase controller,
			IPanelRenderer panelRenderer,
			int width,
			int height,
			Color[] colors,
			RotateMode rotateMode)
		{
			if (panelRenderer is null)
				throw new ArgumentNullException(nameof(panelRenderer));


			IPanelRenderer.Log conditionalLog = async (Exception ex, bool handled, string explanation) =>
			{
				if (handled)
					Console.WriteLine($"{explanation ?? "Handled exception"}: {ex.Message}");
				else
				{
					var user = await controller.GetAuthenticatedUser();
					ex.Log(user);
				}
			};

			try
			{
				var bytes = await panelRenderer.GetCachedImage(width, height, colors, conditionalLog);
				using var image = SixLabors.ImageSharp.Image.Load(bytes);
				image.Mutate(x => x.Rotate(rotateMode));
				return controller.Image(image);
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

				using var image = PanelRenderHelper.CreateImage(width, height, backgroundColor);
				image.Mutate(x =>
					{
						x.DrawText(new TextGraphicsOptions(true) { WrapTextWidth = width }, ex.Message, new Font(FontHelper.NotoSans, 16), errorColor, new Point(0, 0));
					});
				return controller.Image(image);
			}
		}
	}
}

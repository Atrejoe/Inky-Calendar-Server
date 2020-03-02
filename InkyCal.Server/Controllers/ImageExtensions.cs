﻿using InkyCal.Models;
using InkyCal.Utils;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading.Tasks;

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
		/// <param name="panel"></param>
		/// <param name="model"></param>
		/// <param name="requestedWidth"></param>
		/// <param name="requestedHeight"></param>
		/// <param name="rotateMode"></param>
		/// <returns></returns>
		public static async Task<ActionResult> Image(this ControllerBase controller, IPanelRenderer panel, DisplayModel model, int? requestedWidth = null, int? requestedHeight = null, 
			RotateMode rotateMode = RotateMode.None)
		{
			model.GetSpecs(out var width, out var height, out var colors);

			//NB: Web-based requests will specify portrait oriented dimensions.
			//    GetSpecs get landscape-oriented dimensions
			//    Therefore flip-em!
			return await controller.Image(panel, requestedWidth ?? height, requestedHeight ?? width, colors, rotateMode);
		}

		/// <summary>
		/// Returns a <see cref="FileContentResult"/> from <see cref="IPanelRenderer.GetImage"/> with specific dimensions and color space.
		/// </summary>
		/// <param name="controller"></param>
		/// <param name="panel"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="colors"></param>
		/// <param name="rotateMode"></param>
		/// <returns></returns>
		public static async Task<ActionResult> Image(
			this ControllerBase controller,
			IPanelRenderer panel,
			int width,
			int height,
			Color[] colors,
			RotateMode rotateMode)
		{
			using var image = await panel.GetImage(width, height, colors);
			image.Mutate(x => x.Rotate(rotateMode));
			return controller.Image(image);
		}
	}
}

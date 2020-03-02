using InkyCal.Models;
using SixLabors.ImageSharp.Processing;
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

				default:

					throw new NotImplementedException($"Rendering of {panel.GetType().Name} has not yet been implemented");
			}

			return renderer;
		}
	}
}

using InkyCal.Models;
using InkyCal.Utils;
using Microsoft.AspNetCore.Components;

namespace InkyCal.Server.Pages
{
	/// <summary>
	/// Displays an image with a "loading" overlay
	/// </summary>
	/// <seealso cref="ComponentBase" />
	public partial class Index : ComponentBase
	{
		private DisplayModel model;

		private DisplayModelSpecs specs => model.GetSpecs();

		private int modelWidth
		{
			get
			{
				model.GetSpecs(out var width, out var _, out var _);
				return width;
			}
		}
		private int modelHeight
		{
			get
			{
				model.GetSpecs(out var _, out var height, out var _);
				return height;
			}
		}
		//private int modelColors
		//{
		//    get
		//    {
		//        model.GetSpecs(out var _, out var _, out var colors);
		//        return colors.Length;
		//    }
		//}

		

		string panelStyle
		{
			get
			{
				return $"border:1px solid silver; width:{modelWidth}px; height:{modelHeight}px";
			}
		}
	}
}

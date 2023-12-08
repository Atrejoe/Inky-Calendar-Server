using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;

namespace InkyCal.Server.Pages
{
	/// <summary>
	/// Displays an image with a "loading" overlay
	/// </summary>
	/// <seealso cref="ComponentBase" />
	public partial class ImageWithProgress : ComponentBase
	{
		/// <summary>
		/// The src of the image
		/// </summary>
		/// <value>
		/// The image url.
		/// </value>
		[Parameter, Url]
		[SuppressMessage("Usage", "BL0007:Component parameters should be auto properties", Justification = "<Pending>")]
		public string src
		{
			get => _src;
			set
			{
				if (!string.Equals(_src, value))
					SetAsLoading();
				_src = value;
			}
		}

		/// <summary>
		/// The style of the display component, is passed to the wrapper of the image.
		/// </summary>
		[Parameter]
		public string style {get;set;}

		private string LoadingCSS = "loading";
		private string _src;

		private void SetAsLoading()
		{
			LoadingCSS = "loading";
		}

		private void HandleOnLoad()
		{
			LoadingCSS = string.Empty;
		}
	}
}

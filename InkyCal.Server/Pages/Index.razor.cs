using System;
using System.Diagnostics.CodeAnalysis;
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
		private DisplayModel model1;

		/// <summary>
		/// The selected display panel
		/// </summary>
		[Parameter]
		[SuppressMessage("Usage", "BL0007:Component parameters should be auto properties", Justification = "<Pending>")]
		public string modelAsString
		{
			get => model.ToString();
			set
			{
				if (Enum.TryParse<DisplayModel>(value, ignoreCase: true, out var parsedModel))
					model = parsedModel;
			}
		}

		[Inject]
		private NavigationManager navigationManager { get; set; }

		/// <summary>
		/// The selected display panel
		/// </summary>
		[Parameter]
		[SuppressMessage("Usage", "BL0007:Component parameters should be auto properties", Justification = "<Pending>")]
		public DisplayModel model
		{
			get => model1;
			set
			{
				model1 = value;
				UpdateRoute();
			}
		}

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

		string panelStyle => $"border:1px solid silver; width:{modelWidth}px; height:{modelHeight}px";

		private bool _initialized;

		/// <summary>
		/// 
		/// </summary>
		protected override void OnInitialized()
		{
			base.OnInitialized();
			_initialized = true;
		}
		internal void UpdateRoute()
		{
			// I'm a noob with navigation, this prevents navigation upon deeplinking
			if (_initialized)
				navigationManager.NavigateTo($"/demo/{model}#demo");
		}

		//void UpdateRoute()
		//{
		//	// You can also change it to any url you want
		//	navigationManager.NavigateTo($"/{model}/");
		//}
	}
}

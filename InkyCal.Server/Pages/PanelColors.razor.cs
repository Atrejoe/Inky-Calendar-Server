using InkyCal.Utils;
using Microsoft.AspNetCore.Components;

namespace InkyCal.Server.Pages
{
	/// <summary>
	/// Display colorss for a panel
	/// </summary>
	/// <seealso cref="ComponentBase" />
	public partial class PanelColors : ComponentBase
	{
		/// <summary>
		/// The display specifications
		/// </summary>
		[Parameter]
		public DisplayModelSpecs Specs { get; set; }
	}
}

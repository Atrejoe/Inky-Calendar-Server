using System.Threading.Tasks;
using InkyCal.Data;
using Microsoft.AspNetCore.Components;

namespace InkyCal.Server.Pages
{
	/// <summary>
	/// 
	/// </summary>
	public partial class PanelStar : AuthenticatedComponentBase
	{
		/// <summary>
		/// The panel to edit
		/// </summary>
		[Parameter]
		public Models.Panel Panel { get; set; }

		async Task Toggle()
		{
			if (Panel == null)
				return;

			this.Panel.Starred = await PanelRepository.ToggleStar(Panel);

			StateHasChanged();
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InkyCal.Models;
using InkyCal.Utils.NewPaperRenderer.FreedomForum.Models;
using Microsoft.AspNetCore.Components;

namespace InkyCal.Server.Pages
{
	/// <summary>
	/// Displays a calendar panel
	/// </summary>
	/// <seealso cref="ComponentBase" />
	public partial class NewspaperPanel : AuthenticatedComponentBase
	{
		private CalendarPanelUrl newUrl { get; set; } = new CalendarPanelUrl() { };

		/// <summary>
		/// Gets or sets the selected panel.
		/// </summary>
		/// <value>
		/// The panel.
		/// </value>
		[Parameter]
		public Models.NewsPaperPanel Panel { get; set; }

		/// <summary>
		/// Gets or sets the navigation manager.
		/// </summary>
		/// <value>
		/// The navigation manager.
		/// </value>
		[Inject]
		public NavigationManager NavigationManager { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public IGrouping<string, NewsPaper>[] NewsPapers { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		protected override async Task OnInitializedAsync()
		{
#pragma warning disable S1481 // Unused local variables should be removed
			var user = await base.GetAuthenticatedUser();
#pragma warning restore S1481 // Unused local variables should be removed
			NewsPapers = (await new Utils.NewPaperRenderer.FreedomForum.ApiClient().GetNewsPapers()).Values.GroupBy(x => x.Country).ToArray();
		}
	}
}

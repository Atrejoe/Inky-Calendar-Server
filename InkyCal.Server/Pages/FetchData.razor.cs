using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InkyCal.Data;
using Microsoft.AspNetCore.Components;

namespace InkyCal.Server.Pages
{

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Microsoft.AspNetCore.Components.ComponentBase" />
	public partial class FetchData : AuthenticatedComponentBase
	{
		private List<Models.Panel> _panels;
		private Models.Panel _selectedPanel;
		internal bool _selectedPanelLoading = false;

		/// <summary>
		/// Gets or sets the navigation manager.
		/// </summary>
		/// <value>
		/// The navigation manager.
		/// </value>
		[Inject]
		public NavigationManager navigationManager { get; set; }

		/// <summary>
		/// Method invoked when the component is ready to start, having received its
		/// initial parameters from its parent in the render tree.
		/// Override this method if you will perform an asynchronous operation and
		/// want the component to refresh when that operation is completed.
		/// </summary>
		protected override async Task OnInitializedAsync()
		{
			var user = await GetAuthenticatedUser();

			if (user is null)
				return;
			
			_panels = (await PanelRepository.List<Models.Panel>(user)).ToList();

			if (_panels.Any())
				await SelectPanel(_panels.First().Id);
		}

		/// <summary>
		/// Selects the panel.
		/// </summary>
		/// <param name="id">The identifier.</param>
		protected async Task SelectPanel(Guid id)
		{
			try
			{
				_selectedPanelLoading = true;
				_selectedPanel = await PanelRepository.Get<Models.Panel>(id: id);
			}
			finally
			{
				_selectedPanelLoading = false;
			}
		}
		private async Task Delete(Guid id)
		{
			var user = await GetAuthenticatedUser();
			await PanelRepository.Delete(id, user);
			_panels.RemoveAll(x => x.Id == id);
		}
	}
}

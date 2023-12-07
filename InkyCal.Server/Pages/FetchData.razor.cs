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
	/// <seealso cref="ComponentBase" />
	public partial class FetchData : AuthenticatedComponentBase
	{
		private List<Models.Panel> _panels;
		private Models.Panel _selectedPanel;
		internal bool _selectedPanelLoading;

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
				await SelectPanel(_panels[0].Id);
		}

		/// <summary>
		/// Selects the panel.
		/// </summary>
		/// <param name="id">The identifier.</param>
		protected async Task SelectPanel(Guid id)
		{
			if ((_selectedPanel?.Id).Equals(id))
				return;

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

		private Guid CacheBreaker = Guid.NewGuid();
	}
}

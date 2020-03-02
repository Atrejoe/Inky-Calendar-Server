using System;
using System.Threading.Tasks;
using InkyCal.Models;
using InkyCal.Data;
using InkyCal.Utils;
using Microsoft.AspNetCore.Components;

namespace InkyCal.Server.Pages
{
	/// <summary>
	/// An editor UI for creating a <see cref="Models.Panel"/>
	/// </summary>
	/// <seealso cref="Microsoft.AspNetCore.Components.ComponentBase" />
	public partial class Panel : AuthenticatedComponentBase
	{
		/// <summary>
		/// Gets or sets the panel identifier. When not set, this is an edit page.
		/// </summary>
		/// <value>
		/// The panel identifier.
		/// </value>
		[Parameter]
		public Guid? PanelId { get; set; }

		private Guid CacheBreaker = Guid.NewGuid();

		[Inject]
		private NavigationManager navigationManager { get; set; }

		private Models.Panel _Panel;

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

			if (PanelId.HasValue)
				_Panel = await PanelRepository.Get<Models.Panel>(PanelId.Value, user);
			else
				InitPanel<CalendarPanel>();

		}
		private void InitPanelByType(Type type)
		{
			var method = GetType().GetMethod(nameof(InitPanel));
			var genericMethod = method.MakeGenericMethod(type);
			genericMethod.Invoke(this, null); // no arguments
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TPanel"></typeparam>
		public void InitPanel<TPanel>() where TPanel : Models.Panel, new()
		{
			_Panel = new TPanel()
			{
				Model = (_Panel?.Model).GetValueOrDefault(),
				Height = _Panel?.Height,
				Width = _Panel?.Width,
				Rotation = (_Panel?.Rotation).GetValueOrDefault(Rotation.CounterClockwise)
			};
		}

		private DisplayModelSpecs Specs
		{
			get
			{
				var model = (DisplayModel?)(int?)_Panel?.Model;
				return model.GetSpecs().GetValueOrDefault();
			}
		}

		private async Task HandleValidSubmit()
		{
			Console.WriteLine("OnValidSubmit");

			_Panel.Owner = await GetAuthenticatedUser();
			await _Panel.Update();

			CacheBreaker = Guid.NewGuid();

			navigationManager.NavigateTo($"/panel/{_Panel.Id}/edit");
		}

		private async Task Delete(bool confirm)
		{
			if (!confirm)
				return;

			var user = await GetAuthenticatedUser();
			await PanelRepository.Delete(_Panel.Id, user);

			navigationManager.NavigateTo($"/fetchdata");
		}
	}
}

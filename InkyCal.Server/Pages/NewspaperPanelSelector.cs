using System.Linq;
using System.Threading.Tasks;
using InkyCal.Utils.NewPaperRenderer.FreedomForum.Models;
using Microsoft.AspNetCore.Components;

namespace InkyCal.Server.Pages
{
	/// <summary>
	/// Displays a calendar panel
	/// </summary>
	/// <seealso cref="ComponentBase" />
	public partial class NewspaperPanelSelector
	{
		/// <summary>
		/// 
		/// </summary>
		[Parameter]
		public string NewsPaperId { get; set; } 

		/// <summary>
		/// 
		/// </summary>
		[Parameter]
		public EventCallback<string> NewsPaperIdChanged { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public IGrouping<string, NewsPaper>[] NewsPapers { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		protected override async Task OnInitializedAsync()
		{
			NewsPapers = (await new Utils.NewPaperRenderer.FreedomForum.ApiClient().GetNewsPapers()).Values.GroupBy(x => x.Country).ToArray();
		}
        private void SelectionChanged(System.EventArgs e)
        {
			NewsPaperIdChanged.InvokeAsync(NewsPaperId).Wait();
		}
	}
}

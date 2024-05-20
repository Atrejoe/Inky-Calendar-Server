using System.Linq;
using System.Threading.Tasks;
using InkyCal.Utils.NewPaperRenderer.FreedomForum.Models;
using Microsoft.AspNetCore.Components;

namespace InkyCal.Server.Pages
{
	/// <summary>
	/// Newspaper selector component for choosing from available newspapers
	/// </summary>
	/// <seealso cref="ComponentBase" />
	public partial class NewspaperPanelSelector : ComponentBase
	{
		/// <summary>
		/// Gets or sets the selected newspaper ID
		/// </summary>
		[Parameter]
		public string NewsPaperId { get => field; set {
				field = value;

				NewsPaperIdChanged.InvokeAsync(field).Wait();
			}
		}

		/// <summary>
		/// Event callback invoked when the newspaper ID changes
		/// </summary>
		[Parameter]
		public EventCallback<string> NewsPaperIdChanged { get; set; }

		/// <summary>
		/// Gets the available newspapers grouped by country
		/// </summary>
		public IGrouping<string, NewsPaper>[] NewsPapers { get; private set; }

		/// <summary>
		/// Loads the available newspapers on component initialization
		/// </summary>
		protected override async Task OnInitializedAsync()
		{
			NewsPapers = (await new Utils.NewPaperRenderer.FreedomForum.ApiClient().GetNewsPapers())
				.Values
				.GroupBy(x => x.Country)
				.ToArray();

			if (string.IsNullOrEmpty(NewsPaperId) && NewsPapers.Length != 0)
				NewsPaperId = NewsPapers.FirstOrDefault().First().PaperId;

		}
	}
}

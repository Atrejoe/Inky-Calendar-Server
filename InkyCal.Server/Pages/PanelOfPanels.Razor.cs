using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using InkyCal.Models;
using InkyCal.Data;
using Microsoft.AspNetCore.Components;

namespace InkyCal.Server.Pages
{
	/// <summary>
	/// 
	/// </summary>
	public partial class PanelOfPanels : AuthenticatedComponentBase
	{
		[Required]
		private string newPanelIdAsString
		{
			get
			{
				return newPanelId?.ToString();
			}
			set
			{
				if (Guid.TryParse(value, out var success))
					newPanelId = success;
				else
					newPanelId = null;
			}
		}

		[Required]
		private Guid? newPanelId { get; set; }

		private List<Models.Panel> selectablePanels { get; set; }

		/// <summary>
		/// The panel to edit
		/// </summary>
		[Parameter]
		public Models.PanelOfPanels Panel { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected async override Task OnInitializedAsync()
		{
			var user = await GetAuthenticatedUser();
			selectablePanels = (await PanelRepository.List<Models.Panel>(user))
								.Where(x=>!(x is Models.PanelOfPanels))
								.ToList();

			newPanelId = selectablePanels.FirstOrDefault()?.Id;
		}

		private void AddPanel()
		{
			if (!newPanelId.HasValue)
				return;
			if(Panel.Panels is null)
				Panel.Panels = new HashSet<SubPanel>();

			Panel.Panels.Add(new SubPanel()
			{
				IdParent = Panel.Id,
				IdPanel = newPanelId.Value,
				Panel = selectablePanels.Single(x => x.Id == newPanelId.Value),//For display purposes
				SortIndex = Panel.Panels.Any() ? (byte)(Panel.Panels.Max(x => x.SortIndex) + 1) : (byte)1
			});
			newPanelId = selectablePanels.FirstOrDefault()?.Id;
		}

		private void RemovePanel(byte index)
		{
			Console.WriteLine($"Removing by index {index}");
			//Urls are case-sensitive
			Panel.Panels.RemoveWhere(x => x.SortIndex.Equals(index));

			StateHasChanged();
		}
		private void RemovePanel()
		{
			Console.WriteLine($"Removing by index {currentIndex}");
			var victim = Panel.Panels.OrderBy(x => x.SortIndex).ToList()[currentIndex];
			//Urls are case-sensitive
			Panel.Panels.Remove(victim);

			StateHasChanged();
		}

		private int currentIndex;

		void StartDrag(SubPanel item)
		{
			currentIndex = Panel.Panels.OrderBy(x=>x.SortIndex).ToList().IndexOf(item);
			Console.WriteLine($"DragStart for {item.Panel.Name} index {currentIndex}");
		}

		void Drop(SubPanel item)
		{
			if (item == null)
				return;
			
			var intermediate = Panel.Panels.OrderBy(x => x.SortIndex).ToList();
			var index = intermediate.IndexOf(item);

			Panel.Panels.Clear();

			var current = intermediate[currentIndex];
			intermediate.RemoveAt(currentIndex);
			intermediate.Insert(index, current);

			for (byte i = 0; i < intermediate.Count; i++)
				intermediate[i].SortIndex = i;

			Panel.Panels = new HashSet<SubPanel>(intermediate.OrderBy(x => x.SortIndex));

			// update current selection
			currentIndex = index;

			StateHasChanged();
		}
	}
}

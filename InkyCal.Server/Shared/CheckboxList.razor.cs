using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace InkyCal.Server.Shared
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TItem"></typeparam>
	public partial class CheckboxList<TItem> : ComponentBase
	{

		/// <summary>
		/// Data for the Checkbox   
		/// </summary>
		[Parameter] public IEnumerable<TItem> Data { get; set; }

		/// <summary>
		/// The field to be shown adjacent to checkbox  
		/// </summary>
		[Parameter] public Func<TItem, string> TextField { get; set; }

		/// <summary>
		/// The Value which checkbox will return when checked   
		/// </summary>
		[Parameter] public Func<TItem, string> ValueField { get; set; }


		/// <summary>
		/// The Value which checkbox will return when checked   
		/// </summary>
		[Parameter] public Func<TItem, string> IconField { get; set; }

		/// <summary>
		/// The Value which checkbox will return when checked   
		/// </summary>
		[Parameter] public Func<TItem, string> IconTooltipField { get; set; }

		/// <summary>
		/// CSS Style for the Checkbox container    
		/// </summary>
		[Parameter] public string Style { get; set; }

		/// <summary>
		/// The item style
		/// </summary>
		[Parameter] public Func<TItem, string> ItemStyle { get; set; }

		/// <summary>
		/// Any childd content for the control (if needed)   
		/// </summary>
		[Parameter] public RenderFragment ChildContent { get; set; }

		/// <summary>
		/// The array which contains the list of selected checkboxs   
		/// </summary>
		[Parameter] public List<string> SelectedValues { get; set; }

		/// <summary>
		/// Method to update the selected value on click on checkbox   
		/// </summary>
		/// <param name="aSelectedId"></param>
		/// <param name="aChecked"></param>
		public async Task CheckboxClicked(string aSelectedId, object aChecked)
		{
			if ((bool)aChecked)
			{
				if (!SelectedValues.Contains(aSelectedId))
					SelectedValues.Add(aSelectedId);
			}
			else
			{
				if (SelectedValues.Contains(aSelectedId))
					SelectedValues.Remove(aSelectedId);
			}
			StateHasChanged();

			await OnSelectionChanged.InvokeAsync();

		}

		/// <summary>
		/// 
		/// </summary>
		[Parameter]
		public EventCallback OnSelectionChanged { get; set; }

	}
}

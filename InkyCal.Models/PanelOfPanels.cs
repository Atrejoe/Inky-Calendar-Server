using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InkyCal.Models
{

	/// <summary>
	/// A panel displaying other panels
	/// </summary>
	public class PanelOfPanels : Panel
	{
		/// <summary>
		/// The panels to display (up to 5)
		/// </summary>
		[Required, MaxLength(5), MinLength(1)]
		public HashSet<SubPanel> Panels { get; set; } = [];

	}
}

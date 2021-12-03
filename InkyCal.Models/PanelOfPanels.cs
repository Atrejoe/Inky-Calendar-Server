using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InkyCal.Models
{
	public class PanelOfPanels : Panel
	{
		[Required, MaxLength(5), MinLength(1)]
		public HashSet<SubPanel> Panels { get; set;  } = new HashSet<SubPanel>();

	}
}

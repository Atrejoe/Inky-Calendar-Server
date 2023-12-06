using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InkyCal.Models
{
	/// <summary>
	/// The link between a <see cref="PanelOfPanels"/> and a <see cref="Panel"/> that is a child of it.
	/// </summary>
	[Table("SubPanel", Schema = "InkyCal")]
	public class SubPanel
	{

		/// <summary>
		/// Foreign key for <see cref="PanelOfPanels"/>
		/// </summary>
		[Key, Required]
		public Guid IdParent { get; set; }

		/// <summary>
		/// Foreign key for the panel it refers to <see cref="Panel"/>
		/// </summary>
		[Key, Required]
		public Guid IdPanel { get; set; }

		/// <summary>
		/// The <see cref="PanelOfPanels"/> this <see cref="SubPanel"/> belongs to.
		/// </summary>
		[ForeignKey(nameof(IdParent))]
		public virtual Panel Panel { get; set; }

		/// <summary>
		/// The sort index
		/// </summary>
		[Key]
		public byte SortIndex { get; set; }

		/// <summary>
		/// The ratio for distribution
		/// </summary>
		[Range(1, short.MaxValue), DefaultValue(100)]
		public short Ratio { get; set; } = 100;
	}
}

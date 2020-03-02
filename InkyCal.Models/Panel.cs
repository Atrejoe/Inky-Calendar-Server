using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InkyCal.Models
{

	[Table("Panel", Schema = "InkyCal")]
	public class Panel
	{
		[Required, MaxLength(255), Column(Order = 1)]
		public string Name { get; set; }

		public User Owner { get; set; }

		[Key, Column(Order = 0)]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Id { get; set; }

		public DisplayModel Model { get; set; }

		/// <summary>
		/// Indicates how the panels is rotated, affects how default width and height are interpreted.
		/// Default value is <see cref="Rotation.CounterClockwise"/> (portrait)
		/// </summary>
		public Rotation Rotation { get; set; } = Rotation.CounterClockwise;

		/// <summary>
		/// Width as seen in landscape
		/// </summary>
		public int? Width { get; set; }

		/// <summary>
		/// Height as seen in landscape
		/// </summary>
		public int? Height { get; set; }
	}
}

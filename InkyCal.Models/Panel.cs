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

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Panel"/> is starred.
		/// </summary>
		/// <value>
		///   <c>true</c> if starred; otherwise, <c>false</c>.
		/// </value>
		public bool Starred { get; set; }
	}
}

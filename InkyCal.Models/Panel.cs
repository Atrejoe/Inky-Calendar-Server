using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InkyCal.Models
{

	/// <summary>
	/// A display pane,
	/// </summary>
	[Table("Panel", Schema = "InkyCal")]
	public class Panel : ITimeStampable
	{
		/// <summary>
		/// The display name of the panel
		/// </summary>
		[Required, MaxLength(255), Column(Order = 1)]
		public string Name { get; set; }

		/// <summary>
		/// The <see cref="User"/> the panel belongs to.
		/// </summary>
		[Required]
		public User Owner { get; set; }

		/// <summary>
		/// The unique identifier for this panel
		/// </summary>
		[Key, Column(Order = 0)]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Id { get; set; }

		/// <summary>
		/// The <see cref="DisplayModel"/> this panel uses
		/// </summary>
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

		/// <summary>
		/// The date and time at which the panel was created
		/// </summary>
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public DateTime Created { get; set; }

		/// <summary>
		/// The date and time at which the panel was last modified
		/// </summary>
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public DateTime Modified { get; set; }

		/// <summary>
		/// The number of times the panel has been read
		/// </summary>
		public uint AccessCount { get; set; }

		/// <summary>
		/// The date and time at which the calendar was last accessed
		/// </summary>
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public DateTime Accessed { get; set; }

		/// <summary>
		/// Indicates modification timestamp should not be set. Not persisted in the database.
		/// </summary>
		[NotMapped]
		public bool SkipModificationTimestamp { get; set; }
	}
}

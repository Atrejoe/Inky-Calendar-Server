using System;

namespace InkyCal.Models
{
	/// <summary>
	/// Allows keeping track of timestamps
	/// </summary>
	public interface ITimeStampable
	{
		/// <summary>
		/// The date and time this entity was created
		/// </summary>
		DateTime Created { get; set; }

		/// <summary>
		/// The date and time this entity was last modified
		/// </summary>
		DateTime Modified { get; set; }

		/// <summary>
		/// Allows instructing the database context to (temporarily) prevent setting the <see cref="Modified"/> timestamp
		/// </summary>
		bool SkipModificationTimestamp { get; set; }
	}
}

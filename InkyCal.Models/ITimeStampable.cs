using System;

namespace InkyCal.Models
{
	/// <summary>
	/// Allows keeping track of timestamps
	/// </summary>
	public interface ITimeStampable
	{
		DateTime Created { get; set; }

		DateTime Modified { get; set; }

		/// <summary>
		/// Allows instructing the database context to (temporarily) prevent setting the <see cref="Modified"/> timestamp
		/// </summary>
		bool SkipModificationTimestamp { get; set; }
	}
}

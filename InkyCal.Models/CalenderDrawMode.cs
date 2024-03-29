﻿using System.ComponentModel.DataAnnotations;

namespace InkyCal.Models
{
	/// <summary>
	/// 
	/// </summary>
	public enum CalenderDrawMode : byte
	{
		/// <summary>
		/// A list of events
		/// </summary>
		[Display(Name = "List view", Description = "A simple list of events, grouped by day", ShortName = "List")]
		List = 0,
		/// <summary>
		/// A generated image, based on the events of the first day in a list of events.
		/// </summary>
		[Display(Name = "AI generated image (⚠️ experimental ⚠️)", Description = "An impression of the first day of events, generated by AI !!!!", ShortName = "AI Generated")]
		AIImage = 1
	}
}

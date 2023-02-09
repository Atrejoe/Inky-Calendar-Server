using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace InkyCal.Models
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Panel" />
	public abstract class CurrentDatePanel : Panel
	{
		/// <summary>
		/// Gets the date.
		/// </summary>
		/// <value>
		/// The date.
		/// </value>
		[NotMapped]
		public virtual DateTime Date => DateTime.Now;
	}
}

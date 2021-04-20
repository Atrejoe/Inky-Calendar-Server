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
		[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
		public virtual DateTime Date { get { return DateTime.Now; } set { } }
	}
}

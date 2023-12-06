using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InkyCal.Models
{
	/// <summary>
	/// Authenticated user
	/// </summary>
	[Table("User", Schema = "InkyCal")]
	public class User
	{
		/// <summary>
		/// 
		/// </summary>
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		/// <summary>
		/// Foreign keys to the .Net Identity user id.
		/// </summary>
		public string IdentityUserId { get; set; }

		/// <summary>
		/// The panels this user has
		/// </summary>
		[Required, MaxLength(5), MinLength(0)]
		public HashSet<Panel> Panels { get; set; } = [];
	}
}

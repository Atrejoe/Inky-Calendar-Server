using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InkyCal.Models
{
	[Table("User", Schema = "InkyCal")]
	public class User
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public string IdentityUserId { get; set; }

		[Required, MaxLength(5), MinLength(0)]
		public HashSet<Panel> Panels { get; set; } = new HashSet<Panel>();
	}
}

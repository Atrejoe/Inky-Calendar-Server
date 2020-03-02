using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
//using Microsoft.AspNetCore.Identity;

namespace InkyCal.Models
{
	[Table("User", Schema = "InkyCal")]
	public class User
	{

		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public string IdentityUserId { get; set; }

		//public IdentityUser IdentityUser { get; set; }
	}
}

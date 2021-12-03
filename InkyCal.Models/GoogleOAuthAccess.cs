using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
//using Microsoft.AspNetCore.Identity;

namespace InkyCal.Models
{
	[Table("User_GoogleOAuthAccess", Schema = "InkyCal")]
	public class GoogleOAuthAccess
	{

		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public User User { get; set; }

		[Required, MaxLength(200), MinLength(1)]
		public string AccessToken { get; set; }

		[Required]
		public DateTimeOffset AccessTokenExpiry { get; set; }

		[Required, MaxLength(200), MinLength(1)]
		public string RefreshToken { get; set; }

		//public IdentityUser IdentityUser { get; set; }
	}

	[Table("CalendarPanel_GoogleCalender", Schema = "InkyCal")]
	public class SubscribedGoogleCalender : IEquatable<SubscribedGoogleCalender>
	{
		[Required, MaxLength(255), MinLength(1)]
		public string Calender { get; set; }
		[Key]
		public Guid Panel { get; set; }
		[Key]
		public int AccessToken { get; set; }

		public override bool Equals(object obj)
		{
			return base.Equals(obj  as SubscribedGoogleCalender);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		bool IEquatable<SubscribedGoogleCalender>.Equals(SubscribedGoogleCalender other)
		{
			return other!= null
				&& AccessToken.Equals(other.AccessToken)
				&& Calender.Equals(other.Calender);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return HashCode.Combine(Calender, AccessToken);
		}
	}
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InkyCal.Models
{
	[Table("User_GoogleOAuthAccess", Schema = "InkyCal")]
	public class GoogleOAuthAccess
	{

		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public User User { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// GoogleOAuth access token can be 2048 bytes <a href="https://developers.google.com/identity/protocols/oauth2">link</a>.
		/// But Google also permits itself to change the token length. Removing the limit, making the field nvarchar(max)
		/// </remarks>
		[Required, MinLength(1)]
		public string AccessToken { get; set; }

		[Required]
		public DateTimeOffset AccessTokenExpiry { get; set; }

		[Required, MaxLength(200), MinLength(1)]
		public string RefreshToken { get; set; }

	}

	[Table("CalendarPanel_GoogleCalender", Schema = "InkyCal")]
	public sealed class SubscribedGoogleCalender : IEquatable<SubscribedGoogleCalender>
	{
		[Required, MaxLength(255), MinLength(1)]
		public string Calender { get; set; }
		[Key]
		public Guid Panel { get; set; }
		[Key, Column("AccessToken")]
		public int IdAccessToken { get; set; }

		public GoogleOAuthAccess AccessToken { get; set; }

		public override bool Equals(object obj) 
			=> Equals(obj as SubscribedGoogleCalender);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(SubscribedGoogleCalender other) 
			=> other != null
				&& IdAccessToken.Equals(other.IdAccessToken)
				&& Calender.Equals(other.Calender);

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode() 
			=> HashCode.Combine(Calender, IdAccessToken);
	}
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InkyCal.Models
{
	/// <summary>
	/// Google OAUth access information for a <see cref="User"/>.
	/// </summary>
	[Table("User_GoogleOAuthAccess", Schema = "InkyCal")]
	public class GoogleOAuthAccess
	{

		/// <summary>
		/// Identifier of the access information
		/// </summary>
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		/// <summary>
		/// The <see cref="User"/> this access information belongs to.
		/// </summary>
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

		/// <summary>
		/// Access token expiry date
		/// </summary>
		[Required]
		public DateTimeOffset AccessTokenExpiry { get; set; }

		/// <summary>
		/// Refresh token
		/// </summary>
		[Required, MaxLength(200), MinLength(1)]
		public string RefreshToken { get; set; }

	}

	/// <summary>
	/// A Google calendar subscribed to by a <see cref="User"/> for a <see cref="CalendarPanel"/>
	/// </summary>
	[Table("CalendarPanel_GoogleCalender", Schema = "InkyCal")]
	public sealed class SubscribedGoogleCalender : IEquatable<SubscribedGoogleCalender>
	{
		/// <summary>
		/// 
		/// </summary>
		[Required, MaxLength(255), MinLength(1)]
		public string Calender { get; set; }

		/// <summary>
		/// Foreign key for <see cref="CalendarPanel"/>
		/// </summary>
		[Key]
		public Guid Panel { get; set; }

		/// <summary>
		/// Foreign key for <see cref="AccessToken"/>
		/// </summary>
		[Key, Column("AccessToken")]
		public int IdAccessToken { get; set; }

		/// <summary>
		/// Refers tot he <see cref="GoogleOAuthAccess"/> this <see cref="SubscribedGoogleCalender"/> belongs to.
		/// </summary>
		public GoogleOAuthAccess AccessToken { get; set; }

		/// <summary>
		/// Compares two <see cref="SubscribedGoogleCalender"/>s for equality.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
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

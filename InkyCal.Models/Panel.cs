using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InkyCal.Models
{

	[Table("Panel", Schema = "InkyCal")]
	public class Panel
	{
		[Required, MaxLength(255), Column(Order = 1)]
		public string Name { get; set; }

		public User Owner { get; set; }

		[Key, Column(Order = 0)]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Id { get; set; }

		public DisplayModel Model { get; set; }

		/// <summary>
		/// Indicates how the panels is rotated, affects how default width and height are interpreted.
		/// Default value is <see cref="Rotation.CounterClockwise"/> (portrait)
		/// </summary>
		public Rotation Rotation { get; set; } = Rotation.CounterClockwise;

		/// <summary>
		/// Width as seen in landscape
		/// </summary>
		public int? Width { get; set; }

		/// <summary>
		/// Height as seen in landscape
		/// </summary>
		public int? Height { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Panel"/> is starred.
		/// </summary>
		/// <value>
		///   <c>true</c> if starred; otherwise, <c>false</c>.
		/// </value>
		public bool Starred { get; set; }
	}


	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="InkyCal.Models.PanelCacheKey" />
	public class PanelInstanceCacheKey : PanelCacheKey
	{
		/// <summary>
		/// The default expiration in seconds
		/// </summary>
		public const int DefaultExpirationInSeconds = 30;

		public PanelInstanceCacheKey(Guid guid, TimeSpan? expiration = null) : base(expiration.GetValueOrDefault(TimeSpan.FromSeconds(DefaultExpirationInSeconds)))
		{
			Guid = guid;
		}

		/// <summary>
		/// Gets the unique identifier for this panel
		/// </summary>
		/// <value>
		/// The unique identifier.
		/// </value>
		public Guid Guid { get; }

		public override int GetHashCode() => HashCode.Combine(Guid, base.GetHashCode());

		protected override bool Equals(PanelCacheKey other)
		{
			return other is PanelInstanceCacheKey pic
				&& pic.Guid.Equals(Guid);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="System.IEquatable{PanelCacheKey}" />
	public class PanelCacheKey : IEquatable<PanelCacheKey>
	{
		/// <summary>
		/// Gets the expiration.
		/// </summary>
		/// <value>
		/// The expiration.
		/// </value>
		public TimeSpan Expiration { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PanelCacheKey"/> class.
		/// </summary>
		/// <param name="expiration">The expiration.</param>
		public PanelCacheKey(TimeSpan expiration)
		{
			Expiration = expiration;
		}

		bool IEquatable<PanelCacheKey>.Equals(PanelCacheKey other)
		{
			return Equals(other);
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public sealed override bool Equals(object obj)
		{
			return obj != null
				&& obj.GetType().Equals(GetType())//On when matching exact type
				&& ((PanelCacheKey)obj).Expiration.Equals(Expiration)
				&& ((PanelCacheKey)obj).Equals(this);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another <see cref="PanelCacheKey" /> (or derived class))
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
		/// </returns>
		protected virtual bool Equals(PanelCacheKey other) => true;

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode() => HashCode.Combine(Expiration, GetType());
	}
}

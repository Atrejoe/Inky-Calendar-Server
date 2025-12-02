using System;
using System.Text.Json.Serialization;
using InkyCal.Utils;

namespace InkyCal.Models
{
	/// <summary>
	/// Base class for panel cache keys
	/// </summary>
	/// <seealso cref="IEquatable{PanelCacheKey}" />
	[JsonDerivedType(typeof(PanelInstanceCacheKey), "Instance")]
	[JsonDerivedType(typeof(CalendarPanelCacheKey), "Calendar")]
	[JsonDerivedType(typeof(ImagePanelCacheKey), "Image")]
	[JsonDerivedType(typeof(NewsPaperPanelCacheKey), "NewsPaper")]
	[JsonDerivedType(typeof(NewYorkTimePanelCacheKey), "NewYorkTime")]
	[JsonDerivedType(typeof(PerPanelCacheKey), "PanelOfPanel")]
	[JsonDerivedType(typeof(WeatherPanelCacheKey), "Weather")]
	public abstract class PanelCacheKey : IEquatable<PanelCacheKey>
	{
		/// <summary>
		/// Gets the expiration.
		/// </summary>
		/// <value>
		/// The expiration.
		/// </value>
		[JsonIgnore]
		public TimeSpan Expiration { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PanelCacheKey"/> class.
		/// </summary>
		/// <param name="expiration">The expiration.</param>
		protected PanelCacheKey(TimeSpan expiration)
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
		public override bool Equals(object obj)
			=> Equals(obj as PanelCacheKey);

		/// <summary>
		/// Indicates whether the current object is equal to another <see cref="PanelCacheKey" /> (or derived class))
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
		/// </returns>
		protected virtual bool Equals(PanelCacheKey other)
			=> other != null
				&& other.GetType().Equals(GetType())//On when matching exact type
				&& other.Expiration.Equals(Expiration);

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
			=> HashCode.Combine(Expiration, GetType());
	}
}

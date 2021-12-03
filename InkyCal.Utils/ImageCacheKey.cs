using System;
using InkyCal.Models;

namespace InkyCal.Utils
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="System.IEquatable{ImageCacheKey}" />
	public sealed class ImageCacheKey : IEquatable<ImageCacheKey>
	{
		/// <summary>
		/// Gets the image settings.
		/// </summary>
		/// <value>
		/// The image settings.
		/// </value>
		public ImageSettings ImageSettings { get; }
		/// <summary>
		/// Gets the panel cache key.
		/// </summary>
		/// <value>
		/// The panel cache key.
		/// </value>
		public PanelCacheKey PanelCacheKey { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ImageCacheKey"/> class.
		/// </summary>
		/// <param name="panelCacheKey">The panel cache key.</param>
		/// <param name="imageSettings">The image settings.</param>
		/// <exception cref="ArgumentNullException">
		/// panelCacheKey
		/// or
		/// imageSettings
		/// </exception>
		public ImageCacheKey(PanelCacheKey panelCacheKey, ImageSettings imageSettings)
		{
			PanelCacheKey = panelCacheKey ?? throw new ArgumentNullException(nameof(panelCacheKey));
			ImageSettings = imageSettings ?? throw new ArgumentNullException(nameof(imageSettings));
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			return obj is ImageCacheKey ick
				&& Equals(ick);
		}


		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
		/// </returns>
		public bool Equals(ImageCacheKey other)
			=> other != null
				&& other.ImageSettings.Equals(ImageSettings)
				&& other.PanelCacheKey.Equals(PanelCacheKey);

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode() => HashCode.Combine(
												ImageSettings.GetHashCode(), 
												PanelCacheKey.GetHashCode()
											);
	}
}

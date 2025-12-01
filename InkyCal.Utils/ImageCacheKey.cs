using System;
using System.Runtime.Serialization;
using InkyCal.Models;

namespace InkyCal.Utils
{
	/// <summary>
	/// Cache key combining panel and image settings
	/// </summary>
	/// <seealso cref="IEquatable{ImageCacheKey}" />
	/// <remarks>
	/// Initializes a new instance of the <see cref="ImageCacheKey"/> class.
	/// </remarks>
	/// <param name="panelCacheKey">The panel cache key.</param>
	/// <param name="imageSettings">The image settings.</param>
	/// <exception cref="ArgumentNullException">
	/// panelCacheKey
	/// or
	/// imageSettings
	/// </exception>
	public sealed class ImageCacheKey(PanelCacheKey panelCacheKey, ImageSettings imageSettings) : IEquatable<ImageCacheKey>, IJsonSerializable
	{
		/// <summary>
		/// Gets the image settings.
		/// </summary>
		/// <value>
		/// The image settings.
		/// </value>
		public ImageSettings ImageSettings { get; } = imageSettings ?? throw new ArgumentNullException(nameof(imageSettings));
		/// <summary>
		/// Gets the panel cache key.
		/// </summary>
		/// <value>
		/// The panel cache key.
		/// </value>
		public PanelCacheKey PanelCacheKey { get; } = panelCacheKey ?? throw new ArgumentNullException(nameof(panelCacheKey));

		/// <summary>
		/// Deserialization constructor
		/// </summary>
		/// <param name="info">The serialization info</param>
		/// <param name="context">The streaming context</param>
		private ImageCacheKey(SerializationInfo info, StreamingContext context)
			: this(
				(PanelCacheKey)info.GetValue(nameof(PanelCacheKey), typeof(PanelCacheKey)),
				(ImageSettings)info.GetValue(nameof(ImageSettings), typeof(ImageSettings)))
		{
		}

		/// <inheritdoc/>
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			ArgumentNullException.ThrowIfNull(info);

			info.AddValue(nameof(PanelCacheKey), PanelCacheKey, typeof(PanelCacheKey));
			info.AddValue(nameof(ImageSettings), ImageSettings, typeof(ImageSettings));
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj) => obj is ImageCacheKey ick
				&& Equals(ick);


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

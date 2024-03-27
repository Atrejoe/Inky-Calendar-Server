using System;
using System.Threading.Tasks;
using InkyCal.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using StackExchange.Profiling;

namespace InkyCal.Utils
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="PanelCacheKey" />
	/// <remarks>
	/// Initializes a new instance of the <see cref="ImagePanelCacheKey"/> class.
	/// </remarks>
	/// <param name="expiration">The expiration.</param>
	/// <param name="imageUrl">The image URL.</param>
	/// <param name="rotateImage">The rotate image.</param>
	public class ImagePanelCacheKey(TimeSpan expiration, Uri imageUrl, RotateMode rotateImage) : PanelCacheKey(expiration)
	{

		/// <summary>
		/// Gets the image URL.
		/// </summary>
		/// <value>
		/// The image URL.
		/// </value>
		public Uri ImageUrl { get; } = imageUrl;
		/// <summary>
		/// Gets the rotate image.
		/// </summary>
		/// <value>
		/// The rotate image.
		/// </value>
		public RotateMode RotateImage { get; } = rotateImage;

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), ImageUrl, RotateImage);

		/// <summary>
		/// Refers to <see cref="Equals(PanelCacheKey)"/>.
		/// </summary>
		public override bool Equals(object obj) 
			=> Equals(obj as ImagePanelCacheKey);

		/// <summary>
		/// Indicates whether the current object is equal to another <see cref="T:InkyCal.Models.PanelCacheKey" /> (or derived class))
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
		/// </returns>
		protected override bool Equals(PanelCacheKey other) => other is ImagePanelCacheKey ipc
				&& ipc.ImageUrl.Equals(ImageUrl)
				&& ipc.RotateImage.Equals(RotateImage);
	}

	/// <summary>
	/// An image panel, assumes a landscape image, resizes and flips it to portait.
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
	/// <param name="imageUrl"></param>
	/// <param name="rotateImage"></param>
	public class ImagePanelRenderer(Uri imageUrl, RotateMode rotateImage = RotateMode.None) : IPanelRenderer
	{
		private readonly Uri imageUrl = imageUrl ?? throw new ArgumentNullException(nameof(imageUrl));
		private readonly RotateMode rotateImage = rotateImage;
		private readonly ImagePanelCacheKey cacheKey = new ImagePanelCacheKey(expiration: TimeSpan.FromMinutes(1), imageUrl, rotateImage);

		PanelCacheKey IPanelRenderer.CacheKey => cacheKey;

		//private byte[] GetTestImage()
		//{
		//	return client.GetByteArrayAsync(imageUrl.ToString()).Result;
		//}

		/// <inheritdoc/>
		public async Task<Image> GetImage(int width, int height, Color[] colors, IPanelRenderer.Log log) => await Task.Run(async () =>
																													 {
																														 if (colors is null)
																															 colors = new[] { Color.White, Color.Black };

																														 Image<Rgba32> image;
																														 try
																														 {
																															 image = Image.Load<Rgba32>(await imageUrl.LoadCachedContent(TimeSpan.FromMinutes(10)));
																														 }
																														 catch (UnknownImageFormatException ex)
																														 {
																															 ex.Data["ImageUrl"] = imageUrl;
																															 throw;
																														 }

																														 using (MiniProfiler.Current.Step($"Resizing image and reducing image palette"))
																															 image.Mutate(x => x
																															 .Rotate(rotateImage)
																															 .Resize(new ResizeOptions() { Mode = ResizeMode.Crop, Size = new Size(width, height) })
																															 //.BackgroundColor(Color.White)
																															 .Quantize(new PaletteQuantizer(colors))
																															 );

																														 return image;
																													 });

	}
}

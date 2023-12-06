using System.ComponentModel.DataAnnotations;

namespace InkyCal.Models
{

	/// <summary>
	/// A panel that displays an image, based on an external image
	/// </summary>
	public class ImagePanel : Panel
	{
		/// <summary>
		/// The url to the image
		/// </summary>
		[Url, Validation.Url(System.UriKind.Absolute), Required]
		public string Path { get; set; }

		/// <summary>
		/// When specified, this value is used in a <see cref="System.Net.Http.HttpMethod.Post"/> request, otherwise <see cref="System.Net.Http.HttpMethod.Post"/>.
		/// </summary>
		public string Body { get; set; }

		/// <summary>
		/// Indicates if the image should be rotated
		/// </summary>
		public Rotation ImageRotation { get; set; } = Rotation.None;
	}
}

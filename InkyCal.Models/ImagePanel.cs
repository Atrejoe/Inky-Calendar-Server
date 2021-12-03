using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InkyCal.Models
{
	[Table("ImagePanel", Schema = "InkyCal")]

	public class ImagePanel : Panel
	{
		[Url, Validation.Url(System.UriKind.Absolute), Required]
		public string Path { get; set; }
		public string Body { get; set; }

		/// <summary>
		/// Indicates if the image should be rotated
		/// </summary>
		public Rotation ImageRotation { get; set; } = Rotation.None;
	}
}

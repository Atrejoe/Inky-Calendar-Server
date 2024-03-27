using System;
using System.CodeDom.Compiler;
using System.ComponentModel.DataAnnotations;

namespace InkyCal.Utils.NewPaperRenderer.FreedomForum.Models
{

	/// <summary>
	/// Newpaper information, obtained from <a href="https://api.freedomforum.org/cache/papers.js"></a>
	/// </summary>
	[GeneratedCode("paste-as-json", "Visual Studio 2022")]
	public partial class NewsPaper
	{

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>Can be use to download the PDF from <a href="https://cdn.freedomforum.org/dfp/pdf14/{PaperId}.pdf">https://cdn.freedomforum.org/dfp/pdf14/{PaperId}.pdf</a></remarks>
		[Key]
		public string PaperId { get; set; }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public string Title { get; set; }
		public string Location { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string Country { get; set; }
		public string Latitude { get; set; }
		public string Longitude { get; set; }
		public string Website { get; set; }
		public string Region { get; set; }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
	}

	public partial class NewsPaper {

		/// <summary>
		/// The url for the Pdf to be downloaded
		/// </summary>
		public Uri PDFUrl(DateTime date) 
			=> new($"https://cdn.freedomforum.org/dfp/pdf{date.Day}/{PaperId}.pdf");
	}
}

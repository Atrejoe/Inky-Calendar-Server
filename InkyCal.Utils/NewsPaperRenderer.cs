// Ignore Spelling: Utils

using System;
using System.Threading.Tasks;
using InkyCal.Models;
using InkyCal.Utils.NewPaperRenderer.FreedomForum;

namespace InkyCal.Utils
{

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="PanelCacheKey" />
	public class NewsPaperPanelCacheKey : PanelCacheKey
	{
		internal readonly string NewspaperId;

		/// <summary>
		/// Initializes a new instance of the <see cref="WeatherPanelCacheKey"/> class.
		/// </summary>
		/// <param name="expiration"></param>
		/// <param name="newspaperId">The newspaper identifier.</param>
		public NewsPaperPanelCacheKey(TimeSpan expiration, string newspaperId) : base(expiration)
		{
			this.NewspaperId = newspaperId;
		}

		/// <summary>
		/// Included <see cref="NewspaperId"/> in hashcode
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), NewspaperId.ToUpperInvariant());

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as NewsPaperPanelCacheKey);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		protected override bool Equals(PanelCacheKey other)
		{
			return other is NewsPaperPanelCacheKey wpc
				&& base.Equals(other)
				&& NewspaperId == wpc.NewspaperId;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class NewsPaperRenderException : Exception
	{
		/// <inheritdoc/>
		public NewsPaperRenderException() { }
		/// <inheritdoc/>
		public NewsPaperRenderException(string message) : base(message) { }
		/// <inheritdoc/>
		public NewsPaperRenderException(string message, Exception inner) : base(message, inner) { }
	}

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="PdfRenderer{NewYorkTimesPanel}" />
	public class NewsPaperRenderer : PdfRenderer<NewsPaperPanel>
	{
		/// <summary>
		/// The id of the newspaper to obtain
		/// </summary>
		public string NewsPaperId { private set; get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="panel"></param>
		public NewsPaperRenderer(NewsPaperPanel panel) : base(panel) { }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="newsPaperId"></param>
		public NewsPaperRenderer(string newsPaperId) : base()
		{
			NewsPaperId = newsPaperId;
		}

		/// <summary>
		/// Indicated resulting image can be cached for one hour.
		/// </summary>
		public override PanelCacheKey CacheKey => new NewsPaperPanelCacheKey(TimeSpan.FromHours(1), NewsPaperId);

		/// <summary>
		/// Returns url, based from (cached version of) <see cref="NewsPaperPanel.NewsPaperId"/>.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="NewsPaperRenderException">When newspaper failed to download for multiple days.</exception>
		protected override async Task<byte[]> GetPDF()
		{
			var c = new ApiClient();
			if (!(await c.GetNewsPapers()).TryGetValue(NewsPaperId, out var newsPaper))
				throw new NewsPaperRenderException($"Newspaper with id '{NewsPaperId}' is unknown.");

			byte[] pdf;
			try
			{
				pdf = await DownloadHelper.DownloadFileByDay(newsPaper.PDFUrl);
			}
			catch (DownloadException ex)
			{
				throw new NewsPaperRenderException($"Failed to download {newsPaper.PaperId} ('{newsPaper.Title}') homepage.", ex);
			}

			return pdf;
		}

		/// <summary>
		/// Reads <see cref="NewsPaperPanel.NewsPaperId"/>.
		/// </summary>
		/// <param name="panel"></param>
		protected override void ReadConfig(NewsPaperPanel panel)
		{
			NewsPaperId = panel.NewsPaperId;
		}
	}
}

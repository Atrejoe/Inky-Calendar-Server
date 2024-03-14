// Ignore Spelling: Utils

using System;
using System.Threading.Tasks;
using InkyCal.Models;

namespace InkyCal.Utils
{

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="PanelCacheKey" />
	public sealed class NewYorkTimePanelCacheKey : PanelCacheKey
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NewYorkTimePanelCacheKey"/> class.
		/// </summary>
		public NewYorkTimePanelCacheKey(TimeSpan expiration ) : base(expiration)
		{
		}

		/// <summary>
		/// Uses base gethashcode
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), GetType());

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj) => Equals(obj as NewYorkTimePanelCacheKey);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		protected override bool Equals(PanelCacheKey other) 
			=> other is NewYorkTimePanelCacheKey
				&& base.Equals(other);
	}

	/// <summary>
	/// 
	/// </summary>
	public class NewYorkTimeRenderException : Exception
	{
		/// <inheritdoc/>
		public NewYorkTimeRenderException() { }
		/// <inheritdoc/>
		public NewYorkTimeRenderException(string message) : base(message) { }
		/// <inheritdoc/>
		public NewYorkTimeRenderException(string message, Exception inner) : base(message, inner) { }
	}

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="PdfRenderer{NewYorkTimesPanel}" />
	public class NewYorkTimesRenderer : PdfRenderer<NewYorkTimesPanel>
	{
		/// <summary>
		/// Gets the date.
		/// </summary>
		/// <value>
		/// The date.
		/// </value>
		public DateTime Date { get; private set; } = DateTime.Now.Date.AddDays(0);

		/// <summary>
		/// Gets the cache key.
		/// </summary>
		/// <value>
		/// The cache key.
		/// </value>
		public override PanelCacheKey CacheKey => new NewYorkTimePanelCacheKey(TimeSpan.FromMinutes(60));

		/// <summary>
		/// Gets the Pdf file from <c>https://static01.nyt.com/images/{Date:yyyy}/{Date:MM}/{Date:dd}/nytfrontpage/scan.pdf</c>
		/// </summary>
		/// <returns></returns>
		/// <exception cref="NewYorkTimeRenderException"/>
		protected override async Task<byte[]> GetPDF()
		{

			byte[] pdf;
			try
			{
				pdf = await DownloadHelper.DownloadFileByDay((DateTime d) =>
				{
					//No news, just ads on sunday, skip to saturday?
					if (d.DayOfWeek == DayOfWeek.Sunday)
						d = d.AddDays(-1);

					return new Uri($"https://static01.nyt.com/images/{d:yyyy}/{d:MM}/{d:dd}/nytfrontpage/scan.pdf");
				});
			}
			catch (DownloadException ex)
			{
				throw new NewYorkTimeRenderException($"Failed to download NYT homepage", ex);
			}

			return pdf;
		}

		/// <summary>
		/// Reads the configuration.
		/// </summary>
		/// <param name="panel">The panel.</param>
		protected override void ReadConfig(NewYorkTimesPanel panel)
		{
			if (panel is null)
				throw new ArgumentNullException(nameof(panel));

			Date = panel.Date;
		}
	}
}

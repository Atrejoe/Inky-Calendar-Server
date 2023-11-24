// Ignore Spelling: Utils

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using InkyCal.Models;

namespace InkyCal.Utils
{

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
		public override PanelCacheKey CacheKey => new PanelCacheKey(TimeSpan.FromMinutes(60));

		/// <summary>
		/// Gets the Pdf file from <c>https://static01.nyt.com/images/{Date:yyyy}/{Date:MM}/{Date:dd}/nytfrontpage/scan.pdf</c>
		/// </summary>
		/// <returns></returns>
		/// <exception cref="NewYorkTimeRenderException"/>
		protected override async Task<byte[]> GetPDF()
		{
			var d = Date;


			byte[] pdf = null;

			var tries = 0;
			const int maxTries = 5;

			while (tries <= maxTries
				&& !(pdf?.Any()).GetValueOrDefault())

			{

				//No news, just ads on sunday?
				if (d.DayOfWeek == DayOfWeek.Sunday)
					d = d.AddDays(-1);

				try
				{
					tries += 1;
					var url = new Uri($"https://static01.nyt.com/images/{d:yyyy}/{d:MM}/{d:dd}/nytfrontpage/scan.pdf");
					Console.WriteLine($"Downloading: {url}");
					pdf = await url.LoadCachedContent(TimeSpan.FromMinutes(60));
				}
				catch (HttpRequestException ex) when (
					tries <= maxTries
					&& (!ex.StatusCode.HasValue
					//On some days (sundays) NYT is not available.
					|| new[] { System.Net.HttpStatusCode.NotFound
							 , System.Net.HttpStatusCode.Forbidden
					}.Contains(ex.StatusCode.Value)))
				{
					Console.Error.WriteLine($"Failed to downloading: status code {ex.StatusCode}, error message: {ex.Message}");
					d = d.AddDays(-1);
				}
			}

			if (!(pdf?.Any()).GetValueOrDefault())
				throw new NewYorkTimeRenderException("Failed to download NYT homepage");

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

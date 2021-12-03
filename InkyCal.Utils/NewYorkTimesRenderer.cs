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
	/// <typeparam name="TPanel">The type of the panel.</typeparam>
	/// <seealso cref="InkyCal.Models.PanelCacheKey" />
	public class PanelCacheKey<TPanel> : PanelCacheKey where TPanel : IPanelRenderer
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PanelCacheKey{TPanel}"/> class.
		/// </summary>
		/// <param name="expiration">The expiration.</param>
		public PanelCacheKey(TimeSpan expiration) : base(expiration)
		{
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="InkyCal.Utils.PdfRenderer{NewYorkTimesPanel}" />
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
		public override PanelCacheKey CacheKey => new PanelCacheKey<NewYorkTimesRenderer>(TimeSpan.FromMinutes(60));

		/// <summary>
		/// Gets the Pdf file from <c>https://static01.nyt.com/images/{Date:yyyy}/{Date:MM}/{Date:dd}/nytfrontpage/scan.pdf</c>
		/// </summary>
		/// <returns></returns>
		protected override async Task<byte[]> GetPdf()
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
					Console.WriteLine(url.ToString());
					pdf = await url.LoadCachedContent(TimeSpan.FromMinutes(60));
				}
				catch (HttpRequestException ex) when (tries <= maxTries && ex.Message.Contains("404"))
				{
					d = d.AddDays(-1);
				}
			}

			if (!(pdf?.Any()).GetValueOrDefault())
				throw new Exception("Failed to download NYT homepage");

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

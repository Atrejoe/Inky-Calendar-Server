using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using InkyCal.Models;

namespace InkyCal.Utils
{
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
		public DateTime Date { get; private set; } = DateTime.Now.AddDays(0);

		private static readonly HttpClient client = new HttpClient();

		/// <summary>
		/// Gets the Pdf file from <c>https://static01.nyt.com/images/{Date:yyyy}/{Date:MM}/{Date:dd}/nytfrontpage/scan.pdf</c>
		/// </summary>
		/// <returns></returns>
		protected override async Task<byte[]> GetPdf()
		{
			var d = Date;

			//No news, just ads on sunday?
			if (d.DayOfWeek == DayOfWeek.Sunday)
				d = d.AddDays(-1);

			byte[] pdf = null;

			var tries = 0;
			const int maxTries = 5;

			while (tries <= maxTries 
				&& !(pdf?.Any()).GetValueOrDefault())
				try
				{
					tries += 1;
					var url = new Uri($"https://static01.nyt.com/images/{d:yyyy}/{d:MM}/{d:dd}/nytfrontpage/scan.pdf");
					Console.WriteLine(url.ToString());
					pdf = await client.GetByteArrayAsync(url.ToString());
				}
				catch (HttpRequestException ex) when (tries <= maxTries && ex.Message.Contains("404"))
				{
					d = d.AddDays(-1);
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

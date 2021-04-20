using System;
using System.Diagnostics;
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
			var url = new Uri($"https://static01.nyt.com/images/{Date:yyyy}/{Date:MM}/{Date:dd}/nytfrontpage/scan.pdf");
			Trace.WriteLine(url.ToString());
			var pdf = await client.GetByteArrayAsync(url.ToString());
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

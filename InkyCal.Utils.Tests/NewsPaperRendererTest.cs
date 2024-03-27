using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using InkyCal.Models;
using InkyCal.Utils.NewPaperRenderer.FreedomForum;
using Xunit;
using Xunit.Abstractions;

namespace InkyCal.Utils.Tests
{
	/// <summary>
	/// Tests <see creaf="NewYorkTimesRenderer"/>
	/// </summary>
	public sealed class NewsPaperRendererTest : IPanelTests<NewsPaperRenderer>
	{
		protected override NewsPaperRenderer GetRenderer()
		{
			var client = new ApiClient();
			var newsPaper = client.GetNewsPapers().Result.FirstOrDefault().Value;

			output.WriteLine($"Returning renderer for newspaper : {newsPaper.PaperId} (url: \"{newsPaper.PDFUrl(DateTime.UtcNow)}\")");

			return new (newsPaper.PaperId);
		}

		public override async Task GetImageTest(DisplayModel displayModel)
		{
			try
			{
				await base.GetImageTest(displayModel);
			}
			catch (HttpRequestException ex) {
				throw new SkipException($"Http request failed, result inconclusive: {ex.Message}", ex);
			}
		}
	}
}

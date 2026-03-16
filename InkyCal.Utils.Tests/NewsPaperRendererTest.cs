using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using InkyCal.Models;
using InkyCal.Utils.NewPaperRenderer.FreedomForum;
using Xunit;
using Xunit.Sdk;

namespace InkyCal.Utils.Tests
{
	/// <summary>
	/// Tests <see creaf="NewYorkTimesRenderer"/>
	/// </summary>
	public sealed class NewsPaperRendererTest() : IPanelTests<NewsPaperRenderer>()
	{
		protected override NewsPaperRenderer GetRenderer()
		{
			var client = new ApiClient();
			var newsPaper = client.GetNewsPapers().Result.FirstOrDefault().Value;

			Console.WriteLine($"Returning renderer for newspaper : {newsPaper.PaperId} (url: \"{newsPaper.PDFUrl(DateTime.UtcNow)}\")");

			return new(newsPaper.PaperId);
		}

		public override async Task GetImageTest(DisplayModel displayModel)
		{
			try
			{
				await base.GetImageTest(displayModel);
			}
			catch (HttpRequestException ex)
			{
				await Console.Error.WriteLineAsync(ex.ToString());
				throw SkipException.ForSkip($"Http request failed");
			}
		}
	}
}

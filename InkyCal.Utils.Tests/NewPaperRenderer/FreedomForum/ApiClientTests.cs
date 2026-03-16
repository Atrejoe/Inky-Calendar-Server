using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using InkyCal.Utils.NewPaperRenderer.FreedomForum;
using Xunit;

namespace InkyCal.Utils.Tests.NewPaperRenderer.FreedomForum
{
	/// <summary>
	/// Tests <see cref="ApiClient"/>
	/// </summary>
	public class ApiClientTests()
	{
		[Fact(SkipExceptions = new[] { typeof(HttpRequestException) })]
		public async Task GetNewsPapersTests()
		{

			var client = new ApiClient();
			var actual = await client.GetNewsPapers(TestContext.Current.CancellationToken);

			Assert.NotNull(actual);
			Assert.NotEmpty(actual);

			foreach (var item in actual.Values)
				Assert.True(item.PDFUrl(DateTime.UtcNow).IsAbsoluteUri);

		}

		[Fact]
		/// <summary>
		/// 
		/// </summary>
		public async Task GetNewsPapersTest()
		{
			//arrange
			var client = new ApiClient();

			//act
			var actual = await client.GetNewsPapers(TestContext.Current.CancellationToken);

			//assert
			Assert.NotEmpty(actual);

			TestContext.Current.TestOutputHelper.WriteLine(string.Join("\n - ",
				actual.Values.OrderBy(x => x.Country)
				.ThenBy(x => x.State)
				.ThenBy(x => x.City)
				.ThenBy(x => x.Title)
				.Select(x => $"{x.Country}{(string.IsNullOrEmpty(x.State) ? "" : $" - {x.State}")} - {x.City} - {x.Title} ({x.Website})")));
		}
	}
}

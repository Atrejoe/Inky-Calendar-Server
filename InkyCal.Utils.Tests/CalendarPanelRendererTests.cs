using System;
using Xunit;
using Xunit.Abstractions;

namespace InkyCal.Utils.Tests
{
	public class CalendarPanelRendererTests
	{

		protected readonly ITestOutputHelper output;

		public CalendarPanelRendererTests(ITestOutputHelper output) {
			this.output = output;
		}

		[Theory()]
		[InlineData("Hello world", 10, "Hello  ...")]
		[InlineData("Hello world", null, "Hello world")]
		[InlineData("Hello world", 20, "Hello world")]
		[InlineData("Hello world", 0, "")]
		[InlineData("Hello world", 2, "")]
		public void ReduceSummaryTest(string originalSummary, int? remainingSize, string expected)
		{
			//arrange
			//act
			var actual = CalendarPanelRenderer.ReduceSummary(originalSummary, remainingSize);


			//assert
			Assert.Equal(expected, actual);
		}

		[Theory()]
		[InlineData("14:00", null)]
		[InlineData(null, null)]
		[InlineData("17:33", "18:42")]
		[InlineData(null, "18:42")]
		public void DescribePeriodTest(string start, string end)
		{
			//arrange
			TimeSpan? startDate = string.IsNullOrEmpty(start) ? null : TimeSpan.Parse(start);
			TimeSpan? endDate = string.IsNullOrEmpty(end) ? null : TimeSpan.Parse(end);

			//act
			var actual = CalendarPanelRenderer.DescribePeriod(startDate, endDate);

			//assert
			Assert.NotNull(actual);
			Assert.NotEmpty(actual);

			output.WriteLine(actual);

		}
	}
}

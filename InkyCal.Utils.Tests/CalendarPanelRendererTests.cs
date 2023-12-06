using System;
using System.Globalization;
using Xunit;
using Xunit.Abstractions;

namespace InkyCal.Utils.Tests
{
	public class CalendarPanelRendererTests
	{

		protected readonly ITestOutputHelper output;


		private readonly CultureInfo parseCulture = new CultureInfo("en-US");

		public CalendarPanelRendererTests(ITestOutputHelper output)
		{
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
			TimeSpan? startDate = string.IsNullOrEmpty(start) ? null : TimeSpan.Parse(start, parseCulture);
			TimeSpan? endDate = string.IsNullOrEmpty(end) ? null : TimeSpan.Parse(end, parseCulture);

			//act
			var actual = CalendarPanelRenderer.DescribePeriod(startDate, endDate);

			//assert
			Assert.NotNull(actual);
			Assert.NotEmpty(actual);

			output.WriteLine(actual);

		}

		[Theory()]
		#pragma warning disable format
		[InlineData("14:00", null   , "Hallo"      , "14:00 Hallo"                   )]
		[InlineData(null   , null   , "Hallo"      , "All day Hallo"                 )]
		[InlineData("17:33", "18:42", "Hi"         , "17:33 - 18:42 Hi"              )]
		[InlineData("17:33", "18:42", "Hello world", "17:33 - 18:42"          , 15   )]
		[InlineData("17:33", "18:42", "Hello world", "17:33 - 18:42  ..."     , 18   )]
		[InlineData("17:33", "18:42", "Hello world", "17:33 - 18:42  ..."     , 20, 2)]
		[InlineData("17:33", "18:42", "Hello world", "17:33 - 18:42 Hello ...", 25, 2)]
		[InlineData(null   , "18:42", "Hi"         , "All day Hi"                    )]
		#pragma warning restore format
		public void DescribeEventTest(string start, string end, string summary, string expectation, int characterPerLine = 50, int indentSize = 0)
		{
			//arrange

			TimeSpan? startDate = string.IsNullOrEmpty(start) ? null : TimeSpan.Parse(start, parseCulture);
			TimeSpan? endDate = string.IsNullOrEmpty(end) ? null : TimeSpan.Parse(end, parseCulture);

			var e = new Calendar.Event()
			{
				Date = DateTime.Now.Date,
				Start = startDate,
				End = endDate,
				Summary = summary
			};

			//act
			var actual = CalendarPanelRenderer.DescribeEvent(e, characterPerLine, indentSize);

			//assert
			Assert.Equal(expectation, actual);

			output.WriteLine(actual);
		}
	}
}

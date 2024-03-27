using Ical.Net;
using InkyCal.Server.Config;
using Xunit;
using Xunit.Abstractions;

namespace InkyCal.Utils.Tests
{
	/// <summary>
	/// Tests <see creaf="TestCalendarPanel"/> / <see cref="CalendarPanelRenderer"/>
	/// </summary>
	public sealed class CalendarImagePanelTests(ITestOutputHelper output) : IPanelTests<CalendarPanelRenderer>(output)
	{
		protected override TestCalendarImagePanelRenderer GetRenderer()
		{
			if (string.IsNullOrWhiteSpace(Config.OpenAIAPIKey))
				throw new SkipException("OpenAI API keys has not been configured, skipping tests.");

			return new TestCalendarImagePanelRenderer();
		}
	}
}

using InkyCal.Server.Config;
using Xunit.Sdk;

namespace InkyCal.Utils.Tests
{
	/// <summary>
	/// Tests <see creaf="TestCalendarPanel"/> / <see cref="CalendarPanelRenderer"/>
	/// </summary>
	public sealed class CalendarImagePanelTests() : IPanelTests<CalendarPanelRenderer>()
	{
		protected override TestCalendarImagePanelRenderer GetRenderer()
		{
			if (string.IsNullOrWhiteSpace(Config.OpenAIAPIKey))
				throw SkipException.ForSkip("OpenAI API keys has not been configured, skipping tests.");

			return new TestCalendarImagePanelRenderer();
		}
	}
}

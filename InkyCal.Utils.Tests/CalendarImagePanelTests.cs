using Xunit.Abstractions;

namespace InkyCal.Utils.Tests
{
	/// <summary>
	/// Tests <see creaf="TestCalendarPanel"/> / <see cref="CalendarPanelRenderer"/> using a <see cref="MockOpenAIService"/>.
	/// </summary>
	public sealed class CalendarImagePanelTests(ITestOutputHelper output) : IPanelTests<CalendarPanelRenderer>(output)
	{
		protected override TestCalendarImagePanelRenderer GetRenderer()
			=> new TestCalendarImagePanelRenderer(new MockOpenAIService());
	}
}

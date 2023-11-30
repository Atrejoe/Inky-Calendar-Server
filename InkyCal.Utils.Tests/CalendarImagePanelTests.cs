namespace InkyCal.Utils.Tests
{
	/// <summary>
	/// Tests <see creaf="TestCalendarPanel"/> / <see cref="CalendarPanelRenderer"/>
	/// </summary>
	public sealed class CalendarImagePanelTests : IPanelTests<CalendarPanelRenderer>
	{
		protected override TestCalendarImagePanelRenderer GetRenderer()
		{
			return new TestCalendarImagePanelRenderer();
		}
	}
}

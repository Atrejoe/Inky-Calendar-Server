namespace InkyCal.Utils.Tests
{
    /// <summary>
    /// Tests <see creaf="TestCalendarPanel"/>
    /// </summary>
    public sealed class TestCalendarPanelTests : IPanelTests<TestCalendarPanel>
    {
        protected override TestCalendarPanel GetPanel()
        {
            return new TestCalendarPanel();
        }
    }
}
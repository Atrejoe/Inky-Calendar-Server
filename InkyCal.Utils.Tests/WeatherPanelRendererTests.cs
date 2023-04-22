namespace InkyCal.Utils.Tests
{
	/// <summary>
	/// Tests <see creaf="TestCalendarPanel"/>
	/// </summary>
	public sealed class WeatherPanelRendererTests : IPanelTests<WeatherPanelRenderer>
	{
		protected override WeatherPanelRenderer GetRenderer()
		{
			return new WeatherPanelRenderer(
				"token", 
				"Rotterdam,NL");
		}
	}
}

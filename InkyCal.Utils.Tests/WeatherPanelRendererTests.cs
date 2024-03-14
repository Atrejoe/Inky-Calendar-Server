using Xunit.Abstractions;

namespace InkyCal.Utils.Tests
{
	/// <summary>
	/// Tests <see creaf="TestCalendarPanel"/>
	/// </summary>
	public sealed class WeatherPanelRendererTests(ITestOutputHelper output) : IPanelTests<WeatherPanelRenderer>(output)
	{
		protected override WeatherPanelRenderer GetRenderer()
		{
			return new WeatherPanelRenderer(
				InkyCal.Server.Config.Config.OpenWeatherAPIKey, 
				"Rotterdam,NL");
		}
	}
}

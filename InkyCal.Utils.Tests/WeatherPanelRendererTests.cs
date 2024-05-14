using InkyCal.Server.Config;
using Xunit;
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
			if (string.IsNullOrWhiteSpace(Config.OpenWeatherAPIKey))
				throw new SkipException("OpenWeather API keys has not been configured, skipping tests.");

			return new WeatherPanelRenderer(
				InkyCal.Server.Config.Config.OpenWeatherAPIKey, 
				"Rotterdam,NL");
		}
	}
}

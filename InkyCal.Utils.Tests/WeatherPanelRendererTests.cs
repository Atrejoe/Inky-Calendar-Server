using InkyCal.Server.Config;
using Xunit.Sdk;

namespace InkyCal.Utils.Tests
{
	/// <summary>
	/// Tests <see creaf="TestCalendarPanel"/>
	/// </summary>
	public sealed class WeatherPanelRendererTests() : IPanelTests<WeatherPanelRenderer>()
	{
		protected override WeatherPanelRenderer GetRenderer()
		{
			if (string.IsNullOrWhiteSpace(Config.OpenWeatherAPIKey))
				throw SkipException.ForSkip("OpenWeather API keys has not been configured, skipping tests.");

			return new WeatherPanelRenderer(
				InkyCal.Server.Config.Config.OpenWeatherAPIKey,
				"Rotterdam,NL");
		}
	}
}

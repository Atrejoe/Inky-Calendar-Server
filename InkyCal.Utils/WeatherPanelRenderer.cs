using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using InkyCal.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using StackExchange.Profiling;

namespace InkyCal.Utils
{

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="InkyCal.Models.PanelCacheKey" />
	public class WeatherPanelCacheKey: PanelCacheKey{
		internal readonly string Token;
		internal readonly string City;

		/// <summary>
		/// Initializes a new instance of the <see cref="WeatherPanelCacheKey"/> class.
		/// </summary>
		/// <param name="expiration"></param>
		/// <param name="token">The token.</param>
		/// <param name="city">The city.</param>
		public WeatherPanelCacheKey(TimeSpan expiration, string token, string city):base(expiration) {
			this.Token = token;
			this.City = city;
		}

		/// <summary>
		/// Included <see cref="Token"/> and <see cref="City"/>in hashcode
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Token, City.ToUpperInvariant());
	}

	/// <summary>
	/// A renderer for weather
	/// </summary>
	public class WeatherPanelRenderer : PanelRenderer<WeatherPanel>
	{
		private WeatherPanelCacheKey cacheKey;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="token"></param>
		/// <param name="city"></param>
		public WeatherPanelRenderer(string token, string city)
		{
			this.cacheKey = new WeatherPanelCacheKey(TimeSpan.FromMinutes(1), token, city);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WeatherPanelRenderer"/> class.
		/// </summary>
		/// <param name="panel">The panel.</param>
		public WeatherPanelRenderer(WeatherPanel panel)
		{
			Configure(panel);
		}

		/// <inheritdoc/>
		protected override void ReadConfig(WeatherPanel panel)
		{
			if (panel is null)
				throw new ArgumentNullException(nameof(panel));


			this.cacheKey = new WeatherPanelCacheKey(TimeSpan.FromMinutes(1), panel.Token, panel.Location);
		}


		/// <inheritdoc/>
		/// <returns>A panel with weather information</returns>
		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Contains catch-all-and-log logic")]
		[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing result")]
		override public async Task<Image> GetImage(int width, int height, SixLabors.ImageSharp.Color[] colors, IPanelRenderer.Log log)
		{
			//Forecast weather;
			//Station station;

			//using (var api = new OpenWeather.Noaa.Api())
			//{
			//	var stations = await api.GetStationsAsync();

			//	foreach (var s in stations.OrderBy(x => x.CountryCode).ThenBy(x => x.Name))
			//		Trace.WriteLine($"{s.Name}");

			//	var cityName = city.Split(',').FirstOrDefault();
			//	var countryCode = city.Split(',').Skip(1).FirstOrDefault();
			//	station = stations
			//				.OrderByDescending(x => x.CountryCode.Equals(countryCode, StringComparison.InvariantCultureIgnoreCase))
			//				.ThenByDescending(x => x.Name.StartsWith(cityName, StringComparison.InvariantCultureIgnoreCase))
			//				.ThenByDescending(x => x.Name.Equals(cityName, StringComparison.InvariantCultureIgnoreCase))
			//				.FirstOrDefault();

			//	var parameters = new WeatherParameters();
			//	parameters.SelectAll();


			//	weather = await api.GetForecastByStationAsync(station,
			//									  DateTime.UtcNow,
			//									  DateTime.UtcNow.AddDays(4),
			//									  OpenWeather.Noaa.Base.RequestType.Forcast,
			//									  OpenWeather.Noaa.Base.Units.Imperial,
			//									  parameters);
			//}

			colors.ExtractMeaningFullColors(
				out var primaryColor
				, out var supportColor
				, out var errorColor
				, out var backgroundColor
				);

			var result = PanelRenderHelper.CreateImage(width, height, backgroundColor);
			var textFont = new Font(FontHelper.MonteCarlo, 12);
			var weatherFont = new Font(FontHelper.WeatherIcons, 40);

			var textGraphicsOptions = new TextGraphicsOptions(new GraphicsOptions() { Antialias = false }, new TextOptions()
			{
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				WrapTextWidth = width,
				DpiX = 96,
				DpiY = 96
			});
			var rendererOptions = textGraphicsOptions.ToRendererOptions(textFont);
			var weatherRendererOptions = textGraphicsOptions.ToRendererOptions(weatherFont);

			Weather.RootObject weather;

			using (MiniProfiler.Current.Step($"Get weather for '{cacheKey.City}'"))
				try
				{
					using (var util = new Weather.Util(cacheKey.Token))
						weather = await util.GetForeCast(cacheKey.City);
				}
				catch (Weather.WeatherAPIRequestFailureException ex)
				{
					var explanation = "Weather service indicated API authentication failure failure";
					log?.Invoke(ex, true, explanation);

					result.Mutate(context =>
					{
						var y = 100;
						context.RenderErrorMessage(
							explanation,
							errorColor, backgroundColor, ref y, width, rendererOptions);
					});
					return result;
				}
				catch (Exception ex)
				{
					log?.Invoke(ex, true, "Weather service responds with unauthorized");

					result.Mutate(context =>
					{
						var y = 100;
						context.RenderErrorMessage(
							ex.Message.ToString(),
							errorColor, backgroundColor, ref y, width, rendererOptions);
					});
					return result;
				}

			var station = weather?.city;

			using (MiniProfiler.Current.Step("Draw weather panel"))
				result.Mutate(context =>
			{
				var y = 0;
				var locationInfo = $"{station?.name},{station?.country}";
				context.DrawText(
					textGraphicsOptions, locationInfo.ToSafeChars(textFont), textFont, primaryColor, new PointF(5, y));
				y += (int)Math.Ceiling(TextMeasurer.Measure(locationInfo, rendererOptions).Height);

				try
				{
					if (weather is null)
						context.DrawText(textGraphicsOptions, $"No weather found for {station.name}", textFont, primaryColor, new PointF(0, y));
					else
					{
						var firstIcon = weather.list.OrderBy(l => l.dt).FirstOrDefault().weather.FirstOrDefault().icon;
						var weatherIconDimensions = TextMeasurer.Measure(firstIcon, weatherRendererOptions);
						var widthPerIcon = (int)Math.Ceiling(weatherIconDimensions.Width);
						var heightPerIcon = (int)Math.Ceiling(weatherIconDimensions.Height);
						var iconPadding = 5;
						var icons = (int)Math.Floor((decimal)width / (widthPerIcon + 2 * iconPadding));

						var indentPerIcon = (int)Math.Ceiling(((double)width + iconPadding) / icons);

						var x = 0;
						foreach (var forecast in weather.list.OrderBy(l => l.dt).Take(icons))
						{

							if (FontHelper.WeatherIconsMap.TryGetValue(forecast.weather.FirstOrDefault()?.icon, out var icon))
								context.DrawText(
									textGraphicsOptions,
									icon,
									weatherFont,
									supportColor,
									new PointF(x + iconPadding, y));
							else
								context.DrawText(
									textGraphicsOptions,
									$"{forecast.weather.FirstOrDefault()?.icon} has no icon mapping",
									textFont,
									errorColor,
									new PointF(x, y));

							context.DrawText(
								textGraphicsOptions,
								$@"{forecast.Date.ToLocalTime():HH:mm}
{forecast.weather.FirstOrDefault()?.main}",
								textFont, primaryColor, new PointF(x + iconPadding, y + heightPerIcon + iconPadding));

							x += indentPerIcon;

						}

						//var serialized = JsonConvert.SerializeObject(weather, Formatting.Indented);
						//Console.Write(serialized);
					}

				}
				catch (Exception ex)
				{
					log?.Invoke(ex, false);

					Console.Error.WriteLine(ex.ToString());
					y = 100;
					context.RenderErrorMessage(
						ex.Message.ToString(),
						errorColor, backgroundColor, ref y, width, rendererOptions);
				}
			});


			return result;
		}

		/// <summary>
		/// Gets the cache key. (<see cref="WeatherPanelCacheKey"/>)
		/// </summary>
		/// <value>
		/// The cache key.
		/// </value>
		public override PanelCacheKey CacheKey
			=> cacheKey;
		
	}
}

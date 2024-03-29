﻿using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using InkyCal.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using StackExchange.Profiling;

namespace InkyCal.Utils
{

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="PanelCacheKey" />
	/// <remarks>
	/// Initializes a new instance of the <see cref="WeatherPanelCacheKey"/> class.
	/// </remarks>
	/// <param name="expiration"></param>
	/// <param name="token">The token.</param>
	/// <param name="city">The city.</param>
	public class WeatherPanelCacheKey(TimeSpan expiration, string token, string city) : PanelCacheKey(expiration)
	{
		internal readonly string Token = token;
		internal readonly string City = city;

		/// <summary>
		/// Included <see cref="Token"/> and <see cref="City"/>in hashcode
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Token, City.ToUpperInvariant());

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj) => Equals(obj as WeatherPanelCacheKey);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		protected override bool Equals(PanelCacheKey other) => other is WeatherPanelCacheKey wpc
				&& base.Equals(other)
				&& Token == wpc.Token
				&& City == wpc.City;
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
		public WeatherPanelRenderer(string token, string city) => cacheKey = new WeatherPanelCacheKey(TimeSpan.FromMinutes(1), token, city);

		/// <summary>
		/// Initializes a new instance of the <see cref="WeatherPanelRenderer"/> class.
		/// </summary>
		/// <param name="panel">The panel.</param>
		public WeatherPanelRenderer(WeatherPanel panel) => Configure(panel);

		/// <inheritdoc/>
		protected override void ReadConfig(WeatherPanel panel)
		{
			ArgumentNullException.ThrowIfNull(panel);

			cacheKey = new WeatherPanelCacheKey(TimeSpan.FromMinutes(1), panel.Token, panel.Location);
		}


		/// <inheritdoc/>
		/// <returns>A panel with weather information</returns>
		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Contains catch-all-and-log logic")]
		[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing result")]
		override public async Task<Image> GetImage(int width, int height, Color[] colors, IPanelRenderer.Log log)
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

			var result = PanelRenderingHelper.CreateImage(width, height, backgroundColor);
			var textFont = new Font(FontHelper.ProFont, 15);
			var weatherFont = new Font(FontHelper.WeatherIcons, 40);


			var isGrayScale = (SixLabors.ImageSharp.Color color) => color.ToHex().Chunk(2).Take(3).Select(x => new string(x)).Distinct().Count() == 1;

			// Only anti-alias when the support color (this color of the icons) is a grayscale image
			// This assumes that a larger palette only contains one non-grayscale color, which would make it unsuitable for anti-aiassing.
			var antiAlias = colors.Length >= 4 && isGrayScale(supportColor);

			//Console.WriteLine($"Support color: {supportColor}");
			//Console.WriteLine(string.Join("-", supportColor.ToHex().Chunk(2).Select(x => new string(x))));
			//Console.WriteLine(string.Join("-", supportColor.ToHex().Chunk(2).Take(3).Select(x => new string(x))));
			//Console.WriteLine(string.Join("-", supportColor.ToHex().Chunk(2).Take(3).Select(x => new string(x)).Distinct()));
			foreach (var color in colors)
				Trace.TraceInformation($"Color: {color}");

			Trace.TraceInformation($"Multiple colors palette ({colors.Length}), but support color is a gray scale (consists of {supportColor.ToHex().Chunk(2).Take(3).Select(x => new string(x)).Distinct().Count()} the same component(s)), anti-aliassing: {antiAlias}");

			var textOptions = new RichTextOptions(textFont)
			{
				WrappingLength = width,
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				Dpi = 96
			};
			var weatherTextOptions = new RichTextOptions(weatherFont)
			{
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				WrappingLength = width,
				Dpi=96
			};

			Weather.RootObject weather;

			using (MiniProfiler.Current.Step($"Get weather for '{cacheKey.City}'"))
				try
				{
					using var util = new Weather.Util(cacheKey.Token);
					weather = await util.GetForeCast(cacheKey.City);
				}
				catch (Weather.WeatherApiRequestFailureException ex)
				{
					var explanation = "Weather service indicated API authentication failure failure";
					log?.Invoke(ex, true, explanation);

					result.Mutate(context =>
					{
						var y = 100;
						context
							.RenderErrorMessage(
								explanation,
								errorColor, backgroundColor, ref y, width, textOptions);
					});
					return result;
				}
				catch (Exception ex)
				{
					log?.Invoke(ex, true, "Weather service responds with unauthorized");

					result.Mutate(context =>
					{
						var y = 100;
						context
							.RenderErrorMessage(
								ex.Message.ToString(),
								errorColor, backgroundColor, ref y, width, textOptions);
					});
					return result;
				}

			var station = weather?.city;

			using (MiniProfiler.Current.Step("Draw weather panel"))
				result.Mutate(context =>
			{
				context.GetGraphicsOptions().Antialias = false;

				var y = 0;
				var locationInfo = $"{station?.name},{station?.country}";
				context.DrawText(
					new(textOptions) { Origin = new PointF(5, y) }, locationInfo.ToSafeChars(textFont), primaryColor);
				y += (int)Math.Ceiling(TextMeasurer.MeasureSize(locationInfo, textOptions).Height);

				try
				{
					if (weather is null)
						context.DrawText(new(textOptions) { Origin = new PointF(0, y) }, $"No weather found for {station?.name}", primaryColor);
					else
					{
						var firstIcon = (weather.list.OrderBy(l => l.dt).FirstOrDefault()?.weather.FirstOrDefault())?.icon;
						var weatherIconDimensions = TextMeasurer.MeasureSize(firstIcon, weatherTextOptions);

						var allIcons = string.Join("", FontHelper.WeatherIconsMap.Values);
						var weatherIconLineDimensions = TextMeasurer.MeasureSize(allIcons, weatherTextOptions);

						var widthPerIcon = (int)Math.Ceiling(weatherIconDimensions.Width);
						var heightPerIcon = (int)Math.Ceiling(weatherIconLineDimensions.Height);
						var iconPadding = 5;
						var icons = (int)Math.Floor((decimal)width / (widthPerIcon + 2 * iconPadding));

						var indentPerIcon = (int)Math.Ceiling(((double)width + iconPadding) / icons);

						var x = 0;
						foreach (var forecast in weather.list.OrderBy(l => l.dt).Take(icons))
						{

							if (FontHelper.WeatherIconsMap.TryGetValue(forecast.weather.FirstOrDefault()?.icon, out var icon))
								context.DrawText(
									new (weatherTextOptions) { Origin = new PointF(x + iconPadding, y) },
									icon,
									supportColor
									);
							else
								context.DrawText(
									new(textOptions) { Origin= new PointF(x, y) },
									$"{forecast.weather.FirstOrDefault()?.icon} has no icon mapping",
									errorColor);

							context.DrawText(
								new(textOptions) { Origin = new PointF(x + iconPadding, y + heightPerIcon + iconPadding) },
								$@"{forecast.Date.ToLocalTime():HH:mm}
{forecast.weather.FirstOrDefault()?.main}",
								primaryColor);

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
						errorColor, backgroundColor, ref y, width, textOptions);
				}
			});

			if (antiAlias)
				result.Mutate(context =>
				{
					context.Quantize(new PaletteQuantizer(colors));
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

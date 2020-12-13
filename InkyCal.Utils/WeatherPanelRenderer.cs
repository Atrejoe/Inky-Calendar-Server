using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
//using ceTe.DynamicPDF.PageElements;
//using ceTe.DynamicPDF.Rasterizer;
using ImageMagick;
using InkyCal.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using SixLabors.Primitives;
using StackExchange.Profiling;

namespace InkyCal.Utils
{

	/// <summary>
	/// /
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="InkyCal.Utils.PanelRenderer{T}" />
	public abstract class PdfRenderer<T> : PanelRenderer<T> where T : Panel { }

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="InkyCal.Models.Panel" />
	public class CurrentDatePanel : Panel
	{
		/// <summary>
		/// Gets the date.
		/// </summary>
		/// <value>
		/// The date.
		/// </value>
		[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
		public DateTime Date { get { return DateTime.Now; } set { } }
	}

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="InkyCal.Utils.CurrentDatePanel" />
	public class NewYorkTimesPanel : CurrentDatePanel
	{
	}

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="InkyCal.Utils.PdfRenderer{NewYorkTimesPanel}" />
	public class NewYorkTimesRenderer : PdfRenderer<NewYorkTimesPanel>
	{
		/// <summary>
		/// Gets the date.
		/// </summary>
		/// <value>
		/// The date.
		/// </value>
		public DateTime Date { get; private set; } = DateTime.Now.AddDays(0);


		private static readonly HttpClient client = new HttpClient();

		/// <summary>
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="colors"></param>
		/// <param name="log"></param>
		/// <returns></returns>
		/// <inheritdoc />
		public override async Task<Image> GetImage(int width, int height, SixLabors.ImageSharp.Color[] colors, IPanelRenderer.Log log)
		{
			var url = new Uri($"https://static01.nyt.com/images/{Date:yyyy}/{Date:MM}/{Date:dd}/nytfrontpage/scan.pdf");
			Trace.WriteLine(url.ToString());
			var pdf = await client.GetByteArrayAsync(url.ToString());

			var settings = new MagickReadSettings();
			// Settings the density to 300 dpi will create an image with a better quality
			settings.Density = new Density(72);
			settings.Format = MagickFormat.Pdf;




			using (var images = new MagickImageCollection())
			{
				// Add all the pages of the pdf file to the collection
				images.Read(pdf, settings);

				using var ms = new MemoryStream();
				// Create new image that appends all the pages horizontally
				using (var horizontal = images.AppendHorizontally())
				{
					// Save result as a png
					horizontal.Write(ms, MagickFormat.Png);
				}
				ms.Position = 0;

				//Load PNG
				var image = Image.Load(ms, new PngDecoder());
				var colorsExtended = new List<SixLabors.ImageSharp.Color>(colors);
				foreach(float i in Enumerable.Range(0,10))
					colorsExtended.Add(Color.Black.WithAlpha(i/10));

				//colorsExtended.Add(Color.Gray);
				//colorsExtended.Add(Color.DarkGray);
				//colorsExtended.Add(Color.DarkSlateGray);
				//colorsExtended.Add(Color.DimGray);
				//colorsExtended.Add(Color.LightGray);
				//colorsExtended.Add(Color.LightSlateGray);
				//colorsExtended.Add(Color.SlateGray,);


				image.Mutate(x => x
					.Resize(new ResizeOptions() { Mode = ResizeMode.Max, Size = new Size(width, height) })
					.BackgroundColor(SixLabors.ImageSharp.Color.Transparent)
					.Quantize(new PaletteQuantizer(colorsExtended.ToArray(),false)));
				return image;
			}


			//using (var msPdf = new MemoryStream(pdf))
			//using (var document = new Aspose.Pdf.Document(msPdf))
			//using (var input = new InputPdf(pdf))
			//using (var rasterizer = new PdfRasterizer(input))
			{
				//using var ms = new MemoryStream();
				//document.Save(ms, (Aspose.Pdf.SaveFormat.j)13);
				//using (var tmpFile = File.Create("temp.png"))
				//{
				//	rasterizer.Draw(tmpFile.Name, ImageFormat.Png, ImageSize.Dpi72);

				//	//using (var reader = new PdfReader(await (await client.GetAsync(url)).Content.ReadAsStreamAsync()))
				//	//using (PdfDocument origPdf = new PdfDocument(reader))
				//	//{
				//	//	PdfPage origPage = origPdf.GetPage(1);
				//	//	origPage.conv

				//	//	using (var msTemp = new MemoryStream())
				//	//	{
				//	//		using(var writer = new PdfWriter(msTemp))
				//	//		using (var doc = new PdfDocument(writer))
				//	//		{
				//	//			var pageCopy = origPage.CopyAsFormXObject(doc);

				//	//			var img = new iText.Layout.Element.Image(pageCopy);
				//	//		}
				//	//	}

				//	//	PdfToImageRenderer


				//	//}

					////Load PNG
					//return Image.Load(ms);
				//}
			}
		}

		/// <summary>
		/// Reads the configuration.
		/// </summary>
		/// <param name="panel">The panel.</param>
		protected override void ReadConfig(NewYorkTimesPanel panel)
		{
			if (panel is null)
				throw new ArgumentNullException(nameof(panel));

			Date = panel.Date;
		}
	}

	/// <summary>
	/// A renderer for weather
	/// </summary>
	public class WeatherPanelRenderer : PanelRenderer<WeatherPanel>
	{
		private string token;
		private string city;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="token"></param>
		/// <param name="city"></param>
		public WeatherPanelRenderer(string token, string city)
		{
			this.token = token;
			this.city = city;
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


			this.token = panel.Token;
			this.city = panel.Location;
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

			var textGraphicsOptions = new TextGraphicsOptions(false)
			{
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				WrapTextWidth = width,
				DpiX = 96,
				DpiY = 96
			};
			var rendererOptions = textGraphicsOptions.ToRendererOptions(textFont);
			var weatherRendererOptions = textGraphicsOptions.ToRendererOptions(weatherFont);

			Weather.RootObject weather;

			using (MiniProfiler.Current.Step($"Get weather for '{city}'"))
				try
				{
					using (var util = new Weather.Util(token))
						weather = await util.GetForeCast(city);
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
	}
}

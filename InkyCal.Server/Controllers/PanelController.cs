using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InkyCal.Models;
using InkyCal.Utils;
using InkyCal.Utils.NewPaperRenderer;
using InkyCal.Utils.NewPaperRenderer.FreedomForum.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp.Processing;

namespace InkyCal.Server.Controllers
{

	/// <summary>
	/// A controller for returning Inkycal panels
	/// </summary>
	/// <seealso cref="ControllerBase" />
	[Route("panel")]
	[ApiController]
	public class PanelController : ControllerBase
	{
		/// <summary>
		/// Returns a demo image, mapped for <paramref name="model"/>.
		/// </summary>
		/// <param name="model"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns>A demo image panel</returns>
		/// <remarks></remarks>
		/// <response code="200">Returns the panel as a PNG image</response>
		[HttpGet("test/{model}/image")]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<ActionResult> Test(DisplayModel model, CancellationToken cancellationToken, [Range(0, 1200)] int? width = null, [Range(0, 1200)] int? height = null)
		{
			return await this.Image(renderer: new TestImagePanelRenderer(),
						   model: model,
						   cancellationToken: cancellationToken,
						   requestedWidth: width,
						   requestedHeight: height);
		}

		/// <summary>
		/// Returns a demo calendar, mapped for <paramref name="model"/>.
		/// </summary>
		/// <param name="model"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns>A demo calendar panel.</returns>
		/// <remarks>
		/// </remarks>
		/// <response code="200">Returns the panel as a PNG image</response>
		[HttpGet("test/{model}/calendar")]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ResponseCache(NoStore = true)]
		public async Task<ActionResult> TestCalendar(DisplayModel model, CancellationToken cancellationToken, [Range(0, 1200)] int? width = null, [Range(0, 1200)] int? height = null)
		{
			return await this.Image(new TestCalendarPanelRenderer(), model, cancellationToken, width, height);
		}

		/// <summary>
		/// Returns a AI-powered impression image for a demo calendar, mapped for <paramref name="model"/>.
		/// </summary>
		/// <param name="model"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns>A demo calendar panel.</returns>
		/// <remarks>
		/// </remarks>
		/// <response code="200">Returns the panel as a PNG image</response>
		[HttpGet("test/{model}/calendar-image")]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ResponseCache(NoStore = true)]
		public async Task<ActionResult> TestCalendarImage(DisplayModel model, CancellationToken cancellationToken, [Range(0, 1200)] int? width = null, [Range(0, 1200)] int? height = null)
		{
			return await this.Image(new TestCalendarImagePanelRenderer(), model, cancellationToken, width, height);
		}

		/// <summary>
		/// Returns a demo weather panel, mapped for <paramref name="model"/>.
		/// </summary>
		/// <param name="model"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns>A demo weather panel (for the city of Rotterdam)</returns>
		/// <remarks>
		/// </remarks>
		/// <response code="200">Returns the panel as a PNG image</response>
		[HttpGet("test/{model}/weather")]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ResponseCache(NoStore = true)]
		public async Task<ActionResult> TestWeather(DisplayModel model, CancellationToken cancellationToken, [Range(0, 1200)] int? width = null, [Range(0, 1200)] int? height = null)
		{
			return await this.Image(new WeatherPanelRenderer(Config.Config.OpenWeatherAPIKey, "Rotterdam, NL"), model, cancellationToken, width, height);
		}

		/// <summary>
		/// Returns a demo weather panel, mapped for <paramref name="model"/>.
		/// </summary>
		/// <param name="model"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns>A demo weather panel (for the city of Rotterdam)</returns>
		/// <remarks>
		/// </remarks>
		/// <response code="200">Returns the panel as a PNG image</response>
		[HttpGet("test/{model}/newspaper")]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ResponseCache(NoStore = true)]
		public async Task<ActionResult> TestNewsPaper(DisplayModel model, CancellationToken cancellationToken, [Range(0, 1200)] int? width = null, [Range(0, 1200)] int? height = null)
		{
			var newsPapers = (await new Utils.NewPaperRenderer.FreedomForum.ApiClient().GetNewsPapers()).Values.ToArray();

			var r = new Random().Next(0, newsPapers.Length);
			var randomNewsPaper = newsPapers[r];
			return await this.Image(new NewsPaperRenderer(randomNewsPaper.PaperId), model, cancellationToken, width, height);
		}

		/// <summary>
		/// Returns a demo weather panel, mapped for <paramref name="model"/>.
		/// </summary>
		/// <param name="model"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns>A demo weather panel (for the city of Rotterdam)</returns>
		/// <remarks>
		/// </remarks>
		/// <response code="200">Returns the panel as a PNG image</response>
		[HttpGet("test/{model}/panel-of-panels")]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ResponseCache(NoStore = true)]
		public async Task<ActionResult> TestPanelOfPanels(DisplayModel model, CancellationToken cancellationToken, [Range(0, 1200)] int? width = null, [Range(0, 1200)] int? height = null)
		{

			var helper = new PanelRenderHelper(new Data.GoogleOAuthRepository().UpdateAccessToken);
			var panels = (new Panel[] {
								new WeatherPanel() { Token = Config.Config.OpenWeatherAPIKey, Location = "Rotterdam, NL" },
								new NewYorkTimesPanel() { },
								new ImagePanel() { Path = TestImagePanelRenderer.DemoImageUrl, Rotation= Rotation.Zero },
								new CalendarPanel(){  CalenderUrls = new []{ new CalendarPanelUrl() { Url = TestCalendarPanelRenderer.PublicHolidayCalenderUrl } }.ToHashSet() }
							}).
								Select(x =>
									new SubPanel() { Panel = x, Ratio = 1 }
							).ToHashSet();

			return await this.Image(
						new PanelOfPanelRenderer(new PanelOfPanels()
						{
							Panels = panels,
							Rotation = Rotation.Zero
						}, helper), model, cancellationToken, width, height);
		}

		/// <summary>
		/// Returns an image panel with a an image from the user-specified url
		/// </summary>
		/// <param name="model"></param>
		/// <param name="imageUrl"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns>An image panel with a an image from the user-specified url</returns>
		/// <remarks>
		/// The hash may be cached.
		/// </remarks>
		/// <response code="200">Returns the panel as a PNG image</response>
		[HttpGet("image/{model}")]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ResponseCache(Location = ResponseCacheLocation.Client, Duration = 60)]
		public async Task<ActionResult> GetImage(DisplayModel model, [NotNull, Required] Uri imageUrl, CancellationToken cancellationToken, int? width = null, int? height = null)
		{
			if (!imageUrl.IsAbsoluteUri || (imageUrl.Scheme != "http" && imageUrl.Scheme != "https"))
				return BadRequest("Image urls must be absolute urls");

			return await this.Image(new ImagePanelRenderer(imageUrl), model, cancellationToken, width, height);
		}

		/// <summary>
		/// Returns a rasterized version of today's New York times
		/// </summary>
		/// <param name="model"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns>Returns a rasterized version of today's New York times</returns>
		/// <response code="200">Returns the panel as a PNG image</response>
		[HttpGet("nyt/{model}")]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ResponseCache(Location = ResponseCacheLocation.Client, Duration = 60)]
		public async Task<ActionResult> GetNewYorkTime(DisplayModel model, CancellationToken cancellationToken, int? width = null, int? height = null)
		{
			return await this.Image(new NewYorkTimesRenderer(), model, cancellationToken, width, height);
		}

		/// <summary>
		/// Returns a rasterized version of today's Newspaper
		/// </summary>
		/// <param name="newspaperid"></param>
		/// <param name="model"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns>Returns a rasterized version of today's newspaper</returns>
		/// <response code="200">Returns the panel as a PNG image</response>
		[HttpGet("newspaper/{newspaperid}/{model}")]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ResponseCache(Location = ResponseCacheLocation.Client, Duration = 60)]
		public async Task<ActionResult> GetNewspaper(string newspaperid, DisplayModel model, CancellationToken cancellationToken, int? width = null, int? height = null)
		{
			return await this.Image(new NewsPaperRenderer(newspaperid), model, cancellationToken, width, height);
		}


		/// <summary>
		/// Returns a list of available newspapers.
		/// TODO: debug: fow now only shows newspapers that are available now, not in the near future or recent past.
		/// </summary>
		/// <returns>Returns a rasterized version of today's newspaper</returns>
		/// <response code="200">Returns the panel as a PNG image</response>
		[HttpGet("newspaper/list")]
		[ProducesResponseType(typeof(NewsPaper[]), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ResponseCache(Location = ResponseCacheLocation.Client, Duration = 60)]
		public async Task<ActionResult> ListNewspapers()
		{
			var client = new Utils.NewPaperRenderer.FreedomForum.ApiClient();

			return Ok(await client.GetNewsPapers());
		}

		/// <summary>
		/// Returns a calendar panel for a single calendar
		/// </summary>
		/// <param name="model"></param>
		/// <param name="calendar">The calendar to obtain</param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="cancellationToken"></param>
		/// <returns>A badge containing the calculated SHA1 hash.</returns>
		/// <remarks>
		/// The hash may be cached.
		/// </remarks>
		/// <response code="200">Returns the panel as a PNG image</response>
		[HttpGet("calendar/{model}/url")]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ResponseCache(NoStore = true)]
		public async Task<ActionResult> GetCalendar(DisplayModel model, [NotNull, Required(AllowEmptyStrings = false)] Uri calendar, CancellationToken cancellationToken, int? width = null, int? height = null)
		{
			ArgumentNullException.ThrowIfNull(calendar);
			ArgumentNullException.ThrowIfNull(width);
			ArgumentNullException.ThrowIfNull(height);

			if (!calendar.IsAbsoluteUri || (calendar.Scheme != "http" && calendar.Scheme != "https"))
				return BadRequest("Calender urls must be absolute urls");

			return await this.Image(
							renderer: new CalendarPanelRenderer(
								saveToken: new Data.GoogleOAuthRepository().UpdateAccessToken,
								iCalUrl: calendar),
							model: model,
							cancellationToken: cancellationToken,
							requestedWidth: width,
							requestedHeight: height);
		}

		/// <summary>
		/// Returns a calendar panel for multiple calendars
		/// </summary>
		/// <param name="model"></param>
		/// <param name="calendars">The calendars to obtain</param>
		/// <param name="cancellationToken"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns>A badge containing the calculated SHA1 hash.</returns>
		/// <remarks>
		/// The hash may be cached.
		/// </remarks>
		/// <response code="200">Returns the panel as a PNG image</response>
		/// <response code="400">Upon invalid input</response>
		[HttpPost("calendar/{model}")]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ResponseCache(NoStore = true)]
		public async Task<ActionResult> GetCalendar(DisplayModel model, [Required(AllowEmptyStrings = false)] Uri[] calendars, CancellationToken cancellationToken, int? width = null, int? height = null)
		{
			if (calendars.ToList().Exists(x => !x.IsAbsoluteUri || (x.Scheme != "http" && x.Scheme != "https")))
				return BadRequest("Calender urls must be absolute urls");

			return await this.Image(
				new CalendarPanelRenderer(
					saveToken: new Data.GoogleOAuthRepository().UpdateAccessToken,
					iCalUrls: calendars,
					calendars: [],
					drawMode: CalenderDrawMode.List), // Draw mode "AI generated image" is only available for authenticated users
				model, cancellationToken, width, height);
		}

		/// <summary>
		/// Returns a weather forecast panel for a single calendar
		/// </summary>
		/// <param name="model"></param>
		/// <param name="token"></param>
		/// <param name="city"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns>A badge containing the calculated SHA1 hash.</returns>
		/// <remarks>
		/// The hash may be cached.
		/// </remarks>
		/// <response code="200">Returns the panel as a PNG image</response>
		[HttpGet("weather/{model}/forecast/{token}/{city}")]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ResponseCache(NoStore = true)]
		public async Task<ActionResult> GetWeather(DisplayModel model, string token, string city, CancellationToken cancellationToken, int? width = null, int? height = null)
		{
			return await this.Image(new WeatherPanelRenderer(token, city), model, cancellationToken, width, height);
		}

		/// <summary>
		/// Returns a panel
		/// </summary>
		/// <param name="id"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="model"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		/// <remarks>
		/// The hash may be cached.
		/// </remarks>
		/// <response code="200">Returns the panel as a PNG image</response>
		[HttpGet("{id}")]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ResponseCache(NoStore = true)]
		public async Task<ActionResult> GetPanel(Guid id, CancellationToken cancellationToken, DisplayModel? model = null, int? width = null, int? height = null)
		{
			var panel = await Data.PanelRepository.Get<Panel>(id: id, markAsAccessed: true);

			if (panel is null)
				return NotFound($"Panel with id {id} not found");

			model ??= panel.Model;

			var helper = new PanelRenderHelper(new Data.GoogleOAuthRepository().UpdateAccessToken);
			var renderer = helper.GetRenderer(panel);

			PerformanceMonitor.Trace($"Rendering panel {id}");

			return await this.Image(
							renderer,
							model: model.Value,
							cancellationToken: cancellationToken,
							requestedWidth: width ?? panel.Width,
							requestedHeight: height ?? panel.Height,
							(RotateMode)panel.Rotation);

		}

		/// <summary>
		/// Returns the nuber of items in the cahce
		/// </summary>
		/// <returns></returns>
		[HttpGet("Cache")]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
		[ResponseCache(NoStore = true)]
		public ActionResult Cache() => StatusCode((int)System.Net.HttpStatusCode.OK, IPanelRendererExtensions.CacheEntries());
	}
}

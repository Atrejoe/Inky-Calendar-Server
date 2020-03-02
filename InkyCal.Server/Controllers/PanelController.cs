using InkyCal.Models;
using InkyCal.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp.Processing;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InkyCal.Server.Controllers
{

	/// <summary>
	/// A controller for retuning Inkycal panels
	/// </summary>
	/// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
	[Route("panel")]
	[ApiController]
	public class PanelController : ControllerBase
	{
		/// <summary>
		/// Returns a demo image, mapped for <paramref name="model"/>.
		/// </summary>
		/// <param name="model"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns>A badge containing the calculated SHA1 hash.</returns>
		/// <remarks>
		/// The hash may be cached.
		/// </remarks>
		/// <response code="200">Returns the panel as a PNG image</response>
		[HttpGet("test/{model}/image")]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<ActionResult> Test(DisplayModel model, [Range(0, 1200)]int? width = null, [Range(0, 1200)]int? height = null)
		{
			return await this.Image(new TestImagePanelRenderer(), model, width, height);
		}

		/// <summary>
		/// Returns a demo calendar, mapped for <paramref name="model"/>.
		/// </summary>
		/// <param name="model"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns>A badge containing the calculated SHA1 hash.</returns>
		/// <remarks>
		/// The hash may be cached.
		/// </remarks>
		/// <response code="200">Returns the panel as a PNG image</response>
		[HttpGet("test/{model}/calendar")]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ResponseCache(NoStore = true)]
		public async Task<ActionResult> TestCalendar(DisplayModel model, [Range(0, 1200)]int? width = null, [Range(0, 1200)]int? height = null)
		{
			return await this.Image(new TestCalendarPanelRenderer(), model, width, height);
		}

		/// <summary>
		/// Returns an image panel with a user-specified url
		/// </summary>
		/// <param name="model"></param>
		/// <param name="imageUrl"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns>A badge containing the calculated SHA1 hash.</returns>
		/// <remarks>
		/// The hash may be cached.
		/// </remarks>
		/// <response code="200">Returns the panel as a PNG image</response>
		[HttpGet("image/{model}")]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ResponseCache(Location = ResponseCacheLocation.Client, Duration = 60)]
		public async Task<ActionResult> GetImage(DisplayModel model, Uri imageUrl, int? width = null, int? height = null)
		{
			return await this.Image(new ImagePanelRenderer(imageUrl), model, width, height);
		}

		/// <summary>
		/// Returns a calendar panel for a single calendar
		/// </summary>
		/// <param name="model"></param>
		/// <param name="calendar">The calendar to obtain</param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns>A badge containing the calculated SHA1 hash.</returns>
		/// <remarks>
		/// The hash may be cached.
		/// </remarks>
		/// <response code="200">Returns the panel as a PNG image</response>
		[HttpGet("calendar/{model}/url")]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ResponseCache(NoStore = true)]
		public async Task<ActionResult> GetCalendar(DisplayModel model, Uri calendar, int? width = null, int? height = null)
		{
			return await this.Image(new CalendarPanelRenderer(calendar), model, width, height);
		}

		/// <summary>
		/// Returns a calendar panel for multiple calendars
		/// </summary>
		/// <param name="model"></param>
		/// <param name="calendars">The calendars to obtain</param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns>A badge containing the calculated SHA1 hash.</returns>
		/// <remarks>
		/// The hash may be cached.
		/// </remarks>
		/// <response code="200">Returns the panel as a PNG image</response>
		[HttpPost("calendar/{model}")]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ResponseCache(NoStore = true)]
		public async Task<ActionResult> GetCalendar(DisplayModel model, Uri[] calendars, int? width = null, int? height = null)
		{
			return await this.Image(new CalendarPanelRenderer(calendars), model, width, height);
		}

		/// <summary>
		/// Returns a calendar panel for multiple calendars
		/// </summary>
		/// <param name="id"></param>
		/// <param name="model"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns>A badge containing the calculated SHA1 hash.</returns>
		/// <remarks>
		/// The hash may be cached.
		/// </remarks>
		/// <response code="200">Returns the panel as a PNG image</response>
		[HttpGet("calendar/{id}")]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ResponseCache(NoStore = true)]
		public async Task<ActionResult> GetCalendar(Guid id, DisplayModel? model = null, int? width = null, int? height = null)
		{
			var panel = await Data.PanelRepository.Get<CalendarPanel>(id: id);

			if (panel is null)
				return NotFound($"Panel with id {id} not found");

			model = model
				?? (DisplayModel?)(int?)panel.Model
				?? DisplayModel.epd_7_in_5_v2_colour;

			var urls = panel.CalenderUrls.Select(x => new Uri(x.Url));

			return await this.Image(
				new CalendarPanelRenderer(
					iCalUrls: urls.ToArray()),
					model: model.Value,
					requestedWidth: width ?? panel.Width,
					requestedHeight: height ?? panel.Height);
		}

		/// <summary>
		/// Returns a panel
		/// </summary>
		/// <param name="id"></param>
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
		public async Task<ActionResult> GetPanel(Guid id, DisplayModel? model = null, int? width = null, int? height = null)
		{
			var panel = await Data.PanelRepository.Get<Panel>(id: id);

			if (panel is null)
				return NotFound($"Panel with id {id} not found");

			model = model
				?? (DisplayModel?)(int?)panel.Model
				?? DisplayModel.epd_7_in_5_v2_colour;

			int? requestedHeight;
			int? requestedWidth;
			switch (panel.Rotation)
			{
				case Rotation.Clockwise:
				case Rotation.CounterClockwise:
					requestedHeight = width ?? panel.Width;
					requestedWidth = height ?? panel.Height;
					break;
				default:
					requestedWidth = width ?? panel.Width;
					requestedHeight = height ?? panel.Height;
					break;
			}

			var renderer = panel.GetRenderer();

			return await this.Image(
							renderer,
							model: model.Value,
							requestedWidth: requestedWidth,
							requestedHeight: requestedHeight,
							(RotateMode)panel.Rotation);

		}
	}
}

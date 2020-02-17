using InkyCal.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

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
        public ActionResult Test(DisplayModel model, [Range(0,1200)]int? width = null, [Range(0, 1200)]int? height = null)
        {
            return this.Image(new ImagePanelDemo(), model, width, height);
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
        public ActionResult TestCalendar(DisplayModel model, [Range(0,1200)]int? width = null, [Range(0, 1200)]int? height = null)
        {
            return this.Image(new TestCalendarPanel(), model, width, height);
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
        public ActionResult GetImage(DisplayModel model, Uri imageUrl, int? width = null, int? height = null)
        {
            return this.Image(new ImagePanel(imageUrl), model, width, height);
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
        public ActionResult GetCalendar(DisplayModel model, Uri calendar, int? width = null, int? height = null)
        {
            return this.Image(new CalendarPanel(calendar), model, width, height);
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
        public ActionResult GetCalendar(DisplayModel model, Uri[] calendars, int? width = null, int? height = null)
        {
            return this.Image(new CalendarPanel(calendars), model, width, height);
        }
    }
}
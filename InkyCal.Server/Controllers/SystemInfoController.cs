using System;
using InkyCal.Utils.Calendar;
using Microsoft.AspNetCore.Mvc;

namespace InkyCal.Server.Controllers
{
	/// <summary>
	/// A controller for returning system information
	/// </summary>
	/// <seealso cref="ControllerBase" />
	[Route("panel")]
	[ApiController]
	public class SystemInfoController : ControllerBase
	{

		/// <summary>
		/// Returns system time zones
		/// </summary>
		/// <returns></returns>
		[HttpGet("TimeZones")]
		public ActionResult<TimeZoneInfo[]> TimeZones() => Ok(TimeZoneInfo.GetSystemTimeZones());


		/// <summary>
		/// Returns the dutch time zone
		/// </summary>
		/// <returns></returns>
		[HttpGet("TimeZones/Dutch")]
		public ActionResult<TimeZoneInfo> DutchTimeZone() => Ok(GoogleCalenderExtensions.DutchTZ);
	}
}

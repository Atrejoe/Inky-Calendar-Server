using System.Threading.Tasks;
using InkyCal.Data;
using Microsoft.AspNetCore.Mvc;

namespace InkyCal.Server.Pages
{
	/// <summary>
	/// 
	/// </summary>
	[Route("google")]
	[ApiController]
	public class GoogleController : Controller
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		[HttpGet("authorize")]
		public async Task<IActionResult> Authorize([FromQuery]string code)
		{
			CalenderPanel.authorizationCode = code;


			await UserRepository.StoreToken(new Models.GoogleOAuthAccess()
			{
				AccessToken = ""
			});

			return Redirect("/fetchdata");
		}
	}
}

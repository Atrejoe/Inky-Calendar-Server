using InkyCal.Data;
using InkyCal.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace InkyCal.Server.Controllers
{
	/// <summary>
	/// 
	/// </summary>
	public class ControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
	{
		/// <summary>
		/// Gets or sets the authentication state task.
		/// </summary>
		/// <value>
		/// The authentication state task.
		/// </value>
		[Microsoft.AspNetCore.Components.CascadingParameter]
		protected Task<AuthenticationState> authenticationStateTask { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <value>
		/// The user manager.
		/// </value>
		[Microsoft.AspNetCore.Components.Inject]
		protected UserManager<IdentityUser> userManager { get; set; }

		private User _authenticatedUser;

		/// <summary>
		/// Gets the authenticated user.
		/// </summary>
		/// <returns></returns>
		internal async Task<User> GetAuthenticatedUser()
		{
			if (_authenticatedUser != null)
				return _authenticatedUser;

			var principal = await authenticationStateTask;

			if (!principal.User.Identity.IsAuthenticated)
				return null;

			var identityUser = await userManager.GetUserAsync(principal.User);

			_authenticatedUser = await identityUser.GetUser();
			return _authenticatedUser;
		}
	}
}

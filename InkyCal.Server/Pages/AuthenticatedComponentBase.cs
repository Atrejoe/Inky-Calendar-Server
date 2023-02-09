using System.Threading.Tasks;
using InkyCal.Models;
using InkyCal.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace InkyCal.Server.Pages
{
	/// <summary>
	/// A base class for panels that need authentication information
	/// </summary>
	/// <seealso cref="ComponentBase" />
	public class AuthenticatedComponentBase: ComponentBase
	{
		/// <summary>
		/// Gets or sets the authentication state task.
		/// </summary>
		/// <value>
		/// The authentication state task.
		/// </value>
		[CascadingParameter]
		protected Task<AuthenticationState> authenticationStateTask { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <value>
		/// The user manager.
		/// </value>
		[Inject]
		protected UserManager<IdentityUser> userManager { get; set; }

		private User _authenticatedUser;

		/// <summary>
		/// Gets the authenticated user.
		/// </summary>
		/// <returns></returns>
		protected async Task<User> GetAuthenticatedUser()
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

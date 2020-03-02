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
	/// <seealso cref="Microsoft.AspNetCore.Components.ComponentBase" />
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

		///// <summary>
		///// Gets the authenticated user.
		///// </summary>
		///// <value>
		///// The authenticated user.
		///// </value>
		//protected Data.User AuthenticatedUser { get; private set; }

		///// <summary>
		///// Method invoked when the component is ready to start, having received its
		///// initial parameters from its parent in the render tree.
		///// Override this method if you will perform an asynchronous operation and
		///// want the component to refresh when that operation is completed.
		///// </summary>
		//protected override async Task OnInitializedAsync()
		//{
		//	await GetAuthenticatedUser();
		//	await base.OnInitializedAsync();
		//}

		private Models.User _authenticatedUser;

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

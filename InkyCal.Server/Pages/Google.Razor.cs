using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google;
using Google.Apis.Oauth2.v2.Data;
using InkyCal.Data;
using InkyCal.Models;
using InkyCal.Utils;
using InkyCal.Utils.Calendar;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace InkyCal.Server.Pages
{
	/// <summary>
	/// A confirmation page for Google OAuth consent
	/// </summary>
	/// <seealso cref="ComponentBase" />
	public partial class Google : AuthenticatedComponentBase
	{
		/// <summary>
		/// 
		/// </summary>
		public List<(Userinfo Profile, GoogleOAuthAccess Token)> Tokens { get; private set; }

		private bool PermissionAlreadyGranted { get; set; }

		[Inject]
		private NavigationManager NavigationManager { get; set; }


		/// <summary>
		/// Method invoked when the component is ready to start, having received its
		/// initial parameters from its parent in the render tree.
		/// Override this method if you will perform an asynchronous operation and
		/// want the component to refresh when that operation is completed.
		/// </summary>
		protected override async Task OnInitializedAsync()
		{
			if (!InkyCal.Server.Config.GoogleOAuth.Enabled)
			{
				NavigationManager.NavigateTo("/");
				return;
			}

			var user = await GetAuthenticatedUser();
			if (user is null)
				return;

			if (QueryHelpers.ParseQuery(NavigationManager.ToAbsoluteUri(NavigationManager.Uri).Query).TryGetValue("code", out var values))
			{
				var code = values[0];

				if (!string.IsNullOrEmpty(code))
				{
					var token = await GoogleOAuth.GetRefreshToken(code);

					if (token == default ||
						string.IsNullOrWhiteSpace(token.RefreshToken))
						PermissionAlreadyGranted = true;
					else
					{
						await new GoogleOAuthRepository().StoreToken(new GoogleOAuthAccess()
						{
							RefreshToken = token.RefreshToken,
							AccessToken = token.AccessToken,
							AccessTokenExpiry = token.AccessTokenExpiry.GetValueOrDefault(DateTime.UtcNow),
							User = user
						});

						NavigationManager.NavigateTo("/google/authorize");
						return;
					}
				}
			}

			var nizzle = await Task.WhenAll((await new GoogleOAuthRepository().GetTokens(user.Id))
						.Select(async x =>
						{
							(Userinfo User, bool Refreshed) p = default;
							try
							{
								p = await GoogleOAuth.GetProfile(x, base.cancellationTokenSource.Token);

								if (p.Refreshed)
									await new GoogleOAuthRepository().UpdateAccessToken(x, base.cancellationTokenSource.Token);
							}
							catch (GoogleApiException ex)
							{
								ex.Log(user, Severity.Warning);
							}

							return (p.User, x);
						}
						));
			Tokens = nizzle.ToList();
		}

		private async Task DeleteToken(int idToken, CancellationToken cancellationToken)
		{
			Console.WriteLine($"Removing token {idToken}");

			//Urls are case-sensitive
			var token = (Tokens.SingleOrDefault(x => x.Token.Id == idToken));
			await GoogleOAuth.RevokeAccessToken(token.Token?.RefreshToken, cancellationToken);

			await new GoogleOAuthRepository().DeleteToken(idToken);
			Tokens.Remove(token);


		}

		private void GetConsent()
		{
			var flow = InkyCal.Utils.Calendar.GoogleOAuth.GoogleAuthorizationCodeFlow.Value;

			var uri = flow.CreateAuthorizationCodeRequest($"{NavigationManager.BaseUri}google/authorize").Build();
			NavigationManager.NavigateTo(uri.ToString());

		}
	}
}

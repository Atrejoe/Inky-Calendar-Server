﻿using System;
using System.Threading.Tasks;
using InkyCal.Models;
using InkyCal.Data;
using InkyCal.Utils;
using Microsoft.AspNetCore.Components;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using System.Text;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.WebUtilities;
using System.Linq;
using InkyCal.Utils.Calendar;
using System.Collections.Generic;
using Google.Apis.Oauth2.v2.Data;
using Google;

namespace InkyCal.Server.Pages
{
	/// <summary>
	/// A confirmation page for Google OAuth consent
	/// </summary>
	/// <seealso cref="Microsoft.AspNetCore.Components.ComponentBase" />
	public partial class Google : AuthenticatedComponentBase
	{
		/// <summary>
		/// 
		/// </summary>
		public List<(Userinfo Profile, GoogleOAuthAccess Token)> Tokens { get; private set; }

		private bool PermissionAlreadyGranted { get; set; }
		private bool FlowCompleted { get; set; }

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

			if (!FlowCompleted && QueryHelpers.ParseQuery(NavigationManager.ToAbsoluteUri(NavigationManager.Uri).Query).TryGetValue("code", out var values))
			{
				var code = values.First();

				if (!string.IsNullOrEmpty(code))
				{
					var token = await GoogleOAuth.GetRefreshToken(code);

					if (token == default ||
						string.IsNullOrWhiteSpace(token.RefreshToken))
						PermissionAlreadyGranted = true;
					else
					{
						await UserRepository.StoreToken(new Models.GoogleOAuthAccess()
						{
							RefreshToken = token.RefreshToken,
							AccessToken = token.AccessToken,
							AccessTokenExpiry = token.AccessTokenExpiry.GetValueOrDefault(DateTime.UtcNow),
							User = user
						});
						FlowCompleted = true;
					}
				}
			}

			var nizzle = await Task.WhenAll((await InkyCal.Data.UserRepository.GetTokens(user.Id))
						.Select(async x =>
						{
							Userinfo p = null;
							try
							{
								p = await GoogleOAuth.GetProfile(x);
							}
							catch (GoogleApiException ex)
							{
								Console.Error.WriteLine(ex.ToString());
							}

							return (p, x);
						}
						));
			Tokens = nizzle.ToList();
		}

		private async Task DeleteToken(int idToken)
		{
			Console.WriteLine($"Removing token {idToken}");

			//Urls are case-sensitive
			var token = (Tokens.SingleOrDefault(x => x.Token.Id == idToken));
			await GoogleOAuth.RevokeAccessToken(token.Token?.RefreshToken);

			await UserRepository.DeleteToken(idToken);
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
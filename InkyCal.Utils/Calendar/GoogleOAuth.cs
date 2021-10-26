using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Services;
using InkyCal.Models;

namespace InkyCal.Utils.Calendar
{
	/// <summary>
	/// 
	/// </summary>
	public static class GoogleOAuth
	{

		/// <summary>
		/// The Google Calendar API service.
		/// </summary>
		private static readonly Lazy<Oauth2Service> Oauth2Service = new Lazy<Oauth2Service>(() => new Oauth2Service(new BaseClientService.Initializer()
		{
			ApplicationName = "Inky Calender server"
		}));

		/// <summary>
		/// Refreshes an access tokjen
		/// </summary>
		/// <param name="refreshToken"></param>
		/// <returns></returns>
		public static async Task<(string AccessToken, DateTimeOffset? AccessTokenExpiry)> RefreshAccessToken(string refreshToken)
		{
			var flow = GoogleAuthorizationCodeFlow.Value;

			try
			{
				var tokenResponse = await flow.RefreshTokenAsync("", refreshToken, CancellationToken.None);

				return (
					tokenResponse.AccessToken,
					tokenResponse.ExpiresInSeconds.HasValue
						? (DateTime?)DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresInSeconds.Value)
						: null);
			}
			catch (TokenResponseException ex) when (ex.Error.Error == "invalid_grant")
			{
				return default;
			}
		}

		/// <summary>
		/// Disconnects a users' access token.
		/// </summary>
		/// <param name="refreshToken"></param>
		/// <returns></returns>
		public static async Task<bool> RevokeAccessToken(string refreshToken)
		{
			var flow = GoogleAuthorizationCodeFlow.Value;

			try
			{
				await flow.RevokeTokenAsync("", refreshToken, CancellationToken.None);

				return true;
			}
			catch (TokenResponseException ex)
			{
				ex.Log();
				return false;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="authorizationCode"></param>
		/// <returns></returns>
		public static async Task<(string RefreshToken, string AccessToken, DateTimeOffset? AccessTokenExpiry)> GetRefreshToken(string authorizationCode)
		{
			var flow = GoogleAuthorizationCodeFlow.Value;

			try
			{
				var tokenResponse = await flow.ExchangeCodeForTokenAsync("", authorizationCode, $"{new Uri(Server.Config.GoogleOAuth.Website, Server.Config.GoogleOAuth.InkyCalRoot)}/google/authorize", CancellationToken.None);

				return (
					tokenResponse.RefreshToken,
					tokenResponse.AccessToken,
					tokenResponse.ExpiresInSeconds.HasValue
						? (DateTime?)DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresInSeconds.Value)
						: null);
			}
			catch (TokenResponseException)
			{
				return default;
			}
		}

		/// <summary>
		/// 
		/// </summary>>
		/// <returns></returns>
		public static async Task<Userinfo> GetProfile(this GoogleOAuthAccess token)
		{
			var accessToken = await token.GetAccessToken();
			return (accessToken == default)
					? null
					: await GetProfile(accessToken.AccessToken);
		}

		/// <summary>
		/// 
		/// </summary>>
		/// <returns></returns>
		public static async Task<Userinfo> GetProfile(string token)
		{
			if (token == null)
				return null;

			var request = Oauth2Service.Value.Userinfo.Get();
			request.OauthToken = token;
			return await request.ExecuteAsync();
		}

		/// <summary>
		/// 
		/// </summary>
		public static readonly Lazy<GoogleAuthorizationCodeFlow> GoogleAuthorizationCodeFlow = new Lazy<GoogleAuthorizationCodeFlow>(GetFlow);

		private static GoogleAuthorizationCodeFlow GetFlow()
		{
			string[] Scopes = { 
				CalendarService.Scope.CalendarReadonly,					  //Reading calendar
				Google.Apis.Oauth2.v2.Oauth2Service.Scope.UserinfoEmail,  //Displaying email address
				Google.Apis.Oauth2.v2.Oauth2Service.Scope.UserinfoProfile //Displaying profile information
			};


			var secret = GetSecret();

			// The file token.json stores the user's access and refresh tokens, and is created
			// automatically when the authorization flow completes for the first time.


			var init = new GoogleAuthorizationCodeFlow.Initializer()
			{
				ClientSecrets = secret,
				Scopes = Scopes
			};

			var flow = new GoogleAuthorizationCodeFlow(init);
			return flow;
		}

		private static ClientSecrets GetSecret()
		{
			ClientSecrets secret;

			byte[] byteArray = Encoding.ASCII.GetBytes($@"
{{
	""web"": {{
		""client_id"": ""{Server.Config.GoogleOAuth.ClientId}"",
		""project_id"": ""{Server.Config.GoogleOAuth.ProjectId}"",
		""auth_uri"": ""https://accounts.google.com/o/oauth2/auth"",
		""token_uri"": ""https://oauth2.googleapis.com/token"",
		""auth_provider_x509_cert_url"": ""https://www.googleapis.com/oauth2/v1/certs"",
		""client_secret"": ""{Server.Config.GoogleOAuth.ClientSecret}"",
		""redirect_uris"": [
			""{new Uri(Server.Config.GoogleOAuth.Website, Server.Config.GoogleOAuth.InkyCalRoot)}/google/authorize"",
			""https://localhost:5001/google/authorize"",
			""http://localhost:5000/google/authorize""
		],
		""javascript_origins"": [
			""{Server.Config.GoogleOAuth.Website}"",
			""https://localhost:5001"",
			""http://localhost:5000""
		]
	}}
}}
");
			using var stream = new MemoryStream(byteArray);
			secret = GoogleClientSecrets.FromStream(stream).Secrets;


			return secret;
		}
	}

}

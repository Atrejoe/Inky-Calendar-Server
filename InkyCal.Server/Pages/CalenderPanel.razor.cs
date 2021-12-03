using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using InkyCal.Data;
using InkyCal.Models;
using InkyCal.Utils.Calendar;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace InkyCal.Server.Pages
{
	/// <summary>
	/// Displays a calendar panel
	/// </summary>
	/// <seealso cref="Microsoft.AspNetCore.Components.ComponentBase" />
	public partial class CalenderPanel : AuthenticatedComponentBase
	{
		private CalendarPanelUrl newUrl { get; set; } = new CalendarPanelUrl() { };

		/// <summary>
		/// Gets or sets the selected panel.
		/// </summary>
		/// <value>
		/// The panel.
		/// </value>
		[Parameter]
		public Models.CalendarPanel Panel { get; set; }

		/// <summary>
		/// Gets or sets the navigation manager.
		/// </summary>
		/// <value>
		/// The navigation manager.
		/// </value>
		[Inject]
		public NavigationManager NavigationManager { get; set; }

		private void AddUrl()
		{
			if (Panel.CalenderUrls.Any(x => x.Url.Equals(newUrl.Url, StringComparison.InvariantCulture)))
				return;

			Panel.CalenderUrls.Add(newUrl);
			newUrl = new CalendarPanelUrl() { Url = newUrl.Url };
		}


		private void RemoveUrl(string url)
		{
			Console.WriteLine($"Removing url {url}");

			//Urls are case-sensitive
			Panel.CalenderUrls.RemoveWhere(x => x.Url.Equals(url, StringComparison.InvariantCulture));
		}

		/// <summary>
		/// 
		/// </summary>
		protected override async Task OnInitializedAsync()
		{
			var user = await base.GetAuthenticatedUser();

			var tokens = await UserRepository.GetTokens(user.Id);


			AvailableCalendars = (await GoogleCalenderExtensions.ListGoogleCalendars(tokens).ToListAsync())
									.GroupBy(x => (x.IdToken, x.profile))
									.ToDictionary(
										x => x.Key, x => x.Select(y => y.Calender).ToList());

			SubscribedCalenders = Panel.SubscribedGoogleCalenders
				.Select(x => $"{x.AccessToken}_{x.Calender}")
				.ToList();
		}

		private async void SaveSelection()
		{
			if (Panel == null
				|| Panel.Id == Guid.Empty)
				return;

			await CalenderRepository.SaveSubscribedCalenders(Panel, SubscribedCalenders
																	.Select(x => x.Split("_"))
																	.Select(x => (int.Parse(x[0]), string.Join("_", x.Skip(1))))
																	.ToHashSet());

		}

		/// <summary>
		/// 
		/// </summary>
		public Dictionary<(int IdToken, global::Google.Apis.Oauth2.v2.Data.Userinfo profile), List<global::Google.Apis.Calendar.v3.Data.CalendarListEntry>> AvailableCalendars = new();

		/// <summary>
		/// 
		/// </summary>
		public List<string> SubscribedCalenders { get; set; }

	}
}

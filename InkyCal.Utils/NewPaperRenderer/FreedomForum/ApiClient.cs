using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using InkyCal.Utils.NewPaperRenderer.FreedomForum.Models;

namespace InkyCal.Utils.NewPaperRenderer.FreedomForum
{
	/// <summary>
	/// An API client for <a href="api.freedomforum.org">api.freedomforum.org</a>
	/// </summary>
	public class ApiClient
	{
		private static readonly HttpClient _client = new ();

		/// <summary>
		/// Obtains all newspapers
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		[SuppressMessage("Minor Code Smell", "S1075:URIs should not be hard coded", Justification = "API url is hardcoded (just like the contents of the response)")]
		public async Task<Dictionary<string,NewsPaper>> GetNewsPapers(CancellationToken token = default) {
			var response = await _client.GetAsync("https://api.freedomforum.org/cache/papers.js");
			response.EnsureSuccessStatusCode();
			return (await response.Content.ReadFromJsonAsync<NewsPaper[]>(token)).ToDictionary(x=>x.PaperId);
		}

		/// <summary>
		/// Obtains newspapers in Pdf format
		/// </summary>
		/// <param name="newsPaper"></param>
		/// <param name="date"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task<byte[]> DownloadAsPDF(NewsPaper newsPaper, DateTime? date = null, CancellationToken token = default)
		{
			var response = await _client.GetAsync(newsPaper.PDFUrl(date.GetValueOrDefault(DateTime.UtcNow)));
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadAsByteArrayAsync(token);
		}
	}
}

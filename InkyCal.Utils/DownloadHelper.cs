using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace InkyCal.Utils
{
	/// <summary>
	/// A helper class for downloading files
	/// </summary>
	public static class DownloadHelper {

		/// <summary>
		/// Downloads a date-specific file
		/// </summary>
		/// <param name="urlByDate">Method to obtain the dowload url by date</param>
		/// <param name="startDate">Defaults to <see cref="DateTime.UtcNow"/></param>
		/// <param name="maxDaysToLookBack"></param>
		/// <returns></returns>
		/// <exception cref="DownloadException">When no file was downloaded</exception>
		internal static async Task<byte[]> DownloadFileByDay(Func<DateTime, Uri> urlByDate, DateTime? startDate = null, int maxDaysToLookBack = 5)
		{
			var d = startDate.GetValueOrDefault(DateTime.UtcNow);

			byte[] file = null;

			var tries = 0;

			while (tries <= maxDaysToLookBack
				&& !(file?.Any()).GetValueOrDefault())

			{
				tries += 1;
				var url = urlByDate(d);
				try
				{
					Trace.TraceInformation($"Downloading: {url}");
					file = await url.LoadCachedContent(TimeSpan.FromHours(1));
				}
				catch (HttpRequestException ex) when (
					tries <= maxDaysToLookBack
					&& (!ex.StatusCode.HasValue
					//On some days newspapers may not be available is not available.
					|| new[] { System.Net.HttpStatusCode.NotFound
							 , System.Net.HttpStatusCode.Forbidden
					}.Contains(ex.StatusCode.Value)))
				{
					Console.Error.WriteLine($"Failed ({tries:n0}/{maxDaysToLookBack:n0}) to download from {url}: status code {ex.StatusCode}, error message: {ex.Message}");
					d = d.AddDays(-1);
				}
			}

			if (!(file?.Any()).GetValueOrDefault())
				throw new DownloadException($"Failed to download file by day in {tries:n0} tries.");

			return file;
		}
	}
}

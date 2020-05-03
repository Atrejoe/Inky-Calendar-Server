using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace InkyCal.Utils.Weather
{
	internal class Util : IDisposable
	{
		private readonly HttpClient client = new HttpClient();

		private readonly string apiKey;

		public Util(string apiKey)
		{
			this.apiKey = apiKey;
		}

		public async Task<RootObject> GetForeCast(int cityId) => await getForeCast($"https://api.openweathermap.org/data/2.5/forecast?id={cityId}&appid={apiKey}");
		public async Task<RootObject> GetForeCast(string cityName) => await getForeCast($"https://api.openweathermap.org/data/2.5/forecast?q={cityName}&appid={apiKey}");

		private async Task<RootObject> getForeCast(string url)
		{
			var response = await client.GetStringAsync(url);

			return JsonConvert.DeserializeObject<RootObject>(response);
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					client.Dispose();
				}

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~Util()
		// {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion
	}

	internal static class DateTimeHelper{
		private static readonly DateTime Epoch = new DateTime(1970, 01, 01, 0, 0, 0, DateTimeKind.Utc);

		public static DateTime ToDateTime(this int secondsSinceEpoch)
		{
			return Epoch.AddSeconds(secondsSinceEpoch);
		}
	}



#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

	public class Main
	{
		public double temp { get; set; }
		public double feels_like { get; set; }
		public double temp_min { get; set; }
		public double temp_max { get; set; }
		public int pressure { get; set; }
		public int sea_level { get; set; }
		public int grnd_level { get; set; }
		public int humidity { get; set; }
		public double temp_kf { get; set; }
	}

	public class Weather
	{
		public int id { get; set; }
		public string main { get; set; }
		public string description { get; set; }
		public string icon { get; set; }
	}

	public class Clouds
	{
		public int all { get; set; }
	}

	public class Wind
	{
		public double speed { get; set; }
		public int deg { get; set; }
	}

	public class Sys
	{
		public string pod { get; set; }
	}

	public class List
	{
		public int dt { get; set; }

		public DateTime Date => dt.ToDateTime();

		public Main main { get; set; }
		public List<Weather> weather { get; set; }
		public Clouds clouds { get; set; }
		public Wind wind { get; set; }
		public Sys sys { get; set; }
		public string dt_txt { get; set; }
	}

	public class Coord
	{
		public double lat { get; set; }
		public double lon { get; set; }
	}

	public class City
	{
		public int id { get; set; }
		public string name { get; set; }
		public Coord coord { get; set; }
		public string country { get; set; }
		public int timezone { get; set; }
		public int sunrise { get; set; }
		public int sunset { get; set; }
	}

	public class RootObject
	{
		public string cod { get; set; }
		public int message { get; set; }
		public int cnt { get; set; }
		public List<List> list { get; set; }
		public City city { get; set; }
	}
}

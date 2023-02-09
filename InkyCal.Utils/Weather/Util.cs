using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace InkyCal.Utils.Weather
{

	/// <summary>
	/// A reason for failing to obtain weather data
	/// </summary>
	public enum FailureReason
	{
		/// <summary>
		/// The request to the weather service did not pass authentication
		/// </summary>
		Unauthenticated,

		/// <summary>
		/// The cause of failure has not been determined.
		/// </summary>
		Undetermined
	}

	/// <summary>
	/// <see cref="WeatherApiRequestFailureException" /> is thrown when....
	/// </summary>
	/// <remarks></remarks>
	[Serializable()]
	public class WeatherApiRequestFailureException : Exception
	{

		/// <summary>
		/// 
		/// </summary>
		/// <value>
		/// The reason.
		/// </value>
		public FailureReason Reason { get; } = FailureReason.Undetermined;

		/// <summary>
		/// Initializes a new instance of the <see cref="WeatherApiRequestFailureException" /> class.
		/// </summary>
		/// <remarks>Adhering to coding guideline: http://msdn.microsoft.com/library/ms182151(VS.100).aspx</remarks>
		public WeatherApiRequestFailureException() : this("An exception has occurred.")
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WeatherApiRequestFailureException" /> class.
		/// </summary>
		/// <param name="message">The message that describes the error</param>
		/// <remarks>Adhering to coding guideline: http://msdn.microsoft.com/library/ms182151(VS.100).aspx</remarks>
		public WeatherApiRequestFailureException(string message) : this(message, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WeatherApiRequestFailureException" /> class.
		/// </summary>
		/// <param name="message">The message that describes the error</param>
		/// <param name="reason">The reason of failure</param>
		/// <remarks>Adhering to coding guideline: http://msdn.microsoft.com/library/ms182151(VS.100).aspx</remarks>
		public WeatherApiRequestFailureException(string message, FailureReason reason) : this(message, null)
		{
			Reason = reason;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WeatherApiRequestFailureException" /> class.
		/// </summary>
		/// <param name="message">The message that describes the error</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) is not inner exception is specified.</param>
		/// <remarks>Adhering to coding guideline: http://msdn.microsoft.com/library/ms182151(VS.100).aspx</remarks>
		public WeatherApiRequestFailureException(string message, Exception innerException) : base(message, innerException)
		{
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="WeatherApiRequestFailureException" /> class.
		/// </summary>
		/// <param name="message">The message that describes the error</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) is not inner exception is specified.</param>
		/// <param name="reason">The reason of failure</param>
		/// <remarks>Adhering to coding guideline: http://msdn.microsoft.com/library/ms182151(VS.100).aspx</remarks>
		public WeatherApiRequestFailureException(string message, Exception innerException, FailureReason reason) : this(message, innerException)
		{
			Reason = reason;
		}

		/// <summary>
		/// Constructor for deserialization
		/// </summary>
		/// <remarks>Adhering to coding guideline: http://msdn.microsoft.com/library/ms182151(VS.100).aspx</remarks>
		protected WeatherApiRequestFailureException(
			  System.Runtime.Serialization.SerializationInfo info,
			  System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
		{
			// Implement type-specific serialization constructor logic.
			Reason = ((FailureReason?)info.GetValue("reason", typeof(FailureReason?))).GetValueOrDefault();
		}
	}

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
			var response = await client.GetAsync(url);

			if (response.IsSuccessStatusCode)
			{
				var content = await response.Content.ReadAsStringAsync();
				return JsonConvert.DeserializeObject<RootObject>(content);
			}
			else
				switch (response.StatusCode)
				{
					case System.Net.HttpStatusCode.Unauthorized:
						throw new WeatherApiRequestFailureException(response.ReasonPhrase, FailureReason.Unauthenticated);
					default:
						response.EnsureSuccessStatusCode();
						break;
				}

			return null;

		}

		#region IDisposable Support
		private bool disposedValue; // To detect redundant calls

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
			GC.SuppressFinalize(this);
		}
		#endregion
	}

	internal static class DateTimeHelper
	{
		private static readonly DateTime Epoch = new DateTime(1970, 01, 01, 0, 0, 0, DateTimeKind.Utc);

		public static DateTime ToDateTime(this int secondsSinceEpoch)
		{
			return Epoch.AddSeconds(secondsSinceEpoch);
		}
	}



#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA1707 // Identifiers should not contain underscores : Mapped from OpenWeatherAPI dtos
#pragma warning disable CA2227 // Collection properties should be read only
#pragma warning disable CA1724 // Class anmes should not conflict with namespaces

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

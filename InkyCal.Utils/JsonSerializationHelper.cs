using System.Text.Json;

namespace InkyCal.Utils
{
	/// <summary>
	/// 
	/// </summary>
	public static class JsonSerializationHelper
	{
		/// <summary>
		/// Serializes to json.
		/// </summary>
		/// <param name="subject"></param>
		/// <returns></returns>
		public static string SerializeToJson<T>(this T subject) 
			=> JsonSerializer.Serialize(subject);

		/// <summary>
		/// Serializes from json.
		/// </summary>
		/// <returns></returns>
		public static T Deserialize<T>(string json)
			=> JsonSerializer.Deserialize<T>(json);
	}
}

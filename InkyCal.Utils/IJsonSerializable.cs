using System.Text.Json;

namespace InkyCal.Utils
{
	/// <summary>
	/// Marker interface, indicates that the class can be serialized to JSON, and therefore can use attributes that control, serialization
	/// </summary>
	public interface IJsonSerializable
	{
	}

	/// <summary>
	/// 
	/// </summary>
	public static class IJsonSerializableHelper
	{
		/// <summary>
		/// Serializes to json.
		/// </summary>
		/// <param name="subject"></param>
		/// <returns></returns>
		public static string SerializeToJson<T>(this T subject) where T : IJsonSerializable 
			=> JsonSerializer.Serialize(subject);

		/// <summary>
		/// Serializes from json.
		/// </summary>
		/// <returns></returns>
		public static T Deserialize<T>(string json) where T : IJsonSerializable 
			=> JsonSerializer.Deserialize<T>(json);
	}
}

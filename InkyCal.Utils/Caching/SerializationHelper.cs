using System;
using System.Text;
using System.Text.Json;

namespace InkyCal.Utils.Caching
{
	internal static class SerializationHelper {
		public static string SerializeToBase64<T>(this T obj)
		{
			if (object.Equals(obj, default(T)))
				throw new ArgumentNullException(nameof(obj));

			// Serialize to JSON string
			string jsonString = JsonSerializer.Serialize(obj);

			// Convert to bytes and then to base64
			byte[] bytes = Encoding.UTF8.GetBytes(jsonString);
			return Convert.ToBase64String(bytes);
		}

		public static T DeserializeFromBase64<T>(string base64String)
		{
			if (string.IsNullOrEmpty(base64String))
				throw new ArgumentNullException(nameof(base64String));

			// Convert from base64 to bytes
			byte[] bytes = Convert.FromBase64String(base64String);

			// Convert bytes to JSON string
			string jsonString = Encoding.UTF8.GetString(bytes);

			// Deserialize from JSON
			return JsonSerializer.Deserialize<T>(jsonString)
				?? throw new InvalidOperationException("Deserialization resulted in null");
		}
	}
}

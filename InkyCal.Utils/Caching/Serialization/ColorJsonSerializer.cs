using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace InkyCal.Utils.Caching.Serialization
{

	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="System.Text.Json.Serialization.JsonConverter{T}" />
	public class ColorJsonSerializer : JsonConverter<Color[]>
	{
		/// <summary>
		/// Reads and converts the JSON to type <see cref="Color"/>.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="typeToConvert">The type to convert.</param>
		/// <param name="options">An object that specifies serialization options to use.</param>
		/// <returns>
		/// The converted value.
		/// </returns>
		/// <exception cref="NotImplementedException">Deserialization not implemented for Color[] with ExternalTypeArrayConverter</exception>
		public override Color[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			 

			if (reader.TokenType == JsonTokenType.StartArray) {
				
				var colorsAsStrings = new List<Color>();

				while (reader.Read())
				{
					if (reader.TokenType == JsonTokenType.EndArray)
						break;

					colorsAsStrings.Add(new Color(Rgba32.ParseHex(reader.GetString())));
				}

				return colorsAsStrings.ToArray();
			}

			throw new NotImplementedException("Deserialization only possible for array of hex color values");
		}

		/// <summary>
		/// Writes colors to hex values in JSON array
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="value">The value to convert to JSON.</param>
		/// <param name="options">An object that specifies serialization options to use.</param>
		public override void Write(Utf8JsonWriter writer, Color[] value, JsonSerializerOptions options)
		{
			writer.WriteStartArray();
			foreach (var item in value)
			{
				// Custom serialization per item
				//writer.WriteStartObject();
				writer.WriteStringValue(item.ToHex());
				//writer.WriteEndObject();
			}
			writer.WriteEndArray();
		}
	}
}

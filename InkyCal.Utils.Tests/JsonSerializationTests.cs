using System;
using System.Text.Json;
using InkyCal.Models;
using InkyCal.Utils;
using SixLabors.ImageSharp;
using Xunit;
using Xunit.Abstractions;

namespace InkyCal.Utils.Tests
{
	/// <summary>
	/// Tests JSON serialization for classes implementing <see cref="IJsonSerializable"/>
	/// </summary>
	public class JsonSerializationTests
	{
		private readonly ITestOutputHelper output;

		public JsonSerializationTests(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		public void ImageSettings_SerializeToJson_ProducesValidJson()
		{
			// Arrange
			var colors = new[] { Color.Black, Color.White };
			var imageSettings = new ImageSettings(800, 600, colors);

			// Act
			var json = imageSettings.SerializeToJson();

			// Assert
			Assert.NotNull(json);
			Assert.NotEmpty(json);
			output.WriteLine($"Serialized ImageSettings: {json}");

			// Verify it's valid JSON by deserializing
			var deserialized = IJsonSerializableHelper.Deserialize<ImageSettings>(json);
			Assert.NotNull(deserialized);
			Assert.Equal(800, deserialized.Width);
			Assert.Equal(600, deserialized.Height);
		}

		[Fact]
		public void ImageSettings_SerializeToJson_WithMultipleColors_ProducesValidJson()
		{
			// Arrange
			var colors = new[] { Color.Black, Color.White, Color.Red, Color.Yellow };
			var imageSettings = new ImageSettings(1024, 768, colors);

			// Act
			var json = imageSettings.SerializeToJson();

			// Assert
			Assert.NotNull(json);
			Assert.NotEmpty(json);
			output.WriteLine($"Serialized ImageSettings with multiple colors: {json}");

			var deserialized = IJsonSerializableHelper.Deserialize<ImageSettings>(json);
			Assert.NotNull(deserialized);
			Assert.Equal(1024, deserialized.Width);
			Assert.Equal(768, deserialized.Height);
			Assert.Equal(4, deserialized.Colors.Length);
		}

		[Fact]
		public void ImageCacheKey_SerializeToJson_ProducesValidJson()
		{
			// Arrange
			var panelCacheKey = new PanelInstanceCacheKey(Guid.NewGuid(), TimeSpan.FromMinutes(5));
			var colors = new[] { Color.Black, Color.White };
			var imageSettings = new ImageSettings(800, 600, colors);
			var imageCacheKey = new ImageCacheKey(panelCacheKey, imageSettings);

			// Act
			var json = imageCacheKey.SerializeToJson();

			// Assert
			Assert.NotNull(json);
			Assert.NotEmpty(json);
			output.WriteLine($"Serialized ImageCacheKey: {json}");

			// Verify it's valid JSON
			var doc = JsonDocument.Parse(json);
			Assert.NotNull(doc);
		}

		[Fact]
		public void ImageCacheKey_WithPanelInstanceCacheKey_SerializeToJson_ProducesValidJson()
		{
			// Arrange
			var guid = Guid.Parse("12345678-1234-1234-1234-123456789012");
			var panelCacheKey = new PanelInstanceCacheKey(guid, TimeSpan.FromSeconds(30));
			var colors = new[] { Color.Red, Color.Green, Color.Blue };
			var imageSettings = new ImageSettings(640, 480, colors);
			var imageCacheKey = new ImageCacheKey(panelCacheKey, imageSettings);

			// Act
			var json = imageCacheKey.SerializeToJson();

			// Assert
			Assert.NotNull(json);
			Assert.NotEmpty(json);
			output.WriteLine($"Serialized ImageCacheKey with PanelInstanceCacheKey: {json}");

			var doc = JsonDocument.Parse(json);
			Assert.NotNull(doc);
			Assert.True(doc.RootElement.TryGetProperty("PanelCacheKey", out var _));
			Assert.True(doc.RootElement.TryGetProperty("ImageSettings", out var _));
		}

		[Theory]
		[InlineData(100, 100)]
		[InlineData(800, 600)]
		[InlineData(1920, 1080)]
		[InlineData(640, 384)]
		public void ImageSettings_SerializeToJson_WithVariousDimensions_ProducesValidJson(int width, int height)
		{
			// Arrange
			var colors = new[] { Color.Black, Color.White };
			var imageSettings = new ImageSettings(width, height, colors);

			// Act
			var json = imageSettings.SerializeToJson();

			// Assert
			Assert.NotNull(json);
			Assert.NotEmpty(json);
			output.WriteLine($"Serialized ImageSettings ({width}x{height}): {json}");

			var deserialized = IJsonSerializableHelper.Deserialize<ImageSettings>(json);
			Assert.NotNull(deserialized);
			Assert.Equal(width, deserialized.Width);
			Assert.Equal(height, deserialized.Height);
		}

		[Fact]
		public void ImageSettings_SerializeToJson_IncludesAllProperties()
		{
			// Arrange
			var colors = new[] { Color.Black, Color.White, Color.Red };
			var imageSettings = new ImageSettings(1024, 768, colors);

			// Act
			var json = imageSettings.SerializeToJson();

			// Assert
			output.WriteLine($"Serialized ImageSettings: {json}");
			
			var doc = JsonDocument.Parse(json);
			Assert.True(doc.RootElement.TryGetProperty("Width", out var widthElement));
			Assert.Equal(1024, widthElement.GetInt32());
			
			Assert.True(doc.RootElement.TryGetProperty("Height", out var heightElement));
			Assert.Equal(768, heightElement.GetInt32());
			
			Assert.True(doc.RootElement.TryGetProperty("Colors", out var colorsElement));
			Assert.Equal(JsonValueKind.Array, colorsElement.ValueKind);
		}

		[Fact]
		public void ImageCacheKey_SerializeToJson_IncludesNestedObjects()
		{
			// Arrange
			var guid = Guid.NewGuid();
			var panelCacheKey = new PanelInstanceCacheKey(guid, TimeSpan.FromMinutes(10));
			var colors = new[] { Color.Black, Color.White };
			var imageSettings = new ImageSettings(800, 600, colors);
			var imageCacheKey = new ImageCacheKey(panelCacheKey, imageSettings);

			// Act
			var json = imageCacheKey.SerializeToJson();

			// Assert
			output.WriteLine($"Serialized ImageCacheKey: {json}");
			
			var doc = JsonDocument.Parse(json);
			Assert.True(doc.RootElement.TryGetProperty("PanelCacheKey", out var panelCacheKeyElement));
			Assert.Equal(JsonValueKind.Object, panelCacheKeyElement.ValueKind);
			
			Assert.True(doc.RootElement.TryGetProperty("ImageSettings", out var imageSettingsElement));
			Assert.Equal(JsonValueKind.Object, imageSettingsElement.ValueKind);
		}

		[Fact]
		public void ImageSettings_SerializeToJson_WithGrayscaleColors_ProducesValidJson()
		{
			// Arrange
			var colors = new[] { Color.Black, Color.White, Color.Gray };
			var imageSettings = new ImageSettings(640, 480, colors);

			// Act
			var json = imageSettings.SerializeToJson();

			// Assert
			Assert.NotNull(json);
			Assert.NotEmpty(json);
			output.WriteLine($"Serialized ImageSettings with grayscale: {json}");

			var deserialized = IJsonSerializableHelper.Deserialize<ImageSettings>(json);
			Assert.NotNull(deserialized);
			Assert.Equal(3, deserialized.Colors.Length);

			Assert.Equal(imageSettings, deserialized);
		}

		[Fact]
		public void ImageCacheKey_SerializeToJson_IsConsistentAcrossMultipleCalls()
		{
			// Arrange
			var guid = Guid.Parse("12345678-1234-1234-1234-123456789012");
			var panelCacheKey = new PanelInstanceCacheKey(guid, TimeSpan.FromSeconds(30));
			var colors = new[] { Color.Black, Color.White };
			var imageSettings = new ImageSettings(800, 600, colors);
			var imageCacheKey = new ImageCacheKey(panelCacheKey, imageSettings);

			// Act
			var json1 = imageCacheKey.SerializeToJson();
			var json2 = imageCacheKey.SerializeToJson();

			// Assert
			Assert.Equal(json1, json2);
			output.WriteLine($"Serialized ImageCacheKey (call 1): {json1}");
			output.WriteLine($"Serialized ImageCacheKey (call 2): {json2}");
		}

		[Fact]
		public void ImageSettings_SerializeToJson_IsConsistentAcrossMultipleCalls()
		{
			// Arrange
			var colors = new[] { Color.Black, Color.White };
			var imageSettings = new ImageSettings(800, 600, colors);

			// Act
			var json1 = imageSettings.SerializeToJson();
			var json2 = imageSettings.SerializeToJson();

			// Assert
			Assert.Equal(json1, json2);
			output.WriteLine($"Serialized ImageSettings (call 1): {json1}");
			output.WriteLine($"Serialized ImageSettings (call 2): {json2}");
		}

		[Fact]
		public void ImageCacheKey_SerializeToJson_WithComplexPanelCacheKey_ProducesValidJson()
		{
			// Arrange
			var guid = Guid.NewGuid();
			var panelCacheKey = new PanelInstanceCacheKey(guid, TimeSpan.FromHours(2));
			var colors = new[] { Color.ParseHex("#FF0000"), Color.ParseHex("#00FF00"), Color.ParseHex("#0000FF") };
			var imageSettings = new ImageSettings(1920, 1080, colors);
			var imageCacheKey = new ImageCacheKey(panelCacheKey, imageSettings);

			// Act
			var json = imageCacheKey.SerializeToJson();

			// Assert
			Assert.NotNull(json);
			Assert.NotEmpty(json);
			output.WriteLine($"Serialized complex ImageCacheKey: {json}");

			var doc = JsonDocument.Parse(json);
			Assert.NotNull(doc);
		}

		[Fact]
		public void ImageSettings_SerializeToJson_WithSingleColor_ProducesValidJson()
		{
			// Arrange
			var colors = new[] { Color.Black };
			var imageSettings = new ImageSettings(100, 100, colors);

			// Act
			var json = imageSettings.SerializeToJson();

			// Assert
			Assert.NotNull(json);
			Assert.NotEmpty(json);
			output.WriteLine($"Serialized ImageSettings with single color: {json}");

			var deserialized = IJsonSerializableHelper.Deserialize<ImageSettings>(json);
			Assert.Equal(deserialized, imageSettings);
			Assert.Single(deserialized.Colors);
		}
	}
}

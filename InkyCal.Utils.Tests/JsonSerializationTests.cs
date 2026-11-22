using System;
using System.Text.Json;
using InkyCal.Models;
using InkyCal.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
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

		#region ImageSettings Tests

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
			var deserialized = JsonSerializer.Deserialize<ImageSettings>(json);
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

			var deserialized = JsonSerializer.Deserialize<ImageSettings>(json);
			Assert.NotNull(deserialized);
			Assert.Equal(1024, deserialized.Width);
			Assert.Equal(768, deserialized.Height);
			Assert.Equal(4, deserialized.Colors.Length);
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

			var deserialized = JsonSerializer.Deserialize<ImageSettings>(json);
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

			var deserialized = JsonSerializer.Deserialize<ImageSettings>(json);
			Assert.NotNull(deserialized);
			Assert.Equal(3, deserialized.Colors.Length);
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

			var deserialized = JsonSerializer.Deserialize<ImageSettings>(json);
			Assert.NotNull(deserialized);
			Assert.Single(deserialized.Colors);
		}

		#endregion

		#region ImageCacheKey Tests

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
			Assert.True(doc.RootElement.TryGetProperty("PanelCacheKey", out var panelCacheKeyElement));
			Assert.Equal(JsonValueKind.Object, panelCacheKeyElement.ValueKind);
			Assert.True(doc.RootElement.TryGetProperty("ImageSettings", out var imageSettingsElement));
			Assert.Equal(JsonValueKind.Object, imageSettingsElement.ValueKind);
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

		#endregion

		#region PanelCacheKey Derived Classes Tests

		[Fact]
		public void PanelInstanceCacheKey_SerializeToJson_ProducesValidJson()
		{
			// Arrange
			var guid = Guid.Parse("12345678-1234-1234-1234-123456789012");
			var cacheKey = new PanelInstanceCacheKey(guid, TimeSpan.FromMinutes(5));

			// Act
			var json = JsonSerializer.Serialize<PanelCacheKey>(cacheKey);

			// Assert
			Assert.NotNull(json);
			Assert.NotEmpty(json);
			output.WriteLine($"Serialized PanelInstanceCacheKey: {json}");

			// Verify type discriminator is present
			var doc = JsonDocument.Parse(json);
			Assert.True(doc.RootElement.TryGetProperty("$type", out var typeElement));
			Assert.Equal("Instance", typeElement.GetString());
			Assert.True(doc.RootElement.TryGetProperty("Guid", out _));
		}

		[Fact]
		public void CalendarPanelCacheKey_SerializeToJson_ProducesValidJson()
		{
			// Arrange
			var urls = new[] { new Uri("https://example.com/calendar1.ics"), new Uri("https://example.com/calendar2.ics") };
			var cacheKey = new CalendarPanelCacheKey(TimeSpan.FromMinutes(1), urls, null, CalenderDrawMode.List);

			// Act
			var json = JsonSerializer.Serialize<PanelCacheKey>(cacheKey);

			// Assert
			Assert.NotNull(json);
			Assert.NotEmpty(json);
			output.WriteLine($"Serialized CalendarPanelCacheKey: {json}");

			// Verify type discriminator and properties
			var doc = JsonDocument.Parse(json);
			Assert.True(doc.RootElement.TryGetProperty("$type", out var typeElement));
			Assert.Equal("Calendar", typeElement.GetString());
			Assert.True(doc.RootElement.TryGetProperty("ICalUrls", out _));
			Assert.True(doc.RootElement.TryGetProperty("DrawMode", out _));
		}

		[Fact]
		public void ImagePanelCacheKey_SerializeToJson_ProducesValidJson()
		{
			// Arrange
			var imageUrl = new Uri("https://example.com/image.jpg");
			var cacheKey = new ImagePanelCacheKey(TimeSpan.FromMinutes(1), imageUrl, RotateMode.Rotate90);

			// Act
			var json = JsonSerializer.Serialize<PanelCacheKey>(cacheKey);

			// Assert
			Assert.NotNull(json);
			Assert.NotEmpty(json);
			output.WriteLine($"Serialized ImagePanelCacheKey: {json}");

			// Verify type discriminator and properties
			var doc = JsonDocument.Parse(json);
			Assert.True(doc.RootElement.TryGetProperty("$type", out var typeElement));
			Assert.Equal("Image", typeElement.GetString());
			Assert.True(doc.RootElement.TryGetProperty("ImageUrl", out _));
			Assert.True(doc.RootElement.TryGetProperty("RotateImage", out _));
		}

		[Fact]
		public void NewsPaperPanelCacheKey_SerializeToJson_ProducesValidJson()
		{
			// Arrange
			var cacheKey = new NewsPaperPanelCacheKey(TimeSpan.FromHours(1), "USA_WAPO");

			// Act
			var json = JsonSerializer.Serialize<PanelCacheKey>(cacheKey);

			// Assert
			Assert.NotNull(json);
			Assert.NotEmpty(json);
			output.WriteLine($"Serialized NewsPaperPanelCacheKey: {json}");

			// Verify type discriminator
			var doc = JsonDocument.Parse(json);
			Assert.True(doc.RootElement.TryGetProperty("$type", out var typeElement));
			Assert.Equal("NewsPaper", typeElement.GetString());
		}

		[Fact]
		public void NewYorkTimePanelCacheKey_SerializeToJson_ProducesValidJson()
		{
			// Arrange
			var cacheKey = new NewYorkTimePanelCacheKey(TimeSpan.FromHours(1));

			// Act
			var json = JsonSerializer.Serialize<PanelCacheKey>(cacheKey);

			// Assert
			Assert.NotNull(json);
			Assert.NotEmpty(json);
			output.WriteLine($"Serialized NewYorkTimePanelCacheKey: {json}");

			// Verify type discriminator
			var doc = JsonDocument.Parse(json);
			Assert.True(doc.RootElement.TryGetProperty("$type", out var typeElement));
			Assert.Equal("NewYorkTime", typeElement.GetString());
		}

		[Fact]
		public void PerPanelCacheKey_SerializeToJson_ProducesValidJson()
		{
			// Arrange
			var guid = Guid.Parse("87654321-4321-4321-4321-210987654321");
			var cacheKey = new PerPanelCacheKey(TimeSpan.FromSeconds(30), guid);

			// Act
			var json = JsonSerializer.Serialize<PanelCacheKey>(cacheKey);

			// Assert
			Assert.NotNull(json);
			Assert.NotEmpty(json);
			output.WriteLine($"Serialized PerPanelCacheKey: {json}");

			// Verify type discriminator and properties
			var doc = JsonDocument.Parse(json);
			Assert.True(doc.RootElement.TryGetProperty("$type", out var typeElement));
			Assert.Equal("PanelOfPanel", typeElement.GetString());
			Assert.True(doc.RootElement.TryGetProperty("Id", out _));
		}

		[Fact]
		public void WeatherPanelCacheKey_SerializeToJson_ProducesValidJson()
		{
			// Arrange
			var cacheKey = new WeatherPanelCacheKey(TimeSpan.FromMinutes(1), "test-token", "New York, US");

			// Act
			var json = JsonSerializer.Serialize<PanelCacheKey>(cacheKey);

			// Assert
			Assert.NotNull(json);
			Assert.NotEmpty(json);
			output.WriteLine($"Serialized WeatherPanelCacheKey: {json}");

			// Verify type discriminator
			var doc = JsonDocument.Parse(json);
			Assert.True(doc.RootElement.TryGetProperty("$type", out var typeElement));
			Assert.Equal("Weather", typeElement.GetString());
		}

		[Fact]
		public void PanelCacheKey_Polymorphic_RoundTrip_PanelInstanceCacheKey()
		{
			// Arrange
			var guid = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA");
			PanelCacheKey original = new PanelInstanceCacheKey(guid, TimeSpan.FromMinutes(5));

			// Act
			var json = JsonSerializer.Serialize(original);
			output.WriteLine($"Serialized PanelInstanceCacheKey: {json}");

			var deserialized = JsonSerializer.Deserialize<PanelCacheKey>(json);

			// Assert
			Assert.NotNull(deserialized);
			Assert.IsType<PanelInstanceCacheKey>(deserialized);
			var typedDeserialized = (PanelInstanceCacheKey)deserialized;
			Assert.Equal(guid, typedDeserialized.Guid);
			output.WriteLine($"Round-trip PanelInstanceCacheKey: {json}");
		}

		[Fact]
		public void PanelCacheKey_Polymorphic_RoundTrip_ImagePanelCacheKey()
		{
			// Arrange
			var imageUrl = new Uri("https://example.com/test.jpg");
			PanelCacheKey original = new ImagePanelCacheKey(TimeSpan.FromMinutes(1), imageUrl, RotateMode.Rotate180);

			// Act
			var json = JsonSerializer.Serialize(original);
			var deserialized = JsonSerializer.Deserialize<PanelCacheKey>(json);

			// Assert
			Assert.NotNull(deserialized);
			Assert.IsType<ImagePanelCacheKey>(deserialized);
			var typedDeserialized = (ImagePanelCacheKey)deserialized;
			Assert.Equal(imageUrl, typedDeserialized.ImageUrl);
			Assert.Equal(RotateMode.Rotate180, typedDeserialized.RotateImage);
			output.WriteLine($"Round-trip ImagePanelCacheKey: {json}");
		}

		[Fact]
		public void PanelCacheKey_Polymorphic_RoundTrip_PerPanelCacheKey()
		{
			// Arrange
			var guid = Guid.Parse("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB");
			PanelCacheKey original = new PerPanelCacheKey(TimeSpan.FromSeconds(30), guid);

			// Act
			var json = JsonSerializer.Serialize(original);
			var deserialized = JsonSerializer.Deserialize<PanelCacheKey>(json);

			// Assert
			Assert.NotNull(deserialized);
			Assert.IsType<PerPanelCacheKey>(deserialized);
			var typedDeserialized = (PerPanelCacheKey)deserialized;
			Assert.Equal(guid, typedDeserialized.Id);
			output.WriteLine($"Round-trip PerPanelCacheKey: {json}");
		}

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using InkyCal.Models;
using InkyCal.Utils.Calendar;
using SixLabors.ImageSharp.Formats.Png;
using Xunit;
using Xunit.Abstractions;

namespace InkyCal.Utils.Tests
{
	[Serializable]
	public class EventWrapper : Event, IXunitSerializable
	{

		public override string ToString()
		{
			return $"[{CalendarName}] {Start ?? (object)"No start"}-{End ?? (object)"No end"} \"{Summary}\"";
		}

		void IXunitSerializable.Deserialize(IXunitSerializationInfo info)
		{
			CalendarName = info.GetValue<string>(nameof(CalendarName));
		}

		void IXunitSerializable.Serialize(IXunitSerializationInfo info)
		{
			info.AddValue(nameof(CalendarName), CalendarName);
			info.AddValue(nameof(Summary), Summary);
			info.AddValue(nameof(Start), Start);
			info.AddValue(nameof(End), End);
		}
	}

	/// <summary>
	/// Tests <see creaf="TestCalendarPanel"/> / <see cref="CalendarPanelRenderer"/>
	/// </summary>
	public sealed class TestCalendarPanelTests : IPanelTests<TestCalendarPanelRenderer>
	{
		protected override TestCalendarPanelRenderer GetRenderer()
		{
			return new TestCalendarPanelRenderer();
		}

		public static IEnumerable<object[]> DisplayModelsAndEvents()
		{

			var result = new List<object[]>();

			var models = Enum.GetValues(typeof(DisplayModel))
				.Cast<DisplayModel>();

			var events = new[] {
				new EventWrapper() {  CalendarName = "Regular calendar name", Summary="Regular Summary" },
				new EventWrapper() {  CalendarName = "ẞpecial calendar", Summary="Regular Summary" },
				new EventWrapper() {  CalendarName = "Regular calendar name", Summary="Specielles sümmary" },
				new EventWrapper() {  CalendarName = "Regular calendar name", Summary="ẞpecielles ẞümmary" },
				new EventWrapper() {  CalendarName = "Chinese event", Summary="我可以处理（但可能无法显示）汉字" },
				new EventWrapper() {  CalendarName = "Arabic event", Summary="يمكنني التعامل مع الأحرف الصينية (ولكن ربما لا أعرضها)" }
				};


			models.ToList().ForEach(m =>
				{
					events.ToList().ForEach(e =>
						{
							result.Add(new object[] { m, e });
						});
				});

			return result;
		}

		[Theory]
		[MemberData(nameof(DisplayModelsAndEvents))]
		public void TestDiaCritics(DisplayModel displayModel, EventWrapper calenderEvent)
		{
			//arrange
			var filename = $"GetImageTest_{typeof(CalendarPanelRenderer).Name}_{displayModel}_Diacritics_{Guid.NewGuid()}.png";
			displayModel.GetSpecs(out var width, out var height, out var colors);

			IPanelRenderer.Log assertNoError = (Exception ex, bool handled, string explanation) =>
			{
				if (handled)
				{
					var errorMessage = $"{explanation ?? "Handled exception"}: {ex.Message}";
					Console.WriteLine(errorMessage);
					throw ex;
				}
				else
				{
					if (!string.IsNullOrEmpty(explanation))
						Console.Error.WriteLine(explanation);
					throw ex;
				}
			};

			//act
			var image = CalendarPanelRenderer.DrawImage(
								width: width,
								height: height,
								colors: colors,
								new List<Event> { calenderEvent },
								string.Empty,
								assertNoError
								);

			var bitmap = image.CloneAs<SixLabors.ImageSharp.PixelFormats.Rgba32>();

			//assert
			Assert.NotNull(image);

			var pixels = Enumerable.Range(0, bitmap.Width - 1)
				.SelectMany(x =>
				{
					return Enumerable.Range(0, bitmap.Height - 1).Select(y => bitmap[x, y]);
				}).ToHashSet();

			var message = $"{pixels.Count:n0} distinct colors in the image ({string.Join(",", pixels.Select(x => x.ToString()))}), a palette of {colors.Length:n0} colors ({string.Join(",", colors.Select(x => x.ToString()))}) was specified.";
			Trace.WriteLine(message);
			Assert.False(pixels.Count > colors.Length, message);

			using var fileStream = File.Create(filename);
			image.Save(fileStream, new PngEncoder());

			var fi = new FileInfo(filename);
			Assert.True(fi.Exists, $"File {fi.FullName} does not exist");

			Trace.WriteLine(fi.FullName);

		}
	}


}

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace InkyCal.Utils.Tests
{
    public abstract class IPanelTests<T> where T:IPanel {

        protected abstract T GetPanel();

        [Fact()]
        public void GetImageTest()
        {
            //arrange
            var panel = GetPanel();
            var filename = $"GetImageTest_{typeof(T).Name}.png";

            //act
            var image = panel.GetImage(400,400, new[] {Color.White, Color.Black, Color.Pink });

            //assert
            Assert.NotNull(image);

            using var fileStream = File.Create(filename);
            image.Save(fileStream, new PngEncoder());

            var fi = new FileInfo(filename);
            Assert.True(fi.Exists,$"File {fi.FullName} does not exist");

            Trace.WriteLine(fi.FullName);

        }
    }

    /// <summary>
    /// Tests <see creaf="ImagePanelDemo"/>
    /// </summary>
    public sealed class ImagePanelTests : IPanelTests<ImagePanelDemo>
    {
        protected override ImagePanelDemo GetPanel()
        {
            return new ImagePanelDemo();
        }
    }

    /// <summary>
    /// Tests <see creaf="TestCalendarPanel"/>
    /// </summary>
    public sealed class TestCalendarPanelTests : IPanelTests<TestCalendarPanel>
    {
        protected override TestCalendarPanel GetPanel()
        {
            return new TestCalendarPanel();
        }
    }
}
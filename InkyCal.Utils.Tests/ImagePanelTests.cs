using Xunit.Abstractions;

namespace InkyCal.Utils.Tests
{
	/// <summary>
	/// Tests <see creaf="ImagePanelDemo"/>
	/// </summary>
	public sealed class ImagePanelTests(ITestOutputHelper output) : IPanelTests<TestImagePanelRenderer>(output)
	{
		protected override TestImagePanelRenderer GetRenderer()
		{
			return new TestImagePanelRenderer();
		}
	}
}

namespace InkyCal.Utils.Tests
{
	/// <summary>
	/// Tests <see creaf="ImagePanelDemo"/>
	/// </summary>
	public sealed class ImagePanelTests : IPanelTests<TestImagePanelRenderer>
	{
		protected override TestImagePanelRenderer GetRenderer()
		{
			return new TestImagePanelRenderer();
		}
	}
}

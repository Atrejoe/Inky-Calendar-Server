namespace InkyCal.Utils.Tests
{

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
}
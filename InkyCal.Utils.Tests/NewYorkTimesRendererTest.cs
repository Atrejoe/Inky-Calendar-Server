namespace InkyCal.Utils.Tests
{
	/// <summary>
	/// Tests <see creaf="NewYorkTimesRenderer"/>
	/// </summary>
	public sealed class NewYorkTimesRendererTest : IPanelTests<NewYorkTimesRenderer>
	{
		protected override NewYorkTimesRenderer GetRenderer()
		{
			return new NewYorkTimesRenderer();
		}
	}
}

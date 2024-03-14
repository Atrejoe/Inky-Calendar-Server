using Xunit.Abstractions;

namespace InkyCal.Utils.Tests
{
	/// <summary>
	/// Tests <see creaf="NewYorkTimesRenderer"/>
	/// </summary>
	public sealed class NewYorkTimesRendererTest(ITestOutputHelper output) : IPanelTests<NewYorkTimesRenderer>(output)
	{
		protected override NewYorkTimesRenderer GetRenderer()
		{
			return new NewYorkTimesRenderer();
		}
	}
}

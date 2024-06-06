using System.Collections.Generic;
using System.Threading.Tasks;
using InkyCal.Models;
using Xunit.Abstractions;

namespace InkyCal.Utils.Tests
{
	/// <summary>
	/// Tests <see creaf="PanelOfPanelRenderer"/>, using <see cref="PanelOfPanels"/> with smae parameters as <see cref="TestCalendarPanelRenderer"/> and <see cref="TestImagePanelRenderer"/>
	/// </summary>
	public sealed class PanelOfPanelRendererTests(ITestOutputHelper output) : IPanelTests<PanelOfPanelRenderer>(output)
	{
		protected override PanelOfPanelRenderer GetRenderer()
		{
			var pp = new PanelOfPanels()
			{
				Panels = new HashSet<SubPanel>(
					 new[] {
						 new SubPanel() {
						Panel = new CalendarPanel() {
										CalenderUrls = new HashSet<CalendarPanelUrl>(
														[
															new CalendarPanelUrl(){
																Url = TestCalendarPanelRenderer.PublicHolidayCalenderUrl
															}
														])
													}
						}
						,
						new SubPanel() {
							Panel = new ImagePanel() { Path = TestImagePanelRenderer.DemoImageUrl}
						}
					 })
			};

			return new PanelOfPanelRenderer(pp, new PanelRenderHelper(async (_,_) => await Task.CompletedTask));
		}
	}
}

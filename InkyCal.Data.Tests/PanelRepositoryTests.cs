using System;
using System.Linq;
using System.Threading.Tasks;
using InkyCal.Models;
using StackExchange.Profiling;
using Xunit;

namespace InkyCal.Data.Tests
{

	public class PanelRepositoryTests : RepositoryTestBase
	{

		public PanelRepositoryTests() : base()
		{
		}

		[Fact()]
		public async Task ToggleStarTest()
		{
			//Arrange
			Panel panel;
			using (MiniProfiler.Current.Step("Get panel"))
			{
				panel = (await UserRepository.GetAll().SkipConnectionException()).First(x => x.Panels.Any()).Panels.First();
			}

			var starred = panel.Starred;

			bool actual;
			using (MiniProfiler.Current.Step("Toggle star"))
			{
				//Act
				actual = await PanelRepository.ToggleStar(panel);
			}

			using (MiniProfiler.Current.Step("Get panel again"))
			{
				panel = await PanelRepository.Get<Panel>(panel.Id);
			}

			//Assert
			Assert.NotNull(panel);
			Assert.NotEqual(starred, actual);
			Assert.Equal(actual, panel.Starred);

		}

		[Fact()]
		public async Task PanelAccessTest()
		{
			//Arrange
			Panel panel;
			using (MiniProfiler.Current.Step("Get panel"))
				panel = (await UserRepository.GetAll().SkipConnectionException()).First(x => x.Panels.Any()).Panels.First();

			var accessCount = panel.AccessCount;
			var accessed = panel.Accessed;
			var modified = panel.Modified;

			//Act
			using (MiniProfiler.Current.Step("Get panel again, marking as accessed"))
				panel = await PanelRepository.Get<Panel>(panel.Id, markAsAccessed: true);

			//Assert
			Assert.NotNull(panel);
			Assert.NotEqual(accessCount, panel.AccessCount);
			Assert.True(accessCount < panel.AccessCount);
			Assert.NotEqual(accessed, panel.Accessed);
			Assert.True(accessed < panel.Accessed);

			//Act again
			var panel2 = await PanelRepository.Get<Panel>(panel.Id, markAsAccessed: false);

			Assert.NotEqual(accessCount, panel2.AccessCount);
			Assert.True(accessCount < panel2.AccessCount);
			Assert.NotEqual(accessed, panel2.Accessed);
			Assert.True(accessed < panel2.Accessed);

			Assert.Equal(panel.AccessCount, panel2.AccessCount);
			Assert.Equal(panel.Accessed, panel2.Accessed);

			// Panel should not have been marked as modified
			Assert.Equal(modified, panel2.Modified);

		}

		[Fact()]
		public async Task TaskUpdateCalendarPanelTest()
		{
			//Arrange
			var panel = await PanelRepository.GetRandom<CalendarPanel>().SkipConnectionException();
			Assert.SkipWhen(panel is null, "Panel is null");
			var previousDateModified = panel.Modified;

			//Act
			var actual = await PanelRepository.Update(panel);

			//Assert
			Assert.NotNull(actual);
			Assert.NotEqual(previousDateModified, actual.Modified);
		}

		[Fact()]
		public async Task TaskUpdateGoogleCalendarPanelTest()
		{
			//Arrange
			var panel = await PanelRepository.GetRandomGoogleCalendarPanel().SkipConnectionException();
			Assert.SkipWhen(panel is null, "panel is null");
			Assert.DoesNotContain(panel.SubscribedGoogleCalenders, x => x.AccessToken is null);

			//Act
			await PanelRepository.Update(panel);

			//Assert
		}

		[Fact()]
		public async Task TaskUpdatePanelOfPanelsTest()
		{
			//Arrange
			var panel = await PanelRepository.GetRandom<PanelOfPanels>().SkipConnectionException();
			Assert.SkipWhen(panel is null, "Panel is null");
			var previousDateModified = panel.Modified;

			//Act
			var actual = await PanelRepository.Update(panel);
			var savedPanel = await PanelRepository.Get<PanelOfPanels>(panel.Id).SkipConnectionException();

			//Assert
			Assert.NotNull(savedPanel);
			Assert.NotNull(actual);
			Assert.NotEqual(previousDateModified, savedPanel.Modified);
			Assert.NotEqual(previousDateModified, actual.Modified);
		}

		[Fact()]
		public async Task TaskUpdateImagePanelTest()
		{
			//Arrange
			var panel = await PanelRepository.GetRandom<ImagePanel>().SkipConnectionException();
			Assert.SkipWhen(panel is null, "Panel is null");
			var previousDateModified = panel.Modified;

			//Act
			var actual = await PanelRepository.Update(panel);

			//Assert
			Assert.NotNull(actual);
			Assert.NotEqual(previousDateModified, actual.Modified);
		}

		[Fact()]
		public async Task ListTest()
		{
			//Arrange
			var user = (await UserRepository.GetAll().SkipConnectionException()).Last(x => x.Panels.Any());

			//Act
			var actual = await PanelRepository.List<Panel>(user);

			//Assert
			Assert.NotEmpty(actual);
			Assert.Equal(user.Panels.Count, actual.Length);
		}

		[Fact()]
		public async Task DeleteTest()
		{
			//Arrange
			var panel = (await PanelRepository.All().SkipConnectionException()).LastOrDefault(x => x is PanelOfPanels);
			Assert.SkipWhen(panel == null, "Panel is null");

			//Act

			await PanelRepository.Delete(panel.Id, panel.Owner.Id);

			//Assert
			Assert.NotNull(panel);
		}

		[Fact()]
		public async Task DeleteInvalidTest()
		{
			await Assert.ThrowsAsync<DalException>(async () =>
			{
				//Arrange
				//Act
				await PanelRepository.Delete(Guid.Empty, int.MaxValue).SkipConnectionException();

				//Assert
			}).SkipConnectionException();
		}
	}
}

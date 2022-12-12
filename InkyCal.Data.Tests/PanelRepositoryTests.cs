using System;
using System.Linq;
using System.Threading.Tasks;
using InkyCal.Models;
using StackExchange.Profiling;
using Xunit;
using Xunit.Abstractions;

namespace InkyCal.Data.Tests
{
	public class PanelRepositoryTests : RepositoryBase
	{

		public PanelRepositoryTests(ITestOutputHelper output) : base(output)
		{
		}

		[Fact()]
		public async void ToggleStarTest()
		{
			//Arrange
			Panel panel;
			using (MiniProfiler.Current.Step("Get panel"))
			{
				panel = (await UserRepository.GetAll()).First(x => x.Panels.Any()).Panels.First();
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
		public async void PanelAccessTest()
		{
			//Arrange
			Panel panel;
			using (MiniProfiler.Current.Step("Get panel"))
				panel = (await UserRepository.GetAll()).First(x => x.Panels.Any()).Panels.First();

			var accessCount = panel.AccessCount;
			var accessed = panel.Accessed;

			//Act
			using (MiniProfiler.Current.Step("Get panel again, marking as accessed"))
				panel = await PanelRepository.Get<Panel>(panel.Id, markAsAccessed:true);

			//Assert
			Assert.NotNull(panel);
			Assert.NotEqual(accessCount ,panel.AccessCount);
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

		}

		[SkippableFact()]
		public async Task TaskUpdateCalendarPanelTest()
		{
			//Arrange
			var panel = await PanelRepository.GetRandom<CalendarPanel>();
			Skip.If(panel is null);

			//Act
			await PanelRepository.Update(panel);

			//Assert
		}

		[SkippableFact()]
		public async Task TaskUpdateGoogleCalendarPanelTest()
		{
			//Arrange
			var panel = await PanelRepository.GetRandomGoogleCalendarPanel();
			Skip.If(panel is null);
			Assert.DoesNotContain(panel.SubscribedGoogleCalenders, x => x.AccessToken is null);

			//Act
			await PanelRepository.Update(panel);

			//Assert
		}

		[SkippableFact()]
		public async Task TaskUpdatePanelOfPanelsTest()
		{
			//Arrange
			var panel = await PanelRepository.GetRandom<PanelOfPanels>();
			Skip.If(panel is null);

			//Act
			await PanelRepository.Update(panel);

			//Assert
		}

		[SkippableFact()]
		public async Task TaskUpdateImagePanelTest()
		{
			//Arrange
			var panel = await PanelRepository.GetRandom<ImagePanel>();
			Skip.If(panel is null);

			//Act
			await PanelRepository.Update(panel);

			//Assert
		}

		[Fact()]
		public async void ListTest()
		{
			//Arrange
			var user = (await UserRepository.GetAll()).Last(x => x.Panels.Any());

			//Act
			var actual = await PanelRepository.List<Panel>(user);

			//Assert
			Assert.NotEmpty(actual);
			Assert.Equal(user.Panels.Count, actual.Length);
		}

		[SkippableFact()]
		public async void DeleteTest()
		{
			//Arrange
			var panel = (await PanelRepository.All()).LastOrDefault(x => x is PanelOfPanels);
			Skip.If(panel == null);

			//Act

			await PanelRepository.Delete(panel.Id);

			//Assert
			Assert.NotNull(panel);
		}

		[Fact()]
		public async void DeleteInvalidTest()
		{
			await Assert.ThrowsAsync<DalException>(async () =>
			{
				//Arrange
				//Act
				await PanelRepository.Delete(Guid.Empty);

				//Assert
			});
		}
	}
}

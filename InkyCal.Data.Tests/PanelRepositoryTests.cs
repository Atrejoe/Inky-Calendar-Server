using System;
using System.Linq;
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

		//[Fact()]
		//public void UpdateTest()
		//{
		//	//Arrange
		//	//Act
		//	//Assert
		//	Assert.True(false, "This test needs an implementation");
		//}

		[Fact()]
		public async void ListTest()
		{
			//Arrange
			var user = (await UserRepository.GetAll()).Last(x=>x.Panels.Any());

			//Act
			var actual = await PanelRepository.List<Panel>(user);

			//Assert
			Assert.NotEmpty(actual);
			Assert.Equal(user.Panels.Count, actual.Length);
		}

		[Fact()]
		public async void DeleteTest()
		{
			//Arrange
			var panel = (await PanelRepository.All()).Last(x=>x is PanelOfPanels);

			//Act
			await PanelRepository.Delete(panel.Id);

			//Assert
			Assert.NotNull(panel);
		}

		[Fact()]
		public async void DeleteInvalidTest()
		{
			await Assert.ThrowsAsync<Exception>(async () =>
			{
				//Arrange
				//Act
				await PanelRepository.Delete(Guid.Empty);

				//Assert
			});
		}
	}
}

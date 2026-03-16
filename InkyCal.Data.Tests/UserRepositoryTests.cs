using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Runner.InProc.SystemConsole;

namespace InkyCal.Data.Tests
{

	public class UserRepositoryTests : RepositoryTestBase
	{

		public UserRepositoryTests()
		{
		}


		[Fact()]
		public async Task GetUserTest()
		{
			//arrange
			//act
			var actual = (await UserRepository.GetAll().SkipConnectionException()).First();

			//assert
			Assert.NotNull(actual);
		}

		[Fact()]
		public async Task GetAllTest()
		{
			//arrange
			//act
			var actual = await UserRepository.GetAll().SkipConnectionException();


			//assert
			Assert.NotNull(actual);
			Console.WriteLine(string.Join(Environment.NewLine, actual.Select(x => x.Id)));

		}
	}
}

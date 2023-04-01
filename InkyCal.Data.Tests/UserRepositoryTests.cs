using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace InkyCal.Data.Tests
{

	public class UserRepositoryTests : RepositoryBase
	{

		public UserRepositoryTests(ITestOutputHelper output) : base(output)
		{
		}


		[SkippableFact()]
		public async Task GetUserTest()
		{
			//arrange
			//act
			var actual = (await UserRepository.GetAll().SkipConnectionException()).First();

			//assert
			Assert.NotNull(actual);
		}

		[SkippableFact()]
		public async Task GetAllTest()
		{
			//arrange
			//act
			var actual = await UserRepository.GetAll().SkipConnectionException();


			//assert
			Assert.NotNull(actual);
			output.WriteLine(string.Join(Environment.NewLine, actual.Select(x => x.Id)));

		}
	}
}

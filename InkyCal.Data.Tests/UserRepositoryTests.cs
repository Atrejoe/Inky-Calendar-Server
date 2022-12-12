using System;
using System.Linq;
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
		public async void GetUserTest()
		{
			//arrange
			//act
			var actual = (await UserRepository.GetAll().SkipConnectionException()).First();

			//assert
			Assert.NotNull(actual);
		}

		[SkippableFact()]
		public async void GetAllTest()
		{
			//arrange
			//act
			var actual = await UserRepository.GetAll().SkipConnectionException();


			//assert
			output.WriteLine(string.Join(Environment.NewLine, actual.Select(x => x.Id)));

		}
	}
}

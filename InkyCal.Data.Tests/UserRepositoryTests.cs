using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace InkyCal.Data.Tests
{
	public class UserRepositoryTests : RepositoryBase
	{

		public UserRepositoryTests(ITestOutputHelper output) : base(output) { 
		}


		[Fact()]
		public async void GetUserTest()
		{
			//arrange
			//act
			var actual = (await UserRepository.GetAll()).First();

			//assert
			Assert.NotNull(actual);
		}

		[Fact()]
		public async void GetAllTest()
		{
			//arrange
			//act
			var actual = await UserRepository.GetAll();

			//assert
			output.WriteLine(string.Join(Environment.NewLine,actual.Select(x=>x.Id)));
		}
	}
}

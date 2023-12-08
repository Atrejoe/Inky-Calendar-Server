using System;
using System.Collections.Generic;
using InkyCal.Models;
using Xunit;
using Xunit.Abstractions;

namespace InkyCal.Utils.Tests
{
	public class DisplayModelHelperTests
	{
		protected readonly ITestOutputHelper output;

		public DisplayModelHelperTests(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Theory()]
		[MemberData(nameof(DisplayModels))]
		public void GetSpecsTest(DisplayModel model)
		{
			//arrang

			//act
			var specs = DisplayModelHelper.GetSpecs(model);

			//assert
			Assert.NotEmpty(specs.Colors);
		}

		public static IEnumerable<object[]> DisplayModels()
		{
			foreach(var value in Enum.GetValues<DisplayModel>())
				yield return new object[] { value };

			
		}
	}
}

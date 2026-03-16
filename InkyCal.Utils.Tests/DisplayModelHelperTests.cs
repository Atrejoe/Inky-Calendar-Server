using System;
using System.Collections.Generic;
using InkyCal.Models;
using Xunit;

namespace InkyCal.Utils.Tests
{
	public class DisplayModelHelperTests
	{

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
			foreach (var value in Enum.GetValues<DisplayModel>())
				yield return new object[] { value };


		}
	}
}

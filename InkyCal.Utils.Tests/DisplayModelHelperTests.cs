using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using InkyCal.Models;
using Microsoft.VisualStudio.TestPlatform.Utilities;
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
			var actual = DisplayModelHelper.GetSpecs(model);

			//assert
			Assert.NotNull(actual);
		}

		public static IEnumerable<object[]> DisplayModels()
		{
			foreach(var value in Enum.GetValues<DisplayModel>())
				yield return new object[] { value };

			
		}
	}
}

using Xunit;
using InkyCal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InkyCal.Models.Tests
{
	public class ColorHelperTests
	{
		[Theory()]
		[InlineData((byte)1)]
		[InlineData((byte)2)]
		[InlineData((byte)3)]
		public void GrayScalesTest(byte levels)
		{
			//arrange

			//act
			var colors = ColorHelper.GrayScales(levels);

			//assert
			Assert.Equal(levels, colors.Length);
		}
	}
}

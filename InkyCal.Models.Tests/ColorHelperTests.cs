using Xunit;

namespace InkyCal.Models.Tests
{
	public class ColorHelperTests
	{
		[Theory()]
		[InlineData((byte)1, (byte)2)]
		[InlineData((byte)2)]
		[InlineData((byte)3)]
		[InlineData((byte)16)]
		public void GrayScalesTest(byte levels, byte? expected = null)
		{
			//arrange

			//act
			var colors = ColorHelper.GrayScales(levels).Distinct().ToArray();

			//assert
			Assert.Equal(expected ?? levels, colors.Length);
		}
	}
}

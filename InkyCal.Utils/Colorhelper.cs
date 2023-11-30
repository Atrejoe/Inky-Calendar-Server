using System.Linq;
using SixLabors.ImageSharp;

namespace InkyCal.Utils
{
	/// <summary>
	/// 
	/// </summary>
	public static class Colorhelper
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		public static string ColorName(this Color color)
		{

			var colors = typeof(Color).GetFields().Where(x => x.FieldType.Equals(typeof(Color))).Select(x => new
			{
				x.Name,
				Color = (Color)x.GetValue(null)
			});

			return colors.FirstOrDefault(x => x.Color.Equals(color))?.Name ?? color.ToString();
		}
	}
}

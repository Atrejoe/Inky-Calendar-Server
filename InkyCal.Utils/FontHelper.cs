using SixLabors.Fonts;
using System;
using System.Reflection;

namespace InkyCal.Utils
{
	/// <summary>
	/// A helper class for font-sizing
	/// </summary>
	public static class FontHelper
	{
		private static readonly FontCollection fonts = new FontCollection();

		/// <summary>
		/// Font family NotoSans-SemiCondensed
		/// </summary>
		public static readonly FontFamily NotoSans;
		/// <summary>
		/// Font family MonteCarloFixed12
		/// </summary>
		public static readonly FontFamily MonteCarlo;

		/// <summary>
		/// 
		/// </summary>
		public static readonly FontFamily GoHu;

		static FontHelper()
		{
            var assembly = typeof(CalendarPanelRenderer).GetTypeInfo().Assembly;

			using (var resource = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.fonts.NotoSans-SemiCondensed.ttf"))
				NotoSans = fonts.Install(resource);

			using (var resource = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.fonts.MonteCarloFixed12.ttf"))
				MonteCarlo = fonts.Install(resource);

			using (var resource = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.fonts.gohufont-14.ttf"))
				GoHu = fonts.Install(resource);

		}

		/// <summary>
		/// Return the width of characters in a fixed-width character font.
		/// </summary>
		/// <param name="font"></param>
		/// <returns></returns>
		public static float? GetDPI(this Font font) {
			if (font.Family.Equals(NotoSans))
				return null;
			if (font.Family.Equals(MonteCarlo))
				return 96;
			if (font.Family.Equals(GoHu))
				return null;

			return null;
		}

		/// <summary>
		/// Return the width of characters in a fixed-width character font.
		/// </summary>
		/// <param name="font"></param>
		/// <returns></returns>
		public static ushort? GetCharacterWidth(this Font font)
		{
			if (font.Family.Equals(NotoSans))
				return null;
			if (font.Family.Equals(MonteCarlo))
				switch (font.Size)
				{
					case 12:
						return 7;
					case 24:
						return 7;
					default:
						return null;
				}
			if (font.Family.Equals(GoHu))
				switch (font.Size)
				{
					case 14:
						return 7;
					default:
						return null;
				}
			else
				return null;
		}
	}
}

using UnityEngine;

namespace Engine.Utilities
{
	public static class ColorUtils
	{
		private static readonly Color purple	= new Color(.5f, 0f, .5f);
		private static readonly Color magenta	= new Color(1f, 0f, 1f);
		private static readonly Color lilac = new Color(.5f, 0f, 1f);

		private static readonly Color darkblue = new Color(0f, 0f, .5f);
		private static readonly Color blue		= new Color(0f, 0f, 1f);
		private static readonly Color cyan		= new Color(0f, 1f, 1f);
		private static readonly Color teal		= new Color(0f, .5f, 1f);
		private static readonly Color darkcyan	= new Color(0f, .5f, .5f);
		

		private static readonly Color[] colors = new Color[]
		{
			purple, magenta, lilac, darkblue, blue, cyan, teal, darkcyan
		};

		public static Color ColorById(int id)
		{
			if (id < 0 || id > colors.Length - 1)
				return Color.black;

			return colors[id];
		}
	}
}

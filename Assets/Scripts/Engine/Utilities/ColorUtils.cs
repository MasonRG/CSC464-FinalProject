using UnityEngine;

namespace Engine.Utilities
{
	public static class ColorUtils
	{
		private static readonly Color[] colors = new Color[]
		{
			new Color(0,0,1f),
			new Color(.75f,0,0),
			new Color(0,.67f,0),
			new Color(0,.75f,.75f),
			new Color(.8f,0,.8f),
			new Color(.75f,.75f,0),
			new Color(.8f,.4f,0),
			new Color(1f,.45f,.65f)
		};

		public static Color ColorById(int id)
		{
			if (id < 0 || id > colors.Length - 1)
				return Color.black;

			return colors[id];
		}
	}
}

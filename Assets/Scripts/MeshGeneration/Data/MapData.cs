using UnityEngine;

namespace MeshGeneration.Data
{
	public struct MapData
	{
		public readonly float[,] heightMap;
		public readonly Color[] colorMap;

		public readonly float minHeight;
		public readonly float maxHeight;

		public MapData(float[,] heightMap, float minHeight, float maxHeight, Color[] colorMap)
		{
			this.heightMap = heightMap;
			this.minHeight = minHeight;
			this.maxHeight = maxHeight;
			this.colorMap = colorMap;
		}
	}
}
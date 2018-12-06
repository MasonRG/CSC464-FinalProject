using MeshGeneration.Data;
using UnityEngine;

namespace MeshGeneration.Generators
{
	public static class MapDataGenerator
	{
		public static MapData GenerateMapData(int width, int height, MapDataSettings settings, TerrainType[] regions, Vector2 sampleCenter)
		{
			float[,] heightMapUnscaled = NoiseGenerator.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCenter);
			float[,] heightMap = new float[width, height];

			AnimationCurve heightCurve_threadSafe = new AnimationCurve(settings.heightCurve.keys);

			float minHeight = float.MaxValue;
			float maxHeight = float.MinValue;

			for(int i = 0; i < width; i++) 
			{ for (int j = 0; j < height; j++)
				{
					heightMap[i, j] = heightMapUnscaled[i,j] * heightCurve_threadSafe.Evaluate(heightMapUnscaled[i, j]) * settings.heightMultiplier;

					var h = heightMap[i, j];
					if (h > maxHeight) maxHeight = h;
					if (h < minHeight) minHeight = h;
				}
			}

			return new MapData(heightMap, minHeight, maxHeight, GenerateColorMap(heightMapUnscaled, regions));
		}

		private static Color[] GenerateColorMap(float[,] heightMap, TerrainType[] regions)
		{
			int mapChunkSize = MapGenerator.mapChunkSize;

			Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
			for (int y = 0; y < mapChunkSize; y++)
			{
				for (int x = 0; x < mapChunkSize; x++)
				{
					float currentHeight = heightMap[x, y];
					for (int i = 0; i < regions.Length; i++)
					{
						if (currentHeight >= regions[i].height)
						{
							colorMap[y * mapChunkSize + x] = regions[i].color;
						}
						else
						{
							break;
						}
					}
				}
			}

			return colorMap;
		}
	}
}
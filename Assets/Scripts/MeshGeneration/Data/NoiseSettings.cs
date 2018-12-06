using UnityEngine;

namespace MeshGeneration.Data
{
	[System.Serializable]
	public class NoiseSettings
	{
		public Generators.NoiseGenerator.NormalizeMode normalizeMode;

		public float scale = 25;
		public int octaves = 5;
		[Range(0f, 1f)]
		public float persistance = 0.5f;
		public float lacunarity = 2;

		public int seed;
		public Vector2 offset;

		public void ValidateValues()
		{
			scale = Mathf.Max(scale, 0.01f);
			octaves = Mathf.Max(octaves, 1);
			lacunarity = Mathf.Max(lacunarity, 1);
			persistance = Mathf.Clamp01(persistance);
		}
	}
}
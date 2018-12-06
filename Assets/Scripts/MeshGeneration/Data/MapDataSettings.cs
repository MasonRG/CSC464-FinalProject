using UnityEngine;

namespace MeshGeneration.Data
{
	[System.Serializable]
	public class MapDataSettings
	{
		public NoiseSettings noiseSettings;
		
		public float heightMultiplier;
		public AnimationCurve heightCurve;

		public float MinHeight
		{
			get { return heightMultiplier * heightCurve.Evaluate(0); }
		}

		public float MaxHeight
		{
			get { return heightMultiplier * heightCurve.Evaluate(1); }
		}

		public void ValidateValues()
		{
			noiseSettings.ValidateValues();
		}
	}
}
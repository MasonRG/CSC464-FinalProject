using UnityEngine;

namespace MeshGeneration.Data
{
	public class ChunkData
	{
		public readonly MeshData meshData;
		public readonly Color[] colorData;

		public ChunkData(MeshData meshData, Color[] colorData)
		{
			this.meshData = meshData;
			this.colorData = colorData;
		}
	}
}
using System.Collections.Generic;
using UnityEngine;

namespace MeshGeneration
{
	public class ChunkedTerrain : MonoBehaviour
	{
		private int chunkSize;

		public int chunks;

		private Transform container;
		public Transform Container
		{
			get
			{
				if (container == null)
				{
					container = new GameObject("terrain_chunks").transform;
				}
				return container;
			}
		}

		private List<TerrainChunk> terrainChunks = new List<TerrainChunk>();


		private void Start()
		{
			chunkSize = MapGenerator.mapChunkSize - 1;
			GenerateChunks(chunks);
		}

		public void GenerateChunks(int size)
		{
			ClearChunks();

			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					Vector2 chunkCoord = new Vector2(x, y);
					terrainChunks.Add(new TerrainChunk(chunkCoord, chunkSize, Container));
				}
			}
		}

		public void ClearChunks()
		{
			foreach(var tc in terrainChunks)
			{
				tc.Clear();
			}
			terrainChunks.Clear();
		}

		public class TerrainChunk
		{
			GameObject meshObject;
			Vector2 position;

			public TerrainChunk(Vector2 coord, int size, Transform parent)
			{
				position = coord * size;
				Vector3 positionV3 = new Vector3(position.x, 0, position.y);

				meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
				meshObject.transform.parent = parent;
				meshObject.transform.localPosition = positionV3;
				meshObject.transform.localScale = Vector3.one * size / 10f;
			}

			public void Clear()
			{
				if (meshObject != null)
					Destroy(meshObject);
			}
		}
	}
}
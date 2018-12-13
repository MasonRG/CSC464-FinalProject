using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeshGeneration
{
	public class ChunkData
	{
		public readonly Vector2Int coord;
		public readonly HeightMapData heightMap;

		public Vector2 Position
		{
			get { return coord * (MapGenerator.mapChunkSize - 1);  }
		}

		public Vector3 PositionV3
		{
			get { return new Vector3(Position.x, 0, Position.y) * MapGenerator.Instance.meshScale; }
		}

		public Vector3 Scale
		{
			get { return Vector3.one * MapGenerator.Instance.meshScale; }
		}

		public ChunkData(Vector2Int coord)
		{
			this.coord = coord;
			heightMap = HeightMapGenerator.GenerateHeightMapData(MapGenerator.mapChunkSize, MapGenerator.mapChunkSize, MapGenerator.Instance.noiseSettings, Position);
		}

		public ChunkData(Vector2Int coord, float[,] heightMap)
		{
			this.coord = coord;
			this.heightMap = new HeightMapData(heightMap, 0f, 0f);
		}
	}

	public class ChunkedTerrainManager
	{
		private Transform chunkContainer;
		public Transform ChunkContainer
		{
			get
			{
				if (chunkContainer == null)
					chunkContainer = new GameObject("terrain_chunks").transform;
				return chunkContainer;
			}
		}


		public void GenerateChunkDataForRange(int numChunksSqrt, int start, int end)
		{
			List<ChunkData> chunks = new List<ChunkData>();

			for (int i = start; i < end; i++)
			{
				int x = i / numChunksSqrt;
				int y = i % numChunksSqrt;
				Vector2Int chunkCoord = new Vector2Int(x, y);
				chunks.Add(new ChunkData(chunkCoord));
			}

			MapGenerator.Instance.FireChunksGenerated(chunks);
		}

		List<TerrainChunk> terrainChunks = new List<TerrainChunk>();
		public void AddChunks(List<ChunkData> chunks, Color chunkColor)
		{
			for(int i = 0; i < chunks.Count; i++)
			{
				AddChunk(chunks[i], chunkColor);
			}
		}

		public void AddChunk(ChunkData chunkData, Color chunkColor)
		{
			var newChunk = new TerrainChunk(chunkData);
			newChunk.Material.color = chunkColor;
			terrainChunks.Add(newChunk);
		}
	}


	public class TerrainChunk
	{
		public readonly ChunkData data;

		private GameObject meshObject;
		private Vector2 position;
		private MeshRenderer meshRenderer;
		private MeshFilter meshFilter;

		public Material Material { get { return meshRenderer.material; } }

		public TerrainChunk(ChunkData data)
		{
			this.data = data;

			meshObject = new GameObject("Terrain Chunk (" + data.coord.x + "," + data.coord.y + ")");
			meshFilter = meshObject.AddComponent<MeshFilter>();

			meshRenderer = meshObject.AddComponent<MeshRenderer>();
			meshRenderer.material = MapGenerator.Instance.meshMaterial;

			meshObject.transform.parent = MapGenerator.Instance.ChunkContainer;
			meshObject.transform.localPosition = data.PositionV3;
			meshObject.transform.localScale = data.Scale;

			MeshDataRequester.Instance.RequestMeshData(MapGenerator.Instance, data, OnMeshGenerated);
		}

		void OnMeshGenerated(MeshData meshData)
		{
			meshFilter.mesh = meshData.CreateMesh();
		}

		public void Clear()
		{
			if (meshObject != null)
				UnityEngine.Object.Destroy(meshObject);
		}

	}
}
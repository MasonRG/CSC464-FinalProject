using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeshGeneration
{
	public class ChunkData
	{
		public readonly Vector2Int coord;
		public Vector3[] vertices;
		public int[] triangles;

		public bool isReady = false;
		private readonly Action<ChunkData> onReady;

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

		public ChunkData(Vector2Int coord, Vector3[] vertices, int[] triangles)
		{
			this.coord = coord;
			this.vertices = vertices;
			this.triangles = triangles;
		}

		public ChunkData(Vector2Int coord, Action<ChunkData> onReady)
		{
			this.coord = coord;
			this.onReady = onReady;

			ChunkDataRequester.Instance.RequestChunkData(MapGenerator.Instance, Position, OnDataReceived);
		}

		private void OnDataReceived(MeshData meshData)
		{
			this.vertices = meshData.vertices;
			this.triangles = meshData.triangles;

			isReady = true;

			if (onReady != null)
				onReady(this);
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

		private Dictionary<string, List<ChunkData>> chunkDict = new Dictionary<string, List<ChunkData>>();
		private Dictionary<string, int> chunkCounts = new Dictionary<string, int>();

		public void GenerateChunkDataForRange(int numChunksSqrt, int start, int end)
		{
			string key = start.ToString() + end.ToString();
			if (chunkDict.ContainsKey(key)) return;

			chunkDict.Add(key, new List<ChunkData>());
			chunkCounts.Add(key, end - start);

			for (int i = start; i < end; i++)
			{
				int x = i / numChunksSqrt;
				int y = i % numChunksSqrt;
				Vector2Int chunkCoord = new Vector2Int(x, y);
				var chunk = new ChunkData(chunkCoord, (data) => ChunkReady(key, data));
			}

		}

		private void ChunkReady(string id, ChunkData data)
		{
			chunkDict[id].Add(data);
			if (chunkDict[id].Count == chunkCounts[id])
			{
				var tmp = chunkDict[id];
				chunkDict.Remove(id);
				chunkCounts.Remove(id);
				MapGenerator.Instance.FireChunksGenerated(tmp);
			}
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
			meshFilter.mesh = MeshGenerator.GenerateTerrainMeshFromPrecomputed(data.vertices, data.triangles).CreateMesh();

			meshRenderer = meshObject.AddComponent<MeshRenderer>();
			meshRenderer.material = MapGenerator.Instance.meshMaterial;

			meshObject.transform.parent = MapGenerator.Instance.ChunkContainer;
			meshObject.transform.localPosition = data.PositionV3;
			meshObject.transform.localScale = data.Scale;
		}

		public void Clear()
		{
			if (meshObject != null)
				UnityEngine.Object.Destroy(meshObject);
		}

	}
}
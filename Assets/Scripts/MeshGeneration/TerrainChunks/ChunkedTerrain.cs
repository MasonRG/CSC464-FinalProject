using System.Collections.Generic;
using UnityEngine;

namespace MeshGeneration.TerrainChunks
{
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

		//Chunk events
		public delegate void ChunkEventHandler(TerrainChunk chunk);

		public event ChunkEventHandler OnChunkRequested;
		private void FireChunkRequested(TerrainChunk chunk) { if (OnChunkRequested != null) OnChunkRequested(chunk); }
		public event ChunkEventHandler OnChunkCreated;
		private void FireChunkCreated(TerrainChunk chunk) { if (OnChunkCreated != null) OnChunkCreated(chunk); }

		private void FireChunkEvent(TerrainChunk chunk, bool forRequest)
		{
			if (forRequest) FireChunkRequested(chunk);
			else FireChunkCreated(chunk);
		}

		private List<TerrainChunk> terrainChunks = new List<TerrainChunk>();

		public void GenerateAllChunks(int numChunksSqrt)
		{
			GenerateChunkRange(numChunksSqrt, 0, numChunksSqrt * numChunksSqrt);
		}

		public void GenerateChunkRange(int numChunksSqrt, int start, int end)
		{
			ClearChunks();

			int numChunks = numChunksSqrt * numChunksSqrt;
			for (int i = start; i < end; i++)
			{
				int x = i / numChunksSqrt;
				int y = i % numChunksSqrt;
				Vector2 chunkCoord = new Vector2(x, y);
				terrainChunks.Add(MakeChunk(chunkCoord));

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
		
		private TerrainChunk MakeChunk(Vector2 coord)
		{
			return new TerrainChunk(coord, MapGenerator.mapChunkSize - 1, ChunkContainer, MapGenerator.Instance.meshMaterial, FireChunkEvent);
		}
	}
}
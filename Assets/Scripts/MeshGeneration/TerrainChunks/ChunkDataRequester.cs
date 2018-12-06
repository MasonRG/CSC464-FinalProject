using MeshGeneration.Data;
using MeshGeneration.Generators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MeshGeneration.TerrainChunks
{
	public class ChunkDataRequester : MonoBehaviour
	{
		private static ChunkDataRequester instance;
		public static ChunkDataRequester Instance
		{
			get
			{
				if (instance == null)
				{
					instance = FindObjectOfType<ChunkDataRequester>();
				}
				return instance;
			}
		}


		private Queue<ChunkThreadInfo> chunkDataThreadInfoQueue = new Queue<ChunkThreadInfo>();

		public void RequestChunkData(MapGenerator mapGen, Vector2 center, Action<ChunkData> callback)
		{
			if (mapGen.useMultiThreading)
			{
				ThreadStart threadStart = delegate { ComputeChunkDataThread(mapGen, center, callback); };
				new Thread(threadStart).Start();
			}
			else
			{
				ComputeChunkData(mapGen, center, callback);
			}
		}

		private void ComputeChunkDataThread(MapGenerator mapGen, Vector2 center, Action<ChunkData> callback)
		{
			MapData mapData = MapDataGenerator.GenerateMapData(MapGenerator.mapChunkSize, MapGenerator.mapChunkSize, mapGen.mapSettings, mapGen.regions, center);
			MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap);

			lock (chunkDataThreadInfoQueue)
			{
				chunkDataThreadInfoQueue.Enqueue(new ChunkThreadInfo(callback, new ChunkData(meshData, mapData.colorMap)));
			}
		}

		private void ComputeChunkData(MapGenerator mapGen, Vector2 center, Action<ChunkData> callback)
		{
			MapData mapData = MapDataGenerator.GenerateMapData(MapGenerator.mapChunkSize, MapGenerator.mapChunkSize, mapGen.mapSettings, mapGen.regions, center);
			MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap);
			callback(new ChunkData(meshData, mapData.colorMap));
		}

		private void Update()
		{
			if (chunkDataThreadInfoQueue.Count > 0)
			{
				for (int i = 0; i < chunkDataThreadInfoQueue.Count; i++)
				{
					ChunkThreadInfo threadInfo = chunkDataThreadInfoQueue.Dequeue();
					threadInfo.callback(threadInfo.parameter);
				}
			}

		}

		struct ChunkThreadInfo
		{
			public readonly Action<ChunkData> callback;
			public readonly ChunkData parameter;

			public ChunkThreadInfo(Action<ChunkData> callback, ChunkData parameter)
			{
				this.callback = callback;
				this.parameter = parameter;
			}
		}
	}
}
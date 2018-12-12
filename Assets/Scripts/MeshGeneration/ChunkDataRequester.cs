using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MeshGeneration
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
		public void RequestChunkData(MapGenerator mapGen, Vector2 center, Action<MeshData> callback)
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

		private void ComputeChunkDataThread(MapGenerator mapGen, Vector2 center, Action<MeshData> callback)
		{
			HeightMapData mapData = HeightMapGenerator.GenerateHeightMapData(MapGenerator.mapChunkSize, MapGenerator.mapChunkSize, mapGen.noiseSettings, center);
			MeshData meshData = MeshGenerator.GenerateTerrainMeshFromHeightMap(mapData.heightMap);

			lock (chunkDataThreadInfoQueue)
			{
				chunkDataThreadInfoQueue.Enqueue(new ChunkThreadInfo(callback, meshData));
			}
		}

		private void ComputeChunkData(MapGenerator mapGen, Vector2 center, Action<MeshData> callback)
		{
			HeightMapData mapData = HeightMapGenerator.GenerateHeightMapData(MapGenerator.mapChunkSize, MapGenerator.mapChunkSize, mapGen.noiseSettings, center);
			MeshData meshData = MeshGenerator.GenerateTerrainMeshFromHeightMap(mapData.heightMap);
			callback(meshData);
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
			public readonly Action<MeshData> callback;
			public readonly MeshData parameter;

			public ChunkThreadInfo(Action<MeshData> callback, MeshData parameter)
			{
				this.callback = callback;
				this.parameter = parameter;
			}
		}
	}
}
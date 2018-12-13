using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MeshGeneration
{
	public class MeshDataRequester : MonoBehaviour
	{
		private static MeshDataRequester instance;
		public static MeshDataRequester Instance
		{
			get
			{
				if (instance == null)
				{
					instance = FindObjectOfType<MeshDataRequester>();
				}
				return instance;
			}
		}


		private Queue<ThreadInfo> meshDataThreadInfoQueue = new Queue<ThreadInfo>();
		public void RequestMeshData(MapGenerator mapGen, ChunkData chunk, Action<MeshData> callback)
		{
			if (mapGen.useMultiThreading)
			{
				ThreadStart threadStart = delegate { ComputeMeshDataThread(mapGen, chunk.heightMap, callback); };
				new Thread(threadStart).Start();
			}
			else
			{
				ComputeMeshData(mapGen, chunk.heightMap, callback);
			}
		}

		private void ComputeMeshDataThread(MapGenerator mapGen, HeightMapData heightMapData, Action<MeshData> callback)
		{
			MeshData meshData = MeshGenerator.GenerateTerrainMeshFromHeightMap(heightMapData.heightMap);

			lock (meshDataThreadInfoQueue)
			{
				meshDataThreadInfoQueue.Enqueue(new ThreadInfo(callback, meshData));
			}
		}

		private void ComputeMeshData(MapGenerator mapGen, HeightMapData heightMapData, Action<MeshData> callback)
		{
			MeshData meshData = MeshGenerator.GenerateTerrainMeshFromHeightMap(heightMapData.heightMap);
			callback(meshData);
		}

		private void Update()
		{
			if (meshDataThreadInfoQueue.Count > 0)
			{
				for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
				{
					ThreadInfo threadInfo = meshDataThreadInfoQueue.Dequeue();
					threadInfo.callback(threadInfo.parameter);
				}
			}

		}

		struct ThreadInfo
		{
			public readonly Action<MeshData> callback;
			public readonly MeshData parameter;

			public ThreadInfo(Action<MeshData> callback, MeshData parameter)
			{
				this.callback = callback;
				this.parameter = parameter;
			}
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MeshGeneration
{
	public class MapGenerator : MonoBehaviour
	{
		private static MapGenerator instance;
		public static MapGenerator Instance
		{
			get
			{
				if (instance == null)
					instance = FindObjectOfType<MapGenerator>();
				return instance;
			}
		}

		public const int mapChunkSize = 64;

		public enum DrawMode { NoiseMap, Mesh }

		[Header("Mesh Generation Settings")]
		public DrawMode drawMode;
		public bool autoUpdate;
		[Space()]
		public bool useMultiThreading = true;
		public int numChunksSqrt;
		[Space()]
		public float meshScale = 0.2f;
		public Material meshMaterial;
		[Space()]
		public NoiseSettings noiseSettings;

		public int NumChunks { get { return numChunksSqrt * numChunksSqrt; } }

		private ChunkedTerrainManager chunkManager = new ChunkedTerrainManager();
		public ChunkedTerrainManager ChunkManager { get { return chunkManager; } }
		public Transform ChunkContainer { get { return chunkManager.ChunkContainer; } }


		public delegate void ChunkEventHandler(List<ChunkData> generatedChunks);
		public event ChunkEventHandler OnChunksGenerated;
		public void FireChunksGenerated(List<ChunkData> generatedChunks) { if (OnChunksGenerated != null) OnChunksGenerated(generatedChunks); }


		public void GenerateMapChunkBatch(int start, int end)
		{
			start = Mathf.Clamp(start, 0, NumChunks);
			end = Mathf.Clamp(end, 0, NumChunks);
			if (start > end)
			{
				var tmp = end;
				end = start;
				start = tmp;
			}
			ChunkManager.GenerateChunkDataForRange(numChunksSqrt, start, end);
		}


		public void DrawMapInEditor()
		{
			HeightMapData mapData = HeightMapGenerator.GenerateHeightMapData(mapChunkSize, mapChunkSize, noiseSettings, Vector2.zero);

			MapDisplay display = FindObjectOfType<MapDisplay>();
			if (drawMode == DrawMode.NoiseMap)
			{
				display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
			}
			else if (drawMode == DrawMode.Mesh)
			{
				display.DrawMesh(
					MeshGenerator.GenerateTerrainMeshFromHeightMap(mapData.heightMap),
					TextureGenerator.TextureFromHeightMap(mapData.heightMap));
			}
			
		}

		private void OnValidate()
		{
			noiseSettings.ValidateValues();
		}


		
	}
}
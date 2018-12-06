using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using MeshGeneration.TerrainChunks;
using MeshGeneration.Generators;
using MeshGeneration.Data;

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

		public const int mapChunkSize = 254;

		public enum DrawMode { NoiseMap, ColorMap, Mesh }

		[Header("Mesh Generation Settings")]
		public DrawMode drawMode;
		public bool autoUpdate;
		[Space()]
		public bool useMultiThreading = true;
		public int numChunksSqrt;
		[Space()]
		public float meshScale = 0.05f;
		public Material meshMaterial;
		[Space()]
		public MapDataSettings mapSettings;
		public TerrainType[] regions;


		private ChunkedTerrainManager chunkedTerrain = new ChunkedTerrainManager();

		public int NumChunks { get { return numChunksSqrt * numChunksSqrt; } }

		private void Start()
		{
		//	chunkedTerrain.OnChunkRequested += OnChunkRequested;
			chunkedTerrain.OnChunkCreated += OnChunkCreated;

			chunkedTerrain.GenerateAllChunks(numChunksSqrt);
			//chunkedTerrain.GenerateChunkRange(numChunksSqrt, 0, 12);
		}


		private void OnChunkRequested(TerrainChunk chunk)
		{
			Debug.Log(string.Format("Chunk ({0},{1}) Requested.", chunk.coord.x, chunk.coord.y));
		}

		private void OnChunkCreated(TerrainChunk chunk)
		{
			Debug.Log(string.Format("Chunk ({0},{1}) Created.", chunk.coord.x, chunk.coord.y));
		}

		

		public void DrawMapInEditor()
		{
			MapData mapData = MapDataGenerator.GenerateMapData(mapChunkSize, mapChunkSize, mapSettings, regions, Vector2.zero);

			MapDisplay display = FindObjectOfType<MapDisplay>();
			if (drawMode == DrawMode.NoiseMap)
			{
				display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
			}
			else if (drawMode == DrawMode.ColorMap)
			{
				display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
			}
			else if (drawMode == DrawMode.Mesh)
			{
				display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
			}
		}

		private void OnValidate()
		{
			mapSettings.ValidateValues();
		}


		
	}


	[System.Serializable]
	public struct TerrainType
	{
		public string name;
		public float height;
		public Color color;
	}

	
}
using MeshGeneration.Data;
using MeshGeneration.Generators;
using System;
using UnityEngine;

namespace MeshGeneration.TerrainChunks
{
	public class TerrainChunk
	{
		public readonly Vector2 coord;

		private GameObject meshObject;
		private Vector2 position;

		private MeshRenderer meshRenderer;
		private MeshFilter meshFilter;

		private Action<TerrainChunk, bool> eventCallback;

		public TerrainChunk(Vector2 coord, int size, Transform parent, Material material, Action<TerrainChunk, bool> eventCallback)
		{
			this.coord = coord;
			this.eventCallback = eventCallback;

			position = coord * size;
			Vector3 positionV3 = new Vector3(position.x, 0, position.y) * MapGenerator.Instance.meshScale;

			meshObject = new GameObject("Terrain Chunk (" + coord.x + "," + coord.y + ")");
			meshFilter = meshObject.AddComponent<MeshFilter>();
			meshRenderer = meshObject.AddComponent<MeshRenderer>();
			meshRenderer.material = material;

			meshObject.transform.parent = parent;
			meshObject.transform.localPosition = positionV3;
			meshObject.transform.localScale = Vector3.one * MapGenerator.Instance.meshScale;

			if (eventCallback != null)
				eventCallback(this, true);

			ChunkDataRequester.Instance.RequestChunkData(MapGenerator.Instance, position, OnChunkDataReceived);
		}

		public void Clear()
		{
			if (meshObject != null)
				UnityEngine.Object.Destroy(meshObject);
		}

		private void OnChunkDataReceived(ChunkData data)
		{
			Texture2D texture = TextureGenerator.TextureFromColorMap(data.colorData, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
			meshRenderer.material.mainTexture = texture;
			meshFilter.mesh = data.meshData.CreateMesh();

			if (eventCallback != null)
				eventCallback(this, false);
		}
	}
}
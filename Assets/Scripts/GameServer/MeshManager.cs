using Engine;
using MeshGeneration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshManager : MonoBehaviour
{
	private static MeshManager instance;
	public static MeshManager Instance
	{
		get
		{
			if (instance == null)
				instance = FindObjectOfType<MeshManager>();
			return instance;
		}
	}

	private void Start()
	{
		MapGenerator.Instance.OnChunksGenerated += OnChunkDataGenerated;
	}

	private void OnChunkDataGenerated(List<ChunkData> chunks)
	{
		chunks = SerializationTestPass(chunks);
		MapGenerator.Instance.ChunkManager.AddChunks(chunks);
	}

	int currStart = 0;
	int currEnd = 5;
	void Update ()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			MapGenerator.Instance.GenerateMapChunkBatch(currStart, currEnd);
			var diff = currEnd - currStart;
			currStart += diff;
			currEnd += diff;
		}
	}


	//TESTING BYTE SERIALIZATION
	private List<ChunkData> SerializationTestPass(List<ChunkData> chunks)
	{
		List<SerializedChunk> serializedChunks = new List<SerializedChunk>();
		foreach(var c in chunks)
		{
			serializedChunks.Add(new SerializedChunk(c));
		}
		chunks.Clear();
		foreach (var sc in serializedChunks)
		{
			chunks.Add(sc.Deserialize());
		}
		serializedChunks.Clear();

		return chunks;
	}
}


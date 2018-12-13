using Engine;
using Engine.Utilities;
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

	public HashSet<int> GetAllChunkIds()
	{
		HashSet<int> set = new HashSet<int>();
		for(int i = 0; i < MapGenerator.Instance.NumChunks; i++)
		{
			set.Add(i);
		}
		return set;
	}

	public Pair<int,int>[] GetChunkRanges(int numClients)
	{
		int chunksPerClient = MapGenerator.Instance.NumChunks / numClients;
		int chunksLeftOver = MapGenerator.Instance.NumChunks % numClients;

		int curr = 0;
		int end = chunksPerClient;

		Pair<int, int>[] pairs = new Pair<int, int>[numClients];
		for(int i = 0; i < numClients; i++)
		{
			if (chunksLeftOver > 0)
			{
				end += 1;
				chunksLeftOver--;
			}

			pairs[i] = new Pair<int, int>(curr, end);
			curr = end;
			end += chunksPerClient;
		}

		return pairs;
	}

	private System.Action<List<ChunkData>> onGenerated;

	private void OnChunkDataGenerated(List<ChunkData> chunks)
	{
		if (onGenerated != null)
			onGenerated(chunks);
		//chunks = SerializationTestPass(chunks);
		//MapGenerator.Instance.ChunkManager.AddChunks(chunks);
	}


	public void GenerateChunks(int start, int end, System.Action<List<ChunkData>> callback)
	{
		onGenerated = callback;
		MapGenerator.Instance.GenerateMapChunkBatch(start, end);
	}

	public List<SerializedChunk> SerializeChunks(List<ChunkData> chunks)
	{
		List<SerializedChunk> serializedChunks = new List<SerializedChunk>();
		foreach (var c in chunks)
		{
			serializedChunks.Add(new SerializedChunk(c));
		}
		return serializedChunks;
	}

	public List<ChunkData> DeserializeChunks(List<SerializedChunk> serializedChunks)
	{
		List<ChunkData> chunks = new List<ChunkData>();
		foreach (var sc in serializedChunks)
		{
			chunks.Add(sc.Deserialize());
		}
		return chunks;
	}

	public void AddChunks(List<ChunkData> chunks, uint creatorId)
	{
		MapGenerator.Instance.ChunkManager.AddChunks(chunks, ColorUtils.ColorById((int)creatorId));
	}

	public void AddChunk(ChunkData chunk, uint creatorId)
	{
		MapGenerator.Instance.ChunkManager.AddChunk(chunk, ColorUtils.ColorById((int)creatorId));
	}




	//TESTING BYTE SERIALIZATION
	private List<ChunkData> SerializationTestPass(List<ChunkData> chunks)
	{
		List<SerializedChunk> serializedChunks = new List<SerializedChunk>();
		foreach(var c in chunks)
		{
		//	serializedChunks.Add(new SerializedChunk(c));
		}
		chunks.Clear();
		foreach (var sc in serializedChunks)
		{
		//	chunks.Add(sc.Deserialize());
		}
		serializedChunks.Clear();

		return chunks;
	}
}


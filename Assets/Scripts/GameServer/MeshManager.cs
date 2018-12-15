using Engine;
using Engine.Utilities;
using MeshGeneration;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

	HashSet<int> outstandingChunks;
	Dictionary<int, uint> creationLog;
	public HashSet<int> GetOutstandingChunksCopy()
	{
		return new HashSet<int>(outstandingChunks);
	}

	public bool HasChunkBeenGenerated(int chunkId)
	{
		return creationLog.ContainsKey(chunkId);
	}

	private void Start()
	{
		MapGenerator.Instance.OnChunksGenerated += OnChunkDataGenerated;
		outstandingChunks = GetAllChunkIds();
		creationLog = new Dictionary<int, uint>();
	}

	private HashSet<int> GetAllChunkIds()
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
		int chunksPerClient = outstandingChunks.Count / numClients;
		int chunksLeftOver = outstandingChunks.Count % numClients;

		var outstanding = outstandingChunks.ToList();
		int index = 0;

		Pair<int, int>[] pairs = new Pair<int, int>[numClients];
		for(int i = 0; i < numClients; i++)
		{
			int add = chunksPerClient;
			if (chunksLeftOver > 0)
			{
				add++;
				chunksLeftOver--;
			}

			int curr = outstanding[index];
		//	Debug.Log(string.Format("count: {0} | curr: {1} | add: {2}", outstandingChunks.Count, curr, add));
			int end = outstanding[add-1];

			pairs[i] = new Pair<int, int>(curr, end);
			index += add;
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

	public void AddChunk(ChunkData chunk, uint creatorId)
	{
		//only add this chunk if it is outstanding
		//if added - remove from outstanding chunks
		if (outstandingChunks.Contains(chunk.Id))
		{
			MapGenerator.Instance.ChunkManager.AddChunk(chunk, ColorUtils.ColorById((int)creatorId));
			outstandingChunks.Remove(chunk.Id);
			creationLog.Add(chunk.Id, creatorId);
		}
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


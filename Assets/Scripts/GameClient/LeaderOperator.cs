using System.Collections;
using UnityEngine;
using Engine.Utilities;
using Engine;
using System.Collections.Generic;
using System.Linq;

namespace GameClient
{
	public class LeaderOperator
	{
		float delayTime = 0.5f;
		ToggleRoutine delayedGeneration;

		List<Client> activeClients = new List<Client>();

		public bool Active
		{
			get { return NetworkHub.MyClientID == NetworkHub.LeaderID; }
		}

		public LeaderOperator()
		{
			delayedGeneration = new ToggleRoutine(DelayedGeneration());
		}


		public void StartGeneration(int delayTimeMs, int chunksPerLine)
		{
			if (delayedGeneration.IsRunning) return;

			if (delayTime >= 0)
				delayTime = ((float)delayTimeMs) / 1000;

			delayedGeneration.SetRoutine(DelayedGeneration());
			delayedGeneration.Start();
		}

		private IEnumerator DelayedGeneration()
		{
			timeoutRecord.Clear();

			activeClients = NetworkHub.FindAllBehaviours<Client>();
			var chunkRanges = MeshManager.Instance.GetChunkRanges(activeClients.Count);
			var allChunks = MeshManager.Instance.GetOutstandingChunksCopy();

			while (allChunks.Count > 0)
			{
				for (int i = 0; i < activeClients.Count; i++)
				{
					var range = chunkRanges[i];

					int chunkId = range.first;
					uint clientId = activeClients[i].Id;
					RoutineRunner.StartTimer(() => ChunkGenTimeout(chunkId, clientId), TIMEOUT_WAIT);

					activeClients[i].SendChunksRequest(range.first, range.first + 1);

					allChunks.Remove(range.first);
					chunkRanges[i] = new Pair<int, int>(range.first + 1, range.second);
				}

				yield return new WaitForSeconds(delayTime);
			}

			delayedGeneration.IsRunning = false;
			RoutineRunner.StartTimer(GenMissingChunks, TIMEOUT_WAIT);
		}

		Dictionary<int, uint> timeoutRecord = new Dictionary<int, uint>();
		const float TIMEOUT_WAIT = 2f;
		private void ChunkGenTimeout(int chunkId, uint creatorId)
		{
			if (!MeshManager.Instance.HasChunkBeenGenerated(chunkId))
			{
				Debug.Log("Timeout on chunk! " + chunkId + " by " + creatorId);
				timeoutRecord.Add(chunkId, creatorId);
			}
		}

		private void GenMissingChunks()
		{
			if (timeoutRecord.Count == 0)
				return;

			StartGeneration((int)(delayTime * 1000), -1);
		}

		/* UNUSED
		public void StartDistributedGeneration()
		{
			var clients = NetworkHub.FindAllBehaviours<Client>();
			var chunkRanges = MeshManager.Instance.GetChunkRanges(clients.Count);
			for (int i = 0; i < clients.Count; i++)
			{
				clients[i].SendChunksRequest(chunkRanges[i].first, chunkRanges[i].second);
			}
		}
		*/
	}
}
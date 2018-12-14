using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using GameServer;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking;
using GameConsole;
using Engine.Utilities;
using Engine;

namespace GameClient
{
	public class ClientManager : ClientManagerBehavior
	{
		float delayTime = 0.5f;
		ToggleRoutine delayedGeneration;

		protected override void NetworkStart()
		{
			base.NetworkStart();

			var myClient = NetworkManager.Instance.InstantiateClient() as Client;

			if (networkObject.IsServer)
			{
				NetworkManager.Instance.Networker.playerAccepted += OnConnectionEvent_ClientConnected;
				NetworkManager.Instance.Networker.playerDisconnected += OnConnectionEvent_ClientDisconnect;

				myClient.networkStarted += ServerClientStarted;
			}
		}

		private void Start()
		{
			delayedGeneration = new ToggleRoutine(DelayedGeneration());
		}

		private void ServerClientStarted(NetworkBehavior behavior)
		{
			var client = behavior as Client;
			client.networkStarted -= ServerClientStarted;
			OnClientCreated(client.networkObject);
		}

		public void SendServerMessage(MessageEvent gameEvent, Client client, string message = "")
		{
			if (client != null)
				NetworkHub.GameConsole.BroadcastServerMessage(gameEvent, client.Name, client.Id, message);
			else
				NetworkHub.GameConsole.BroadcastServerMessage(gameEvent, message: message);
		}


		public void StartGeneration(int delayTimeMs, int chunksPerLine)
		{
			if (delayedGeneration.IsRunning)
				return;

			//Ignore chunksPerLine option for now (they need to be synced with all clients
			//if (chunksPerLine > 0)
			//	MeshGeneration.MapGenerator.Instance.numChunksSqrt = chunksPerLine;

			if (delayTime >= 0)
			{
				delayTime = ((float)delayTimeMs) / 1000;
			}
			

			delayedGeneration.Start();
		}

		private IEnumerator DelayedGeneration()
		{
			var clients = NetworkHub.FindAllBehaviours<Client>();
			var chunkRanges = MeshManager.Instance.GetChunkRanges(clients.Count);
			var allChunks = MeshManager.Instance.GetAllChunkIds();

			while (allChunks.Count > 0)
			{
				for (int i = 0; i < clients.Count; i++)
				{
					var range = chunkRanges[i];
					if (range.first == range.second)
						continue;

					clients[i].SendChunksRequest(range.first, range.first + 1);
					allChunks.Remove(range.first);
					chunkRanges[i] = new Pair<int, int>(range.first + 1, range.second);
				}

				yield return new WaitForSeconds(delayTime);
			}

			delayedGeneration.IsRunning = false;
		}

		public void StartDistributedGeneration()
		{
			var clients = NetworkHub.FindAllBehaviours<Client>();

			var chunkRanges = MeshManager.Instance.GetChunkRanges(clients.Count);
			for (int i = 0; i < clients.Count; i++)
			{
				clients[i].SendChunksRequest(chunkRanges[i].first, chunkRanges[i].second);
			}
		}



		protected void OnConnectionEvent_ClientConnected(NetworkingPlayer player, NetWorker sender)
		{
			MainThreadManager.Run(() =>
			{
				Debug.Log(string.Format("Player Accepted (id:{0})!", player.NetworkId));

				player.Networker.objectCreated += OnClientCreated;
			});
		}

		private void OnClientCreated(NetworkObject networkObject)
		{
			networkObject.Networker.objectCreated -= OnClientCreated;

			NetworkHub.TryUntilTrueOrTimeout
				(
					() => {
						var client = networkObject.AttachedBehavior as Client;
						return client != null && client.networkObject.NetworkReady; },
					() => { SendServerMessage(MessageEvent.ClientJoined, networkObject.AttachedBehavior as Client); },
					() => { Debug.Log("Couldn't find Client!"); }
				);
		}

		protected void OnConnectionEvent_ClientDisconnect(NetworkingPlayer player, NetWorker sender)
		{
			//Find all network objects of the client so we can remove them
			MainThreadManager.Run(() =>
			{
				Debug.Log(string.Format("Player Disconnected (id:{0})!", player.NetworkId));
				

				Stack<NetworkObject> destroyStack = new Stack<NetworkObject>();
				foreach (var obj in sender.NetworkObjectList)
				{
					if (obj.Owner == player)
					{
						destroyStack.Push(obj);
						var client = obj.AttachedBehavior as Client;
						if (client != null)
						{
							SendServerMessage(MessageEvent.ClientLeft, client);
						}
					}
				}

				Debug.Log(string.Format("Objects for deletion: {0}", destroyStack.Count));

				while (destroyStack.Count > 0)
				{
					var obj = destroyStack.Pop();
					sender.NetworkObjectList.Remove(obj);
					obj.Destroy();
				}
			});
		}
	}
}
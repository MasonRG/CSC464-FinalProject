using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using GameServer;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking;
using GameConsole;
using Engine.Utilities;

namespace GameClient
{
	public class ClientManager : ClientManagerBehavior
	{


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
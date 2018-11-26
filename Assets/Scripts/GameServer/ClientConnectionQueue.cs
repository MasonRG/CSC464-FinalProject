using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using Engine.Utilities;
using GameClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameServer
{
	/// <summary>
	/// Threadsafe queue for joining clients.
	/// </summary>
	public class ClientConnectionQueue
	{
		private readonly object lockObj = new object();

		private readonly Queue<NetworkingPlayer> joinQueue = new Queue<NetworkingPlayer>();
		private bool playerIsJoining = false;

		//Events
		public delegate void PlayerEventHandler(NetworkingPlayer player);
		public delegate void ClientEventHandler(Client clientObject);

		public event PlayerEventHandler OnJoinReady;
		public event ClientEventHandler OnJoinFinished;

		public void FireJoinReady(NetworkingPlayer player) { if (OnJoinReady != null) OnJoinReady(player); }
		public void FireJoinFinished(Client clientObject) { if (OnJoinFinished != null) OnJoinFinished(clientObject); }


		public ClientConnectionQueue() { }


		public void Enqueue(NetworkingPlayer player)
		{
			lock (lockObj)
			{
				Debug.Log("Enqueuing player (id: " + player.NetworkId + "). " + joinQueue.Count + " other players in join queue.");
				joinQueue.Enqueue(player);
			}

			StartJoin();
		}

		private void StartJoin()
		{
			lock (lockObj)
			{
				if (joinQueue.Count == 0)
					return;

				if (!playerIsJoining)
				{
					playerIsJoining = true;

					var joiningPlayer = joinQueue.Dequeue();
					joiningPlayer.Networker.objectCreated += OnClientObjectCreated;
					FireJoinReady(joiningPlayer);
				}
			}
		}


		private void OnClientObjectCreated(NetworkObject networkObject)
		{
			networkObject.Networker.objectCreated -= OnClientObjectCreated;
			NetworkHub.TryUntilTrueOrTimeout
				(
					() => { return networkObject.AttachedBehavior as Client != null; },
					() => { OnClientObjectFound(networkObject.AttachedBehavior as Client); },
					() => { Debug.Log("Couldn't find Client!"); }
				);
		}


		private void OnClientObjectFound(Client clientObject)
		{
			lock (lockObj)
			{
				playerIsJoining = false;
			}

			FireJoinFinished(clientObject);

			///let the next player join - if there is a player waiting
			StartJoin();
		}
	}
}

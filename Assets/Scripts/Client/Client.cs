using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking;
using Engine.Utilities;

namespace GameClient
{
	public class Client : ClientBehavior
	{
		public string Name
		{
			get
			{
				return "Host-" + networkObject.Owner.NetworkId;
			}
		}

		public uint Id
		{
			get
			{
				return networkObject.Owner.NetworkId;
			}
		}

		protected override void NetworkStart()
		{
			base.NetworkStart();

			if (NetworkManager.Instance.Networker is IServer) { }
			else
			{
				NetworkManager.Instance.Networker.disconnected += OnDisconnect;
			}
		}



		private void Update()
		{
			if (!networkObject.IsOwner || !networkObject.NetworkReady)
				return;


			//Disconnecting
			if (!NetworkHub.MessageConsoleActive && Input.GetKeyDown(KeyCode.End))
			{
				DisconnectFromServer();
				return;
			}
		}



		private void OnDisconnect(NetWorker sender)
		{
			NetworkManager.Instance.Networker.disconnected -= OnDisconnect;

			MainThreadManager.Run(() =>
			{
				//check if it was the server that disconnected...
				foreach (var no in sender.NetworkObjectList)
				{
					//Is it the host disconnecting? If so, we need to exit.
					if (no.Owner.IsHost)
					{
						Debug.Log("Server Disconnected");
						DisconnectFromServer();
						return;
					}
				}
			});
		}

		private void DisconnectFromServer()
		{
			networkObject.Networker.Disconnect(false);
			
			UnityEngine.SceneManagement.SceneManager.LoadScene(0);
			networkObject.ClearRpcBuffer();

			if (NetworkManager.Instance != null)
			{
				Destroy(NetworkManager.Instance.gameObject);
			}
		}
	}
}
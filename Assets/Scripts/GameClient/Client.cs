using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking;
using Engine.Utilities;
using TMPro;

namespace GameClient
{
	public class Client : ClientBehavior
	{
		public MeshRenderer meshRenderer;
		public TextMeshProUGUI nameText;

		private ClientPositionSorter clientSorter;
		public ClientPositionSorter ClientSorter
		{
			get
			{
				if (clientSorter == null)
					clientSorter = FindObjectOfType<ClientPositionSorter>();
				return clientSorter;
			}
		}

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


			networkObject.SendRpc(RPC_FINISHED_JOINING, Receivers.All, Id);
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


		//RPC
		public override void FinishedJoining(RpcArgs args)
		{
			uint senderId = args.GetNext<uint>();

			ClientSorter.SortClients();
			if (senderId == NetworkHub.MyClient.Id)
				nameText.SetText("<u>"+Name+ "</u>");
			else
				nameText.SetText(Name);
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
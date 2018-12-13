using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking;
using Engine.Utilities;
using TMPro;
using MeshGeneration;
using Engine;

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
				return "Host-" + Id;
			}
		}

		public uint Id
		{
			get
			{
				return networkObject.NetworkId - 3;
			}
		}

		public Color Color
		{
			get
			{
				return ColorUtils.ColorById((int)Id);
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

			

			if (networkObject.IsOwner)
			{
				networkObject.SendRpc(RPC_FINISHED_JOINING, Receivers.All, Id);
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


		//RPC
		public override void FinishedJoining(RpcArgs args)
		{
			//uint senderId = args.GetNext<uint>();
			ClientSorter.NewClientJoined();

		}

		public void SendChunksRequest(int start, int end)
		{
			networkObject.SendRpc(RPC_REQUEST_CHUNKS, Receivers.Owner, start, end, Id);
		}

		public override void RequestChunks(RpcArgs args)
		{
			int start = args.GetNext<int>();
			int end = args.GetNext<int>();
			uint id = args.GetNext<uint>();

			//only generate if request is for us
			if (Id == id)
			{
				MeshManager.Instance.GenerateChunks(start, end, OnChunksGenerated);
			}
		}

		private void OnChunksGenerated(List<ChunkData> chunks)
		{
			//byte serialize the chunks and send them over the network
			var serializedChunks = MeshManager.Instance.SerializeChunks(chunks);
			foreach(var sc in serializedChunks)
			{
				networkObject.SendRpc(RPC_DELIVER_CHUNK, Receivers.All, sc.coord, sc.heightMap, Id);
			}

			//add to our own chunk display
			//MeshManager.Instance.AddChunks(chunks, Id);
		}

		public override void DeliverChunk(RpcArgs args)
		{
			byte[] coordBytes = args.GetNext<byte[]>();
			byte[] heightMapBytes = args.GetNext<byte[]>();
			uint senderId = args.GetNext<uint>();

			SerializedChunk serializedChunk = new SerializedChunk(coordBytes, heightMapBytes);
			ChunkData chunk = serializedChunk.Deserialize();
			MeshManager.Instance.AddChunk(chunk, senderId);
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
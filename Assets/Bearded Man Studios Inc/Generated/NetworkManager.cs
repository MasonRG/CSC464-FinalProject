using BeardedManStudios.Forge.Networking.Generated;
using System;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Unity
{
	public partial class NetworkManager : MonoBehaviour
	{
		public delegate void InstantiateEvent(INetworkBehavior unityGameObject, NetworkObject obj);
		public event InstantiateEvent objectInitialized;
		private BMSByte metadata = new BMSByte();

		public GameObject[] ClientManagerNetworkObject = null;
		public GameObject[] ClientNetworkObject = null;
		public GameObject[] ConsoleNetworkObject = null;

		private void SetupObjectCreatedEvent()
		{
			Networker.objectCreated += CaptureObjects;
		}

		private void OnDestroy()
		{
		    if (Networker != null)
				Networker.objectCreated -= CaptureObjects;
		}
		
		private void CaptureObjects(NetworkObject obj)
		{
			if (obj.CreateCode < 0)
				return;
				
			if (obj is ClientManagerNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (ClientManagerNetworkObject.Length > 0 && ClientManagerNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(ClientManagerNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<ClientManagerBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is ClientNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (ClientNetworkObject.Length > 0 && ClientNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(ClientNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<ClientBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
			else if (obj is ConsoleNetworkObject)
			{
				MainThreadManager.Run(() =>
				{
					NetworkBehavior newObj = null;
					if (!NetworkBehavior.skipAttachIds.TryGetValue(obj.NetworkId, out newObj))
					{
						if (ConsoleNetworkObject.Length > 0 && ConsoleNetworkObject[obj.CreateCode] != null)
						{
							var go = Instantiate(ConsoleNetworkObject[obj.CreateCode]);
							newObj = go.GetComponent<ConsoleBehavior>();
						}
					}

					if (newObj == null)
						return;
						
					newObj.Initialize(obj);

					if (objectInitialized != null)
						objectInitialized(newObj, obj);
				});
			}
		}

		private void InitializedObject(INetworkBehavior behavior, NetworkObject obj)
		{
			if (objectInitialized != null)
				objectInitialized(behavior, obj);

			obj.pendingInitialized -= InitializedObject;
		}

		[Obsolete("Use InstantiateClientManager instead, its shorter and easier to type out ;)")]
		public ClientManagerBehavior InstantiateClientManagerNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(ClientManagerNetworkObject[index]);
			var netBehavior = go.GetComponent<ClientManagerBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<ClientManagerBehavior>().networkObject = (ClientManagerNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateClient instead, its shorter and easier to type out ;)")]
		public ClientBehavior InstantiateClientNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(ClientNetworkObject[index]);
			var netBehavior = go.GetComponent<ClientBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<ClientBehavior>().networkObject = (ClientNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		[Obsolete("Use InstantiateConsole instead, its shorter and easier to type out ;)")]
		public ConsoleBehavior InstantiateConsoleNetworkObject(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(ConsoleNetworkObject[index]);
			var netBehavior = go.GetComponent<ConsoleBehavior>();
			var obj = netBehavior.CreateNetworkObject(Networker, index);
			go.GetComponent<ConsoleBehavior>().networkObject = (ConsoleNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}

		/// <summary>
		/// Instantiate an instance of ClientManager
		/// </summary>
		/// <returns>
		/// A local instance of ClientManagerBehavior
		/// </returns>
		/// <param name="index">The index of the ClientManager prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public ClientManagerBehavior InstantiateClientManager(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(ClientManagerNetworkObject[index]);
			var netBehavior = go.GetComponent<ClientManagerBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<ClientManagerBehavior>().networkObject = (ClientManagerNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of Client
		/// </summary>
		/// <returns>
		/// A local instance of ClientBehavior
		/// </returns>
		/// <param name="index">The index of the Client prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public ClientBehavior InstantiateClient(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(ClientNetworkObject[index]);
			var netBehavior = go.GetComponent<ClientBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<ClientBehavior>().networkObject = (ClientNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
		/// <summary>
		/// Instantiate an instance of Console
		/// </summary>
		/// <returns>
		/// A local instance of ConsoleBehavior
		/// </returns>
		/// <param name="index">The index of the Console prefab in the NetworkManager to Instantiate</param>
		/// <param name="position">Optional parameter which defines the position of the created GameObject</param>
		/// <param name="rotation">Optional parameter which defines the rotation of the created GameObject</param>
		/// <param name="sendTransform">Optional Parameter to send transform data to other connected clients on Instantiation</param>
		public ConsoleBehavior InstantiateConsole(int index = 0, Vector3? position = null, Quaternion? rotation = null, bool sendTransform = true)
		{
			var go = Instantiate(ConsoleNetworkObject[index]);
			var netBehavior = go.GetComponent<ConsoleBehavior>();

			NetworkObject obj = null;
			if (!sendTransform && position == null && rotation == null)
				obj = netBehavior.CreateNetworkObject(Networker, index);
			else
			{
				metadata.Clear();

				if (position == null && rotation == null)
				{
					byte transformFlags = 0x1 | 0x2;
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);
					ObjectMapper.Instance.MapBytes(metadata, go.transform.position, go.transform.rotation);
				}
				else
				{
					byte transformFlags = 0x0;
					transformFlags |= (byte)(position != null ? 0x1 : 0x0);
					transformFlags |= (byte)(rotation != null ? 0x2 : 0x0);
					ObjectMapper.Instance.MapBytes(metadata, transformFlags);

					if (position != null)
						ObjectMapper.Instance.MapBytes(metadata, position.Value);

					if (rotation != null)
						ObjectMapper.Instance.MapBytes(metadata, rotation.Value);
				}

				obj = netBehavior.CreateNetworkObject(Networker, index, metadata.CompressBytes());
			}

			go.GetComponent<ConsoleBehavior>().networkObject = (ConsoleNetworkObject)obj;

			FinalizeInitialization(go, netBehavior, obj, position, rotation, sendTransform);
			
			return netBehavior;
		}
	}
}

using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

namespace GameServer
{
	/// <summary>
	/// Instantiates the ClientManager and Console network objects into the game. Only server needs to 
	/// execute this as these objects only have 1 instance across the network.
	/// </summary>
	public class GameInitialization : MonoBehaviour
	{
		private void Start()
		{
			if (NetworkManager.Instance.IsServer)
			{
				NetworkManager.Instance.InstantiateConsole();
				NetworkManager.Instance.InstantiateClientManager();
			}
		}
	}
}
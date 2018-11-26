using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using GameClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Console = GameConsole.Console;

namespace Engine.Utilities
{
	public static class NetworkHub
	{
		private static Console gameConsole;
		public static Console GameConsole
		{
			get
			{
				if (gameConsole == null)
				{
					gameConsole = FindBehaviour<Console>();
				}
				return gameConsole;
			}
		}

		private static ClientManager clientManager;
		public static ClientManager ClientManager
		{
			get
			{
				if (clientManager == null)
				{
					clientManager = FindBehaviour<ClientManager>();
				}
				return clientManager;
			}
		}

		private static Client myClient;
		public static Client MyClient
		{
			get
			{
				if (myClient == null)
				{
					myClient = FindMyClient();
				}
				return myClient;
			}
		}

		public static NetworkingPlayer MyNetworkingPlayer
		{
			get
			{
				return NetworkManager.Instance.Networker.Me;
			}
		}

		public static uint MyPlayerID
		{
			get
			{
				return MyNetworkingPlayer.NetworkId;
			}
		}

		public static bool MessageConsoleActive
		{
			get
			{
				return GameConsole == null ? false : GameConsole.IsTyping;
			}
		}

		/// <summary>
		/// Find a NetworkBehavior out of the list of networkObjects.
		/// Provide a predicate function that will only allow the behaviour to be selected if the predicate evaluates to true when passed
		/// the networkObject we are considering. Leave the predicate null to just accept the first matching behaviour.
		/// </summary>
		public static Tbehaviour FindBehaviour<Tbehaviour>(Func<NetworkObject, bool> predicate = null) where Tbehaviour : NetworkBehavior
		{
			foreach (var obj in NetworkManager.Instance.Networker.NetworkObjectList)
			{
				if (predicate == null || predicate(obj))
				{
					Tbehaviour behaviour = obj.AttachedBehavior as Tbehaviour;
					if (behaviour != null)
					{
						return behaviour;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Find all NetworkBehaviors out of the list of networkObjects.
		/// Provide a predicate function that will only allow the behaviours to be selected if the predicate evaluates to true when passed
		/// the networkObject we are considering. Leave the predicate null to just accept all matching behaviours.
		/// </summary>
		public static List<Tbehaviour> FindAllBehaviours<Tbehaviour>(Func<NetworkObject, bool> predicate = null) where Tbehaviour : NetworkBehavior
		{
			List<Tbehaviour> tList = new List<Tbehaviour>();

			foreach (var obj in NetworkManager.Instance.Networker.NetworkObjectList)
			{
				if (predicate == null || predicate(obj))
				{
					Tbehaviour behaviour = obj.AttachedBehavior as Tbehaviour;
					if (behaviour != null)
						tList.Add(behaviour);
				}
			}
			return tList;
		}

		/// <summary>
		/// Returns our local Client.
		/// </summary>
		public static Client FindMyClient()
		{
			return FindBehaviour<Client>((networkObject) => { return networkObject.IsOwner; });
		}


		/// <summary>
		/// Evaluate the predicate function until it returns true. If predicate is true, calls 'onSuccess', otherwise our search timed out calls 'onTimeout'.
		/// No action is performed if 'predicate' or 'onSuccess' are null.
		/// 'waitBetweenAttempts' is the time in seconds to wait before trying the predicate again. 'maxWaitTime' determines how long we will try the operation before
		/// deciding we have timed out.
		/// </summary>
		/// <param name="predicate">The function to test whether we have succeeded or not.</param>
		/// <param name="onSuccess">The callback to invoke when we succeed.</param>
		/// <param name="onTimeout">The callback to invoke when we fail (from too many failed attempts).</param>
		/// <param name="maxWaitTime">How long (seconds) to attempt the predicate operation before we declare a timeout.</param>
		/// <param name="waitBetweenAttempts">How long (seconds) to wait between attempts.</param>
		public static void TryUntilTrueOrTimeout(Func<bool> predicate, Action onSuccess, Action onTimeout = null, float maxWaitTime = 5f, float waitBetweenAttempts = 0.1f)
		{
			if (predicate == null || onSuccess == null) return;
			TryUntilTrueOrTimeout_Internal(predicate, onSuccess, onTimeout, 1, (int)(Math.Max(0.01f, maxWaitTime) / Math.Max(0.01f, waitBetweenAttempts)), waitBetweenAttempts);
		}

		private static void TryUntilTrueOrTimeout_Internal(Func<bool> predicate, Action onSuccess, Action onTimeout, int attempt, int maxAttempts, float wait)
		{
			if (attempt >= maxAttempts)
			{
				if (onTimeout != null) onTimeout();
			}
			else if (predicate())
			{
				if (onSuccess != null) onSuccess();
			}
			else
			{
				RoutineRunner.StartTimer(() => TryUntilTrueOrTimeout_Internal(predicate, onSuccess, onTimeout, ++attempt, maxAttempts, wait), wait);
			}
		}
	}
}

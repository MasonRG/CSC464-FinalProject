using BeardedManStudios.Forge.Networking.Unity;
using Engine.Utilities;
using GameClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientPositionSorter : MonoBehaviour
{
	public Transform circle;

	public float edgeBorder = 1f;

	private float radius;


	private void Awake()
	{
		var scl = circle.localScale;
		scl.x = scl.z = Mathf.Max(scl.x, scl.z);
		circle.localScale = scl;

		radius = (scl.x / 2) - edgeBorder;
	}

	int netWaitCount = 0;
	public void NewClientJoined()
	{
		var clients = NetworkHub.FindAllBehaviours<Client>();
		foreach (var client in clients)
		{
			if (!client.networkObject.NetworkReady)
			{
				client.networkStarted += Client_networkStarted;
				netWaitCount++;
			}
		}

		if (netWaitCount == 0)
			SortClients();
	}

	private void Client_networkStarted(NetworkBehavior behavior)
	{
		var client = behavior as Client;
		client.networkStarted -= Client_networkStarted;
		netWaitCount--;

		if (netWaitCount == 0)
			SortClients();
	}

	/// <summary>
	/// Takes all clients in the server and places them around the circumference of the circle.
	/// Based on client NetworkingPlayer id.
	/// </summary>
	public void SortClients()
	{
		var clients = NetworkHub.FindAllBehaviours<Client>();
		clients.Sort((a, b) => a.Id.CompareTo(b.Id)); //sort in ascending order of Id

		var angle = 360f / clients.Count;
		for(int i = 0; i < clients.Count; i++)
		{
			//set location
			clients[i].transform.parent = circle.parent;
			clients[i].transform.localPosition = Quaternion.AngleAxis(angle * i, Vector3.up) * Vector3.forward * radius;

			//set name
			if (clients[i].Id == NetworkHub.MyClient.Id) clients[i].nameText.SetText("<u>" + clients[i].Name + "</u>");
			else clients[i].nameText.SetText(clients[i].Name);

			//set color
			clients[i].meshRenderer.material.color = clients[i].Color;
		}
	}

	
}

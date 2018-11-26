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
			clients[i].transform.position = Quaternion.AngleAxis(angle * i, Vector3.up) * Vector3.forward * radius;
		}
	}
}

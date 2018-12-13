using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ColorTester : MonoBehaviour
{
	public MeshRenderer[] renderers;
	public Color[] colors;

	private void Update()
	{
		for(int i = 0; i < renderers.Length; i++)
		{
			renderers[i].material.color = colors[i];
		}
	}
}

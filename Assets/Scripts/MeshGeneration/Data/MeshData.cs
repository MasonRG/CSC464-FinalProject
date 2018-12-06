using UnityEngine;

namespace MeshGeneration.Data
{
	public class MeshData
	{
		public Vector3[] vertices;
		public int[] triangles;
		public Vector2[] uvs;

		int triangleIndex;

		public MeshData(int meshWidth, int meshHeight)
		{
			vertices = new Vector3[meshWidth * meshHeight];
			uvs = new Vector2[meshWidth * meshHeight];
			triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
		}

		public void AddTriangle(int a, int b, int c)
		{
			triangles[triangleIndex + 0] = a;
			triangles[triangleIndex + 1] = b;
			triangles[triangleIndex + 2] = c;
			triangleIndex += 3;
		}

		public Mesh CreateMesh()
		{
			Mesh mesh = new Mesh
			{
				vertices = vertices,
				triangles = triangles,
				uv = uvs,
				normals = CalculateNormals()
			};
			
			return mesh;
		}

		private Vector3[] CalculateNormals()
		{
			Vector3[] vertexNormals = new Vector3[vertices.Length];
			int triangleCount = triangles.Length / 3;

			for (int i = 0; i < triangleCount; i++)
			{
				int normalTriangleIndex = i * 3;
				int vertexIndexA = triangles[normalTriangleIndex + 0];
				int vertexIndexB = triangles[normalTriangleIndex + 1];
				int vertexIndexC = triangles[normalTriangleIndex + 2];

				Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
				vertexNormals[vertexIndexA] += triangleNormal;
				vertexNormals[vertexIndexB] += triangleNormal;
				vertexNormals[vertexIndexC] += triangleNormal;
			}

			for (int i = 0; i < vertexNormals.Length; i++)
			{
				vertexNormals[i].Normalize();
			}

			return vertexNormals;
		}

		private Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
		{
			Vector3 pointA = vertices[indexA];
			Vector3 pointB = vertices[indexB];
			Vector3 pointC = vertices[indexC];

			Vector3 sideAB = pointB - pointA;
			Vector3 sideAC = pointC - pointA;
			return Vector3.Cross(sideAB, sideAC).normalized;
		}
	}
}
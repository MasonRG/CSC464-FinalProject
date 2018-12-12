using MeshGeneration;
using System;
using UnityEngine;

namespace Engine
{
	public struct SerializedChunk
	{
		public readonly byte[] coord;
		public readonly byte[] vertices;
		public readonly byte[] triangles;

		public SerializedChunk(ChunkData data)
		{
			coord = ByteSerializer.Vector2IntToBytes(data.coord);
			vertices = ByteSerializer.Vector3ArrayToBytes(data.vertices);
			triangles = ByteSerializer.IntArrayToBytes(data.triangles);
		}

		public ChunkData Deserialize()
		{
			Vector2Int c = ByteSerializer.BytesToVector2Int(coord);
			Vector3[] v = ByteSerializer.BytesToVector3Array(vertices);
			int[] t = ByteSerializer.BytesToIntArray(triangles);
			return new ChunkData(c, v, t);
		}
	}

	public static class ByteSerializer
	{
		//we want to use 16-bit vals for the vector3 (so 48bit per V3; rather than 96bit), so we can only scale up 100 or we will exceed short max value
		public const int SCALE_FACTOR = 100; 

		public static byte[] Vector3ArrayToBytes(Vector3[] vs)
		{
			int size = sizeof(short) * 3;

			byte[] vals = new byte[vs.Length * size];
			for (int i = 0; i < vs.Length; i++)
			{
				
				var shX = (short)(vs[i].x * SCALE_FACTOR);
				var shY = (short)(vs[i].y * SCALE_FACTOR);
				var shZ = (short)(vs[i].z * SCALE_FACTOR);

				var x = BitConverter.GetBytes(shX);
				var y = BitConverter.GetBytes(shY);
				var z = BitConverter.GetBytes(shZ);

				var index = i * size;
				vals[index + 0] = x[0];
				vals[index + 1] = x[1];
				vals[index + 2] = y[0];
				vals[index + 3] = y[1];
				vals[index + 4] = z[0];
				vals[index + 5] = z[1];
			}

			return vals;
		}

		public static Vector3[] BytesToVector3Array(byte[] bytes)
		{
			int size = sizeof(short) * 3;

			Vector3[] vals = new Vector3[bytes.Length / size];
			for(int i = 0; i < vals.Length; i++)
			{
				var index = i * size;

				var shX = BitConverter.ToInt16(bytes, index + 0);
				var shY = BitConverter.ToInt16(bytes, index + sizeof(short));
				var shZ = BitConverter.ToInt16(bytes, index + sizeof(short)*2);

				var flX = ((float)shX) / SCALE_FACTOR;
				var flY = ((float)shY) / SCALE_FACTOR;
				var flZ = ((float)shZ) / SCALE_FACTOR;

				vals[i] = new Vector3(flX, flY, flZ);
			}

			return vals;
		}

		public static byte[] IntArrayToBytes(int[] ints)
		{
			int size = sizeof(int);
			byte[] vals = new byte[ints.Length * size];
			for(int i = 0; i < ints.Length; i++)
			{
				int index = i * size;
				var intBytes = BitConverter.GetBytes(ints[i]);
				vals[index + 0] = intBytes[0];
				vals[index + 1] = intBytes[1];
				vals[index + 2] = intBytes[2];
				vals[index + 3] = intBytes[3];
			}

			return vals;
		}

		public static int[] BytesToIntArray(byte[] bytes)
		{
			int size = sizeof(int);
			int[] vals = new int[bytes.Length / size];
			for (int i = 0; i < vals.Length; i++)
			{
				int index = i * size;
				vals[i] = BitConverter.ToInt32(bytes, index);
			}

			return vals;
		}

		public static byte[] Vector2IntToBytes(Vector2Int vector)
		{
			var x = BitConverter.GetBytes(vector.x);
			var y = BitConverter.GetBytes(vector.y);

			var vals = new byte[x.Length + y.Length];
			vals[0] = x[0];
			vals[1] = x[1];
			vals[2] = x[2];
			vals[3] = x[3];
			vals[4] = y[0];
			vals[5] = y[1];
			vals[6] = y[2];
			vals[7] = y[3];

			return vals;
		}

		public static Vector2Int BytesToVector2Int(byte[] bytes)
		{
			var x = BitConverter.ToInt32(bytes, 0);
			var y = BitConverter.ToInt32(bytes, 4);

			return new Vector2Int(x, y);
		}

	}
}
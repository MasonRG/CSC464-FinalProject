using MeshGeneration;
using System;
using UnityEngine;

namespace Engine
{
	public struct SerializedChunk
	{
		public readonly byte[] coord;
		public readonly byte[] heightMap;

		public SerializedChunk(ChunkData data)
		{
			coord = ByteSerializer.Vector2IntToBytes(data.coord);
			heightMap = ByteSerializer.HeightMapToBytes(data.heightMap.heightMap);
		}

		public SerializedChunk(byte[] coord, byte[] heightMap)
		{
			this.coord = coord;
			this.heightMap = heightMap;
		}

		public ChunkData Deserialize()
		{
			Vector2Int c = ByteSerializer.BytesToVector2Int(coord);
			float[,] h = ByteSerializer.BytesToHeightMap(heightMap);
			return new ChunkData(c, h);
		}
	}

	public static class ByteSerializer
	{
		public const int SCALE_FACTOR = 15; 

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

		public static byte[] HeightMapToBytes(float[,] heightMap)
		{
			//first lets just do a naive conversion

			int size = sizeof(byte);

			int width = heightMap.GetLength(0);
			int height = heightMap.GetLength(1);

			int index = 0;
			byte[] vals = new byte[width * height * size];
			for(int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					var val = (byte)(heightMap[x, y] * SCALE_FACTOR);

					vals[index] = val;
					index++;
				}
			}

			return vals;
		}

		public static float[,] BytesToHeightMap(byte[] bytes)
		{
			int size = sizeof(byte);

			int sqrt = (int)Mathf.Sqrt(bytes.Length / size);
			float[,] heightMap = new float[sqrt, sqrt];

			int index = 0;
			for (int x = 0; x < sqrt; x++)
			{
				for (int y = 0; y < sqrt; y++)
				{
					heightMap[x, y] = ((float)bytes[index]) / SCALE_FACTOR;
					index += size;
				}
			}

			return heightMap;
		}

	}
}
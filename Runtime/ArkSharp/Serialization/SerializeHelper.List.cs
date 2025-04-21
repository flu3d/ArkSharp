using System;
using System.Collections.Generic;

namespace ArkSharp
{
	public static partial class SerializeHelper
	{
		public static void WriteList(ref this Serializer s, IReadOnlyList<bool> list)
		{
			int count = (list != null) ? list.Count : 0;
			s.WriteLength(count);

			for (int i = 0; i < count; i++)
				s.Write(list[i]);
		}

		public static void WriteList(ref this Serializer s, IReadOnlyList<float> list)
		{
			int count = (list != null) ? list.Count : 0;
			s.WriteLength(count);

			for (int i = 0; i < count; i++)
				s.Write(list[i]);
		}

		public static void WriteList(ref this Serializer s, IReadOnlyList<double> list)
		{
			int count = (list != null) ? list.Count : 0;
			s.WriteLength(count);

			for (int i = 0; i < count; i++)
				s.Write(list[i]);
		}

		public static void WriteList<T>(ref this Serializer s, IReadOnlyList<T> list) where T : unmanaged, Enum
		{
			int count = (list != null) ? list.Count : 0;
			s.WriteLength(count);

			for (int i = 0; i < count; i++)
				s.Write(list[i]);
		}

		public static void WriteList(ref this Serializer s, IReadOnlyList<short> list)
		{
			int count = (list != null) ? list.Count : 0;
			s.WriteLength(count);

			for (int i = 0; i < count; i++)
				s.Write(list[i]);
		}

		public static void WriteList(ref this Serializer s, IReadOnlyList<ushort> list)
		{
			int count = (list != null) ? list.Count : 0;
			s.WriteLength(count);

			for (int i = 0; i < count; i++)
				s.Write(list[i]);
		}

		public static void WriteList(ref this Serializer s, IReadOnlyList<int> list)
		{
			int count = (list != null) ? list.Count : 0;
			s.WriteLength(count);

			for (int i = 0; i < count; i++)
				s.Write(list[i]);
		}

		public static void WriteList(ref this Serializer s, IReadOnlyList<uint> list)
		{
			int count = (list != null) ? list.Count : 0;
			s.WriteLength(count);

			for (int i = 0; i < count; i++)
				s.Write(list[i]);
		}

		public static void WriteList(ref this Serializer s, IReadOnlyList<long> list)
		{
			int count = (list != null) ? list.Count : 0;
			s.WriteLength(count);

			for (int i = 0; i < count; i++)
				s.Write(list[i]);
		}

		public static void WriteList(ref this Serializer s, IReadOnlyList<string> list)
		{
			int count = (list != null) ? list.Count : 0;
			s.WriteLength(count);

			for (int i = 0; i < count; i++)
				s.Write(list[i]);
		}

		public static void WriteObjList<T>(ref this Serializer s, IReadOnlyList<T> list) where T : class, ISerializable
		{
			int count = (list != null) ? list.Count : 0;
			s.WriteLength(count);

			for (int i = 0; i < count; i++)
				s.WriteObject(list[i]);
		}

		public static void ReadList(ref this Deserializer s, out List<bool> list)
		{
			list = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			list = new List<bool>(count);
			for (int i = 0; i < count; i++)
			{
				s.Read(out bool val);
				list.Add(val);
			}
		}

		public static void ReadList(ref this Deserializer s, out List<float> list)
		{
			list = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			list = new List<float>(count);
			for (int i = 0; i < count; i++)
			{
				s.Read(out float val);
				list.Add(val);
			}
		}

		public static void ReadList(ref this Deserializer s, out List<double> list)
		{
			list = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			list = new List<double>(count);
			for (int i = 0; i < count; i++)
			{
				s.Read(out double val);
				list.Add(val);
			}
		}

		public static void ReadList(ref this Deserializer s, out List<short> list)
		{
			list = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			list = new List<short>(count);
			for (int i = 0; i < count; i++)
			{
				s.Read(out short val);
				list.Add(val);
			}
		}

		public static void ReadList(ref this Deserializer s, out List<ushort> list)
		{
			list = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			list = new List<ushort>(count);
			for (int i = 0; i < count; i++)
			{
				s.Read(out ushort val);
				list.Add(val);
			}
		}

		public static void ReadList(ref this Deserializer s, out List<int> list)
		{
			list = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			list = new List<int>(count);
			for (int i = 0; i < count; i++)
			{
				s.Read(out int val);
				list.Add(val);
			}
		}

		public static void ReadList(ref this Deserializer s, out List<uint> list)
		{
			list = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			list = new List<uint>(count);
			for (int i = 0; i < count; i++)
			{
				s.Read(out uint val);
				list.Add(val);
			}
		}

		public static void ReadList(ref this Deserializer s, out List<long> list)
		{
			list = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			list = new List<long>(count);
			for (int i = 0; i < count; i++)
			{
				s.Read(out long val);
				list.Add(val);
			}
		}

		public static void ReadList(ref this Deserializer s, out List<string> list)
		{
			list = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			list = new List<string>(count);
			for (int i = 0; i < count; i++)
			{
				s.Read(out string val);
				list.Add(val);
			}
		}

		public static void ReadList<T>(ref this Deserializer s, out List<T> list) where T : unmanaged, Enum
		{
			list = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			list = new List<T>(count);
			for (int i = 0; i < count; i++)
			{
				s.Read(out T val);
				list.Add(val);
			}
		}

		public static void ReadObjList<T>(ref this Deserializer s, out List<T> list) where T : class, IDeserializable, new()
		{
			list = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			list = new List<T>(count);
			for (int i = 0; i < count; i++)
			{
				s.ReadObject<T>(out var val);
				list.Add(val);
			}
		}

		public static void ReadList(ref this Deserializer s, out bool[] list)
		{
			list = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			list = new bool[count];
			for (int i = 0; i < count; i++)
				s.Read(out list[i]);
		}

		public static void ReadList(ref this Deserializer s, out float[] list)
		{
			list = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			list = new float[count];
			for (int i = 0; i < count; i++)
				s.Read(out list[i]);
		}

		public static void ReadList(ref this Deserializer s, out double[] list)
		{
			list = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			list = new double[count];
			for (int i = 0; i < count; i++)
				s.Read(out list[i]);
		}


		public static void ReadList(ref this Deserializer s, out short[] list)
		{
			list = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			list = new short[count];
			for (int i = 0; i < count; i++)
				s.Read(out list[i]);
		}

		public static void ReadList(ref this Deserializer s, out ushort[] list)
		{
			list = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			list = new ushort[count];
			for (int i = 0; i < count; i++)
				s.Read(out list[i]);
		}

		public static void ReadList(ref this Deserializer s, out int[] list)
		{
			list = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			list = new int[count];
			for (int i = 0; i < count; i++)
				s.Read(out list[i]);
		}

		public static void ReadList(ref this Deserializer s, out uint[] list)
		{
			list = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			list = new uint[count];
			for (int i = 0; i < count; i++)
				s.Read(out list[i]);
		}

		public static void ReadList(ref this Deserializer s, out long[] list)
		{
			list = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			list = new long[count];
			for (int i = 0; i < count; i++)
				s.Read(out list[i]);
		}


		public static void ReadList(ref this Deserializer s, out string[] list)
		{
			list = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			list = new string[count];
			for (int i = 0; i < count; i++)
				s.Read(out list[i]);
		}

		public static void ReadList<T>(ref this Deserializer s, out T[] list) where T : unmanaged, Enum
		{
			list = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			list = new T[count];
			for (int i = 0; i < count; i++)
				s.Read(out list[i]);
		}

		public static void ReadObjList<T>(ref this Deserializer s, out T[] list) where T : class, IDeserializable, new()
		{
			list = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			list = new T[count];
			for (int i = 0; i < count; i++)
				s.ReadObject(out list[i]);
		}
	}
}

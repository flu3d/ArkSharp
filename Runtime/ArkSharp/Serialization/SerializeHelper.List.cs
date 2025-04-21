using System;
using System.Collections.Generic;

namespace ArkSharp
{
	public static partial class SerializeHelper
	{
		public static void WriteList(this Serializer s, IReadOnlyList<bool> list)
		{
			int count = (list != null) ? list.Count : 0;
			s.WriteLength(count);

			for (int i = 0; i < count; i++)
				s.Write(list[i]);
		}

		public static void WriteList(this Serializer s, IReadOnlyList<float> list)
		{
			int count = (list != null) ? list.Count : 0;
			s.WriteLength(count);

			for (int i = 0; i < count; i++)
				s.Write(list[i]);
		}

		public static void WriteList(this Serializer s, IReadOnlyList<double> list)
		{
			int count = (list != null) ? list.Count : 0;
			s.WriteLength(count);

			for (int i = 0; i < count; i++)
				s.Write(list[i]);
		}

		public static void WriteList<T>(this Serializer s, IReadOnlyList<T> list) where T : unmanaged, Enum
		{
			int count = (list != null) ? list.Count : 0;
			s.WriteLength(count);

			for (int i = 0; i < count; i++)
				s.Write(list[i]);
		}

		public static void WriteList(this Serializer s, IReadOnlyList<short> list)
		{
			int count = (list != null) ? list.Count : 0;
			s.WriteLength(count);

			for (int i = 0; i < count; i++)
				s.Write(list[i]);
		}

		public static void WriteList(this Serializer s, IReadOnlyList<ushort> list)
		{
			int count = (list != null) ? list.Count : 0;
			s.WriteLength(count);

			for (int i = 0; i < count; i++)
				s.Write(list[i]);
		}

		public static void WriteList(this Serializer s, IReadOnlyList<int> list)
		{
			int count = (list != null) ? list.Count : 0;
			s.WriteLength(count);

			for (int i = 0; i < count; i++)
				s.Write(list[i]);
		}

		public static void WriteList(this Serializer s, IReadOnlyList<uint> list)
		{
			int count = (list != null) ? list.Count : 0;
			s.WriteLength(count);

			for (int i = 0; i < count; i++)
				s.Write(list[i]);
		}

		public static void WriteList(this Serializer s, IReadOnlyList<long> list)
		{
			int count = (list != null) ? list.Count : 0;
			s.WriteLength(count);

			for (int i = 0; i < count; i++)
				s.Write(list[i]);
		}

		public static void WriteList(this Serializer s, IReadOnlyList<string> list)
		{
			int count = (list != null) ? list.Count : 0;
			s.WriteLength(count);

			for (int i = 0; i < count; i++)
				s.Write(list[i]);
		}

		public static void WriteObjList<T>(this Serializer s, IReadOnlyList<T> list) where T : class, ISerializable
		{
			int count = (list != null) ? list.Count : 0;
			s.WriteLength(count);

			for (int i = 0; i < count; i++)
				s.WriteObject(list[i]);
		}

		public static void ReadList(this Deserializer s, out IList<bool> list)
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

		public static void ReadList(this Deserializer s, out IList<float> list)
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

		public static void ReadList(this Deserializer s, out IList<double> list)
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

		public static void ReadList<T>(this Deserializer s, out IList<T> list) where T : unmanaged, Enum
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

		public static void ReadList(this Deserializer s, out IList<short> list)
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

		public static void ReadList(this Deserializer s, out IList<ushort> list)
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

		public static void ReadList(this Deserializer s, out IList<int> list)
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

		public static void ReadList(this Deserializer s, out IList<uint> list)
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

		public static void ReadList(this Deserializer s, out IList<long> list)
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

		public static void ReadList(this Deserializer s, out IList<string> list)
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

		public static void ReadObjList<T>(this Deserializer s, out IList<T> list) where T : class, IDeserializable, new()
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
	}
}

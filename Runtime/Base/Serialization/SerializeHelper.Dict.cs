using System.Collections.Generic;

namespace ArkSharp
{
	public static partial class SerializeHelper
	{
		public static void WriteDict(ref this Serializer s, IReadOnlyDictionary<int, int> dict)
		{
			if (dict == null)
			{
				s.WriteLength(0);
				return;
			}

			s.WriteLength(dict.Count);

			foreach (var kv in dict)
			{
				s.Write(kv.Key);
				s.Write(kv.Value);
			}
		}

		public static void WriteDict(ref this Serializer s, IReadOnlyDictionary<int, string> dict)
		{
			if (dict == null)
			{
				s.WriteLength(0);
				return;
			}

			s.WriteLength(dict.Count);

			foreach (var kv in dict)
			{
				s.Write(kv.Key);
				s.Write(kv.Value);
			}
		}

		public static void WriteDict<T>(ref this Serializer s, IReadOnlyDictionary<int, T> dict) where T : class, ISerializable
		{
			if (dict == null)
			{
				s.WriteLength(0);
				return;
			}

			s.WriteLength(dict.Count);

			foreach (var kv in dict)
			{
				s.Write(kv.Key);
				s.WriteObject(kv.Value);
			}
		}

		public static void WriteDict(ref this Serializer s, IReadOnlyDictionary<string, int> dict)
		{
			if (dict == null)
			{
				s.WriteLength(0);
				return;
			}

			s.WriteLength(dict.Count);

			foreach (var kv in dict)
			{
				s.Write(kv.Key);
				s.Write(kv.Value);
			}
		}

		public static void WriteDict(ref this Serializer s, IReadOnlyDictionary<string, string> dict)
		{
			if (dict == null)
			{
				s.WriteLength(0);
				return;
			}

			s.WriteLength(dict.Count);

			foreach (var kv in dict)
			{
				s.Write(kv.Key);
				s.Write(kv.Value);
			}
		}

		public static void WriteDict<T>(ref this Serializer s, IReadOnlyDictionary<string, T> dict) where T : class, ISerializable
		{
			if (dict == null)
			{
				s.WriteLength(0);
				return;
			}

			s.WriteLength(dict.Count);

			foreach (var kv in dict)
			{
				s.Write(kv.Key);
				s.WriteObject(kv.Value);
			}
		}


		public static void ReadDict(ref this Deserializer s, out Dictionary<int, int> dict)
		{
			dict = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			dict = new Dictionary<int, int>(count);
			for (int i = 0; i < count; i++)
			{
				s.Read(out int key);
				s.Read(out int val);
				dict.Add(key, val);
			}
		}

		public static void ReadDict(ref this Deserializer s, out Dictionary<int, string> dict)
		{
			dict = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			dict = new Dictionary<int, string>(count);
			for (int i = 0; i < count; i++)
			{
				s.Read(out int key);
				s.Read(out string val);
				dict.Add(key, val);
			}
		}

		public static void ReadDict<T>(ref this Deserializer s, out Dictionary<int, T> dict) where T : class, IDeserializable, new()
		{
			dict = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			dict = new Dictionary<int, T>(count);
			for (int i = 0; i < count; i++)
			{
				s.Read(out int key);
				s.ReadObject(out T val);
				dict.Add(key, val);
			}
		}

		public static void ReadDict(ref this Deserializer s, out Dictionary<string, int> dict)
		{
			dict = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			dict = new Dictionary<string, int>(count);
			for (int i = 0; i < count; i++)
			{
				s.Read(out string key);
				s.Read(out int val);
				dict.Add(key, val);
			}
		}

		public static void ReadDict(ref this Deserializer s, out Dictionary<string, string> dict)
		{
			dict = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			dict = new Dictionary<string, string>(count);
			for (int i = 0; i < count; i++)
			{
				s.Read(out string key);
				s.Read(out string val);
				dict.Add(key, val);
			}
		}

		public static void ReadDict<T>(ref this Deserializer s, out Dictionary<string, T> dict) where T : class, IDeserializable, new()
		{
			dict = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			dict = new Dictionary<string, T>(count);
			for (int i = 0; i < count; i++)
			{
				s.Read(out string key);
				s.ReadObject(out T val);
				dict.Add(key, val);
			}
		}
	}
}

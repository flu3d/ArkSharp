using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Ark.Data
{
	public static class BinarySerialization
	{
		public static void WriteRecord(this BinaryWriter writer, Record record)
		{
			var fields = record.GetSerializeFields();
			for (int i = 0; i < fields.Count; i++)
			{
				var field = fields[i];
				var value = field.GetValue(record);

				writer.WriteObject(value, field.memberType);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T ReadRecord<T>(this BinaryReader reader) where T : Record
		{
			return (T)ReadRecord(reader, typeof(T));
		}

		public static Record ReadRecord(this BinaryReader reader, Type type)
		{
			var record = Activator.CreateInstance(type) as Record;

			var fields = record.GetSerializeFields();
			for (int i = 0; i < fields.Count; i++)
			{
				var field = fields[i];

				object value = null;

				if (field.isList)
				{
					var list = field.GetValue(record) as IList;
					reader.ReadList(ref list, field.elementType);
					value = list;
				}
				else
				{
					value = reader.ReadObject(field.memberType);
				}

				field.SetValue(record, value);
			}

			return record;
		}

		private static readonly Dictionary<Type, IReadOnlyList<MemberInfoEx>> _serializeFields = new Dictionary<Type, IReadOnlyList<MemberInfoEx>>();

		private static IReadOnlyList<MemberInfoEx> GetSerializeFields(this Record record)
		{
			var recordType = record.GetType();
			_serializeFields.TryGetValue(recordType, out var result);

			if (result == null)
			{
				result = MemberInfoEx.GetSerializeFields(recordType);
				_serializeFields[recordType] = result;
			}

			return result;
		}

		public static void WriteArray(this BinaryWriter writer, Array array, Type elementType)
		{
			int count = (array != null) ? array.Length : 0;
			writer.WriteIntV(count);

			for (int i = 0; i < count; i++)
				writer.WriteObject(array.GetValue(i), elementType);
		}

		public static void WriteList(this BinaryWriter writer, IList list, Type elementType)
		{
			int count = (list != null) ? list.Count : 0;
			writer.WriteIntV(count);

			for (int i = 0; i < count; i++)
				writer.WriteObject(list[i], elementType);
		}

		public static void ReadArray(this BinaryReader reader, ref Array array, Type elementType)
		{
			int count = reader.ReadIntV();
			array = Array.CreateInstance(elementType, count);

			for (int i = 0; i < count; i++)
				array.SetValue(reader.ReadObject(elementType), i);
		}

		public static void ReadList(this BinaryReader reader, ref IList list, Type elementType)
		{
			if (list == null || list.IsReadOnly)
			{
				var listType = typeof(List<>).MakeGenericType(elementType);
				list = (IList)Activator.CreateInstance(listType);
			}
			else
			{
				list.Clear();
			}

			int count = reader.ReadIntV();
			for (int i = 0; i < count; i++)
				list.Add(reader.ReadObject(elementType));
		}

		public static void WriteObject(this BinaryWriter writer, object obj, Type type)
		{
			_writers.TryGetValue(type, out var func);

			if (func != null)
			{
				func.Invoke(writer, obj);
				return;
			}

			if (type.IsEnum)
			{
				int val = Convert.ToInt32(obj);

				_writers.TryGetValue(typeof(int), out func);
				func.Invoke(writer, obj);
			}
			else if (type.IsGenericType && typeof(IList).IsAssignableFrom(type))
			{
				writer.WriteList((IList)obj, Reflect.GetGenericArgAt(type, 0));
			}
			else if (type.IsArray)
			{
				writer.WriteArray((Array)obj, type.GetElementType());
			}
			else if (typeof(Record).IsAssignableFrom(type))
			{
				writer.WriteRecord((Record)obj);
			}
			else
			{
				throw new Exception($"Serialization.WriteObject({type?.Name}) writeFunc not found");
			}
		}

		public static object ReadObject(this BinaryReader reader, Type type)
		{
			_readers.TryGetValue(type, out var func);

			if (func != null)
				return func.Invoke(reader);

			if (type.IsEnum)
			{
				_readers.TryGetValue(typeof(int), out func);

				var val = func.Invoke(reader);
				return Enum.ToObject(type, val);
			}
			else if (type.IsGenericType && typeof(IList).IsAssignableFrom(type))
			{
				IList list = null;
				reader.ReadList(ref list, Reflect.GetGenericArgAt(type, 0));
				return list;
			}
			else if (type.IsArray)
			{
				Array array = null;
				reader.ReadArray(ref array, type.GetElementType());
				return array;
			}
			else if (typeof(Record).IsAssignableFrom(type))
			{
				return reader.ReadRecord(type);
			}
			else
			{
				throw new Exception($"Serialization.ReadObject({type?.Name}) readFunc not found");
			}
		}

		public static void Register<T>(WriteFunc writeFunc, ReadFunc readFunc)
		{
			Register(typeof(T), writeFunc, readFunc);
		}

		public static void Register(Type type, WriteFunc writeFunc, ReadFunc readFunc)
		{
			_writers[type] = writeFunc;
			_readers[type] = readFunc;
		}

		public delegate void WriteFunc(BinaryWriter writer, object obj);
		public delegate object ReadFunc(BinaryReader reader);

		private static readonly Dictionary<Type, WriteFunc> _writers = new Dictionary<Type, WriteFunc>();
		private static readonly Dictionary<Type, ReadFunc> _readers = new Dictionary<Type, ReadFunc>();

		static BinarySerialization()
		{
			Register<bool>((s, obj) => s.Write((bool)obj), s => s.ReadBoolean());
			Register<char>((s, obj) => s.Write((char)obj), s => s.ReadChar());

			Register<byte>((s, obj) => s.Write((byte)obj), s => s.ReadByte());
			Register<sbyte>((s, obj) => s.Write((sbyte)obj), s => s.ReadSByte());
			Register<short>((s, obj) => s.Write((short)obj), s => s.ReadInt16());
			Register<ushort>((s, obj) => s.Write((ushort)obj), s => s.ReadUInt16());
			Register<int>((s, obj) => s.WriteIntV((int)obj), s => s.ReadIntV());
			Register<uint>((s, obj) => s.WriteUIntV((uint)obj), s => s.ReadUIntV());
			Register<long>((s, obj) => s.Write((long)obj), s => s.ReadInt64());
			Register<ulong>((s, obj) => s.Write((ulong)obj), s => s.ReadUInt64());

			Register<float>((s, obj) => s.Write((float)obj), s => s.ReadSingle());
			Register<double>((s, obj) => s.Write((double)obj), s => s.ReadDouble());
			Register<string>((s, obj) => s.Write((string)obj ?? string.Empty), s => Table.StringPool.Intern(s.ReadString()));
		}
	}
}

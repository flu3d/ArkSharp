using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using CsvHelper;

namespace Ark.Data
{
	/// <summary>
	/// 通用数据表基类
	/// </summary>
	public abstract class Table
	{
		public static class Config
		{
			public static bool StringPooling = true;

#if UNITY_EDITOR || UNITY_STANDALONE
			public static bool LookupWarning = true;
#else
			public static bool LookupWarning = false;
#endif

			public static int CsvHeaderCount = 4;
			public static string CsvDefineField = "#Table";

			public static char[] ListSeparators = new char[] { ',', ';', '|' };
		}

		public static class StringPool
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static void Clear() => pool.Clear();

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal static string Intern(string s)
			{
				if (Config.StringPooling)
					return pool.Intern(s);
				else
					return s;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal static object Intern(object o)
			{
				if (Config.StringPooling && o is string s)
					return pool.Intern(s);
				else
					return o;
			}

			private static readonly Ark.StringPool pool = new Ark.StringPool();
		}

		protected readonly Type _recordType;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected Table(Type recordType) => _recordType = recordType;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public virtual string GetName() => _recordType.Name;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Type GetRecordType() => _recordType;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasNestRecord() => typeof(INest).IsAssignableFrom(_recordType);

		public abstract int Count { get; }
		public abstract object GetRecordAt(int index);
		public abstract void Unload();

		public abstract void LoadCsv(Stream stream);
		public abstract void LoadBinary(Stream stream);
		public abstract void SaveBinary(Stream stream);
		public abstract bool IsLoaded { get; }

		protected virtual void OnLoaded() { }
		protected virtual bool IsStringPooling() => Config.StringPooling;

		protected static readonly Encoding utf8 = new UTF8Encoding(false);
		protected static readonly Encoding utf8bom = new UTF8Encoding(true);
	}

	/// <summary>
	/// 通用泛型数据表
	/// </summary>
	public abstract class Table<T> : Table where T:Record
	{
		public override int Count {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _data.Count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override object GetRecordAt(int index) => _data[index];

		public virtual IReadOnlyList<T> RawData {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _data;
		}

		private bool _loaded;

		public override bool IsLoaded {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _loaded;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override void Unload()
		{
			_data.Clear();
			_loaded = false;
		}

		public override void LoadCsv(Stream stream)
		{
			Unload();

			using (var reader = new StreamReader(stream, utf8bom))
			using (var csv = new CsvReader(reader))
			{
				// 读取首行字段名
				if (!csv.Read())
					return;

				var fields = csv.Context.Record;
				var members = MemberInfoEx.GetMembers(_recordType, fields);
				var defineIndex = Array.IndexOf(fields, Config.CsvDefineField);

				// 跳过其他描述行
				for (int i = 1; i < Config.CsvHeaderCount; i++)
				{
					if (!csv.Read())
						return;
				}

				var stringPooling = IsStringPooling();

				while (csv.Read())
				{
					var recordRaw = csv.Context.Record;
					if (recordRaw == null)
						continue;

					// 跳过定义列以#开头的行
					if (0 <= defineIndex && defineIndex < recordRaw.Length)
					{
						var s = recordRaw[defineIndex];
						if (!string.IsNullOrEmpty(s) && s[0] == Record.PrefixComment)
							continue;
					}

					var record = Activator.CreateInstance<T>();
					for (int i = 0; i < recordRaw.Length; i++)
						members[i]?.SetValue(record, recordRaw[i], stringPooling);
					
					// 序列化之后的动作
					if (record is ISerializeCallback ad)
						ad.OnAfterDeserialize();

					_data.Add(record);
				}
			}

			OnLoaded();
			_loaded = true;
		}

		public override void LoadBinary(Stream stream)
		{
			Unload();

			using (var reader = new BinaryReader(stream, utf8, true))
			{
				int count = reader.ReadIntV();
				for (int i = 0; i < count; i++)
				{
					var record = reader.ReadRecord<T>();

					// 序列化之后的动作
					if (record is ISerializeCallback ad)
						ad.OnAfterDeserialize();

					_data.Add(record);
				}
			}

			OnLoaded();
			_loaded = true;
		}

		public override void SaveBinary(Stream stream)
		{
			using (var writer = new BinaryWriter(stream, utf8, true))
			{
				int count = _data.Count;
				writer.WriteIntV(count);

				foreach (var d in _data)
					writer.WriteRecord(d);
			}
		}

		protected Table() : base(typeof(T)) { }

		protected readonly List<T> _data = new List<T>();
	}
}

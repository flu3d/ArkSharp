using System;
using System.Globalization;
using System.IO;

namespace ArkSharp
{
	/// <summary>
	/// 版本号 Major.Minor.Revised-Timestamp
	/// </summary>
	public struct Version
	{
		/// <summary>
		/// 主版本号
		/// </summary>
		public ushort Major;

		/// <summary>
		/// 次版本号
		/// </summary>
		public ushort Minor;

		/// <summary>
		/// 修订版本号
		/// </summary>
		public ushort Revised;

		/// <summary>
		/// 资源时间戳(0表示未指定)
		/// </summary>
		public uint Timestamp;

		public Version(ushort major, ushort minor, ushort revised, uint timestamp)
		{
			Major = major;
			Minor = minor;
			Revised = revised;
			Timestamp = timestamp;
		}

		public void Reset()
		{
			Major = Minor = Revised = 0;
			Timestamp = 0;
		}

		private static readonly char[] _separators = new char[] { '.', '-' };

		public static Version Parse(string verString)
		{
			var v = new Version();

			if (!string.IsNullOrEmpty(verString))
			{
				var arr = verString.Split(_separators);
				if (arr.Length > 0)
					v.Major = arr[0].To<ushort>();
				if (arr.Length > 1)
					v.Minor = arr[1].To<ushort>();
				if (arr.Length > 2)
					v.Revised = arr[2].To<ushort>();

				if (arr.Length > 3)
					v.Timestamp = ParseTimestamp(arr[3]);
			}

			return v;
		}

		public static uint ParseTimestamp(string strTimestamp)
		{
			uint result = 0;

			if (!string.IsNullOrEmpty(strTimestamp) && 
				DateTime.TryParseExact(strTimestamp, TimestampFormat, 
				CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt))
			{
				result = (uint)(long)(dt.ToUniversalTime() - RealTime.epochTime).TotalSeconds;
			}

			return result;
		}

		public override string ToString() => ToUserString();

		/// <summary>
		/// To short string, without timestamp
		/// </summary>
		public string ToShortString() => $"{Major}.{Minor}.{Revised}";

		/// <summary>
		/// To user-friendly string, with formatted timestamp
		/// </summary>
		public string ToUserString()
		{
			if (Timestamp == 0)
				return ToShortString();

			var dt = RealTime.epochTime.AddSeconds(Timestamp);
			var ds = dt.ToString(TimestampFormat);

			return $"{Major}.{Minor}.{Revised}-{ds}";
		}

		/// <summary>
		/// 将当前版本和输入版本进行比较
		/// </summary>
		public int Compare(Version other, bool compareTimeStamp)
		{
			// 主版本号、次版本号、修订号、时间戳以数值依次比较
			if (Major != other.Major)
				return Major < other.Major ? -1 : 1;

			if (Minor != other.Minor)
				return Minor < other.Minor ? -1 : 1;

			if (Revised != other.Revised)
				return Revised < other.Revised ? -1 : 1;

			if (compareTimeStamp)
			{
				if (Timestamp != other.Timestamp)
					return Timestamp < other.Timestamp ? -1 : 1;
			}

			return 0;
		}

		public void Serialize(Stream stream)
		{
			var w = new BinaryWriter(stream);

			w.WriteUIntV(Major);
			w.WriteUIntV(Minor);
			w.WriteUIntV(Revised);
			w.WriteUIntV(Timestamp);
		}

		public void Deserialize(Stream stream)
		{
			var r = new BinaryReader(stream);

			Major = (ushort)r.ReadUIntV();
			Minor = (ushort)r.ReadUIntV();
			Revised = (ushort)r.ReadUIntV();
			Timestamp = r.ReadUIntV();
		}

		public static readonly Version Empty = new Version();
		public const string TimestampFormat = "yyyyMMddHHmmss";
	}
}

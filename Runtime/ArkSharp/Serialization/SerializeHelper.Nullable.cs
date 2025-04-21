using System;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
	public static partial class SerializeHelper
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Write(ref this Serializer s, bool? val)
		{
			if (val.HasValue)
			{
				s.Write(true);
				s.Write(val.Value);
			}
			else
			{
				s.Write(false);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Write(ref this Serializer s, float? val)
		{
			if (val.HasValue)
			{
				s.Write(true);
				s.Write(val.Value);
			}
			else
			{
				s.Write(false);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Write(ref this Serializer s, double? val)
		{
			if (val.HasValue)
			{
				s.Write(true);
				s.Write(val.Value);
			}
			else
			{
				s.Write(false);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Write<T>(ref this Serializer s, T? val) where T : unmanaged, Enum
		{
			if (val.HasValue)
			{
				s.Write(true);
				s.Write(val.Value);
			}
			else
			{
				s.Write(false);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Write(ref this Serializer s, short? val)
		{
			if (val.HasValue)
			{
				s.Write(true);
				s.Write(val.Value);
			}
			else
			{
				s.Write(false);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Write(ref this Serializer s, ushort? val)
		{
			if (val.HasValue)
			{
				s.Write(true);
				s.Write(val.Value);
			}
			else
			{
				s.Write(false);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Write(ref this Serializer s, int? val)
		{
			if (val.HasValue)
			{
				s.Write(true);
				s.Write(val.Value);
			}
			else
			{
				s.Write(false);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Write(ref this Serializer s, uint? val)
		{
			if (val.HasValue)
			{
				s.Write(true);
				s.Write(val.Value);
			}
			else
			{
				s.Write(false);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Write(ref this Serializer s, long? val)
		{
			if (val.HasValue)
			{
				s.Write(true);
				s.Write(val.Value);
			}
			else
			{
				s.Write(false);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Read(ref this Deserializer s, out bool? result)
		{
			var hasValue = s.ReadBool();
			if (hasValue)
				result = s.ReadBool();
			else
				result = null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Read(ref this Deserializer s, out float? result)
		{
			var hasValue = s.ReadBool();
			if (hasValue)
			{
				s.Read(out float val);
				result = val;
			}
			else
				result = null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Read(ref this Deserializer s, out double? result)
		{
			var hasValue = s.ReadBool();
			if (hasValue)
			{
				s.Read(out double val);
				result = val;
			}
			else
				result = null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Read<T>(ref this Deserializer s, out T? result) where T : unmanaged, Enum
		{
			var hasValue = s.ReadBool();
			if (hasValue)
			{
				s.Read(out T val);
				result = val;
			}
			else
				result = null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Read(ref this Deserializer s, out short? result)
		{
			var hasValue = s.ReadBool();
			if (hasValue)
			{
				s.Read(out short val);
				result = val;
			}
			else
				result = null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Read(ref this Deserializer s, out ushort? result)
		{
			var hasValue = s.ReadBool();
			if (hasValue)
			{
				s.Read(out ushort val);
				result = val;
			}
			else
				result = null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Read(ref this Deserializer s, out int? result)
		{
			var hasValue = s.ReadBool();
			if (hasValue)
			{
				s.Read(out int val);
				result = val;
			}
			else
				result = null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Read(ref this Deserializer s, out uint? result)
		{
			var hasValue = s.ReadBool();
			if (hasValue)
			{
				s.Read(out uint val);
				result = val;
			}
			else
				result = null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Read(ref this Deserializer s, out long? result)
		{
			var hasValue = s.ReadBool();
			if (hasValue)
			{
				s.Read(out long val);
				result = val;
			}
			else
				result = null;
		}
	}
}

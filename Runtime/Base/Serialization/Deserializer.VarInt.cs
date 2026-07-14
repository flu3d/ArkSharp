using System;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
	public ref partial struct Deserializer
	{
		/// <summary>读取变长整数(ZigZag编码)</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long ReadVarIntZg() => ZigZag.Decode(ReadVarUIntImpl());

		/// <summary>读取变长整数(ZigZag编码)</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReadVarIntZg(out long result) => result = ReadVarIntZg();

		/// <summary>读取变长非负整数/summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ulong ReadVarUInt() => ReadVarUIntImpl();

		/// <summary>读取变长非负整数/summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReadVarUInt(out ulong result) => result = ReadVarUInt();

		private ulong ReadVarUIntImpl()
		{
			int p = _position;

			ulong result = 0;
			byte readByte;

			// 前9个字节 
			const int MaxBytesWithoutOverflow = 9;
			for (int shift = 0; shift < MaxBytesWithoutOverflow * 7; shift += 7)
			{
				readByte = _buffer[p++];
				result |= (readByte & 0x7Ful) << shift;

				if (readByte <= 0x7Fu)
				{
					_position = p;
					return result;
				}
			}

			// 第10个字节，只能是0或1
			readByte = _buffer[p++];
			if (readByte > 0b_1u)
				throw new FormatException("Format_Bad7BitInt64");

			result |= (ulong)readByte << (MaxBytesWithoutOverflow * 7);

			_position = p;
			return result;
		}
	}
}

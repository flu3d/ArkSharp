using System;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
	public ref partial struct Serializer
	{
		/// <summary>写入变长整数(ZigZag编码)</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteVarIntZg(long val) => WriteVarUIntImpl(ZigZag.Encode(val));

		/// <summary>写入变长非负整数</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteVarUInt(ulong val) => WriteVarUIntImpl(val);

		private void WriteVarUIntImpl(ulong val)
		{
			unsafe
			{
				var ptr = stackalloc byte[10];
				var input = new Span<byte>(ptr, 10);

				int count = 0;
				while (val >= 0x80)
				{
					input[count++] = (byte)(val | 0x80);
					val >>= 7;
				}
				input[count++] = (byte)val;

				WriteRaw(input.Slice(0, count));
			}
		}
	}
}

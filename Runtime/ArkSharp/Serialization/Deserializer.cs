using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace ArkSharp
{
	/// <summary>
	/// 快速反序列化器（栈上分配，用后即销毁）
	/// 适用于对一段内存buffer进行快速反序列化
	///
	/// var ds = new Deserializer(buffer); // buffer是外部加载的二进制数据
	/// var iVal = ds.Read<int>();
	/// var fVal = ds.Read<float>();
	/// var sVal = ds.ReadString();
	/// </summary>
	public ref struct Deserializer
	{
		private ReadOnlySpan<byte> _buffer;
		private int _position;

		public Deserializer(ReadOnlySpan<byte> buffer)
		{
			_buffer = buffer;
			_position = 0;
		}

		public Deserializer(byte[] buffer, int offset = 0) : this(buffer.AsSpan(offset)) { }
		public Deserializer(byte[] buffer, int offset, int length) : this(buffer.AsSpan(offset, length)) { }

		public int Position => _position;
		public int Length => _buffer.Length;

		/// <summary>
		/// 读取非托管数据(数值、枚举)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Read<T>() where T : unmanaged
		{
			int count = UnsafeHelper.SizeOf<T>();

			T value;

			if (Serializer.UseBigEndian)
			{
				// 默认为小端序，如果需要序列化为大端序则需要做逆序
				Span<byte> input = stackalloc byte[count];
				_buffer.Slice(_position, count).CopyTo(input);
				input.Reverse();

				value = MemoryMarshal.Read<T>(input);
			}
			else
			{
				var input = _buffer.Slice(_position, count);
				value = MemoryMarshal.Read<T>(input);
			}

			_position += count;
			return value;
		}

        /*
		/// <summary>
		/// 读取非托管数据(数值、枚举)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Read<T>(out T value) where T : unmanaged => value = Read<T>();
        */

		/// <summary>
		/// 读取字符串：长度(7bit变长)+utf8字节流
		/// 长度为0则返回string.Empty
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string ReadString()
		{
			var count = (int)ReadVarIntImpl(); // 长度必须是非负整数，不用ZigZag编码
			if (count <= 0)
				return string.Empty;

			var encoding = Encoding.UTF8;

			var input = _buffer.Slice(_position, count);
			var result = encoding.GetString(input);

			_position += count;
			return result;
		}

        /*
		/// <summary>
		/// 读取字符串：长度(7bit变长)+utf8字节流
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReadString(out string value) => value = ReadString();
        */

		/// <summary>
		/// 读取变长整数(ZigZag编码)，只支持小端序列
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long ReadInt64V() => ZigZagHelper.Decode(ReadVarIntImpl());

		/// <summary>
		/// 读取变长非负整数，只支持小端序列
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ulong ReadUInt64V() => ReadVarIntImpl();

		private ulong ReadVarIntImpl()
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

		/// <summary>
		/// 读取字节流，无任何额外信息
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int ReadBytes(byte[] output, int offset, int count) => ReadBytes(output.AsSpan(offset, count));

		/// <summary>
		/// 读取字节流，无任何额外信息
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int ReadBytes(Span<byte> output)
		{
			if (output.IsEmpty)
				return 0;

			int count = output.Length;
			int remain = _buffer.Length - _position;

			if (count > remain)
				count = remain;
			if (count <= 0)
				return 0;

			_buffer.Slice(_position, count).CopyTo(output);
			_position += count;

			return count;
		}
	}
}

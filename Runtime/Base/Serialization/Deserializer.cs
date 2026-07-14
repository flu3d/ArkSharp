using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ArkSharp
{
	/// <summary>
	/// 快速二进制反序列化器（栈上分配，用后即销毁）
	/// 适用于对一段内存buffer进行快速反序列化
	///
	/// var ds = new Deserializer(buffer); // buffer是外部加载的二进制数据
	///
	/// var iVal = ds.Read<int>();
	/// var fVal = ds.Read<float>();
	/// var sVal = ds.ReadString();
	/// </summary>
	public ref partial struct Deserializer
	{
		private ReadOnlySpan<byte> _buffer;
		private int _position;

		internal readonly SerializeOptions options;

		public Deserializer(ReadOnlySpan<byte> buffer, SerializeOptions options = Serializer.DefaultOptions)
		{
			_buffer = buffer;
			_position = 0;
			this.options = options;
		}

		public Deserializer(byte[] buffer, int offset = 0, SerializeOptions options = Serializer.DefaultOptions)
			: this(buffer.AsSpan(offset), options) { }
		public Deserializer(byte[] buffer, int offset, int length, SerializeOptions options = Serializer.DefaultOptions)
			: this(buffer.AsSpan(offset, length), options) { }

		public int Position => _position;
		public int Length => _buffer.Length;


		#region 数值类型读取(自动判定是否变长)

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ReadBool()
		{
			ReadRaw(out byte val);    // 不需要变长支持
			return val != 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Read(out bool result) => result = ReadBool();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Read(out float result) => ReadRaw(out result); // 不需要变长支持

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Read(out double result) => ReadRaw(out result); // 不需要变长支持

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Read<T>(out T result) where T : unmanaged, Enum
		{
			if (options.HasFlag(SerializeOptions.VarInt))
				result = (T)(ValueType)(int)ReadVarIntZg(); // 只支持Enum:int
			else
				ReadRaw(out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Read(out short result)
		{
			if (options.HasFlag(SerializeOptions.VarInt))
				result = (short)ReadVarIntZg();
			else
				ReadRaw(out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Read(out ushort result)
		{
			if (options.HasFlag(SerializeOptions.VarInt))
				result = (ushort)ReadVarIntZg();
			else
				ReadRaw(out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Read(out int result)
		{
			if (options.HasFlag(SerializeOptions.VarInt))
				result = (int)ReadVarIntZg();
			else
				ReadRaw(out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Read(out uint result)
		{
			if (options.HasFlag(SerializeOptions.VarInt))
				result = (uint)ReadVarIntZg();
			else
				ReadRaw(out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Read(out long result)
		{
			if (options.HasFlag(SerializeOptions.VarInt))
				result = ReadVarIntZg();
			else
				ReadRaw(out result);
		}

		#endregion

		#region 按原始内存排布读取数据流

		/// <summary>按原始内存排布读取非托管数据(数值、枚举)</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T ReadRaw<T>() where T : unmanaged
		{
			int count = UnsafeHelper.SizeOf<T>();

			T value;

			// 如果大于1字节，并且开启了大端序模式，则反转字节序
			if (count > 1 && options.HasFlag(SerializeOptions.BigEndian))
			{
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

		/// <summary>按原始内存排布读取非托管数据(数值、枚举)</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReadRaw<T>(out T result) where T : unmanaged => result = ReadRaw<T>();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReadRaw(out byte result) => result = _buffer[_position++];

		/// <summary>
		/// 读取字节流，无任何额外信息
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int ReadRaw(byte[] output, int offset, int count) => ReadRaw(output.AsSpan(offset, count));

		/// <summary>
		/// 读取字节流，无任何额外信息
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int ReadRaw(Span<byte> output)
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

		#endregion
	}
}

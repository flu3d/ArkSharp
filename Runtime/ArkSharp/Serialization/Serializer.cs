using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ArkSharp
{
	[Flags]
	public enum SerializeOptions : byte
	{
		/// <summary>字符串和容器长度启用变长字节数</summary>
		VarLen = 0,
		/// <summary>字符串和容器长度使用2字节定长</summary>
		FixedLen2 = 2,
		/// <summary>字符串和容器长度使用4字节定长</summary>
		FixedLen4 = 4,

		/// <summary>使用大端字节序</summary>
		BigEndian = 0x40,
		/// <summary>启用变长数值类型</summary>
		VarInt = 0x80,
	}

	/// <summary>
	/// 快速二进制反序列化器（栈上分配，用后即销毁）
	/// 适用于快速序列化到一段内存buffer
	///
	/// var bs = new Serializer(buffer);  // 输出到外部分配的定长缓冲区
	/// 或 var bs = new Serializer(1024);  // 输出到内部创建的可变长缓冲区
	///
	/// bs.Write(100);
	/// bs.Write(-1.0f);
	/// bs.WriteString("hello");
	///
	/// var result = ds.GetResult();  // 获取序列化结果
	/// </summary>
	public ref partial struct Serializer
	{
		// 默认开启变长数值，字符串和列表长度固定2字节
		public const SerializeOptions DefaultOptions = SerializeOptions.VarInt | SerializeOptions.FixedLen2;

		internal const int DefaultCapacity = 64;
		internal const int MaxArrayPoolRentalSize = 64 * 1024;

		private Span<byte> _buffer;
		private int _position;

		// 可扩展长度的buffer，
		private byte[] _xbuffer;

		internal readonly SerializeOptions options;

		/// <summary>
		/// 序列化到外部传入的定长缓冲区
		/// </summary>
		public Serializer(Span<byte> buffer, SerializeOptions options = DefaultOptions)
		{
			_xbuffer = null;
			_buffer = buffer;
			_position = 0;
			this.options = options;
		}

		/// <summary>
		/// 序列化到外部传入的定长缓冲区
		/// </summary>
		public Serializer(byte[] buffer, int offset = 0, SerializeOptions options = DefaultOptions)
			: this(buffer.AsSpan(offset), options) { }
		public Serializer(byte[] buffer, int offset, int length, SerializeOptions options = DefaultOptions)
			: this(buffer.AsSpan(offset, length), options) { }

		/// <summary>
		/// 序列化到内部分配的变长缓冲区
		/// </summary>
		public Serializer(int capacity, SerializeOptions options = DefaultOptions)
		{
			if (capacity <= 0)
				capacity = DefaultCapacity;
			if ((uint)capacity > ArrayHelper.MaxLength)
				capacity = ArrayHelper.MaxLength;

			_xbuffer = new byte[capacity];
			_buffer = _xbuffer;
			_position = 0;
			_position = 0;
			this.options = options;
		}

		public int Position => _position;
		public int Capacity => _buffer.Length;

		/// <summary>
		/// 获取序列化结果
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<byte> GetResult() => _buffer.Slice(0, _position);


		#region 数值类型写入(自动判定是否变长)

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(bool value) => WriteRaw((byte)(value ? 1 : 0)); // 不需要变长支持

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(float value) => WriteRaw(value); // 不需要变长支持

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(double value) => WriteRaw(value); // 不需要变长支持

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write<T>(T value) where T : unmanaged, Enum
		{
			if (options.HasFlag(SerializeOptions.VarInt))
				WriteVarIntZg((int)(ValueType)value); // 只支持Enum:int
			else
				WriteRaw(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(short value)
		{
			if (options.HasFlag(SerializeOptions.VarInt))
				WriteVarIntZg(value);
			else
				WriteRaw(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(ushort value)
		{
			if (options.HasFlag(SerializeOptions.VarInt))
				WriteVarIntZg(value);
			else
				WriteRaw(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(int value)
		{
			if (options.HasFlag(SerializeOptions.VarInt))
				WriteVarIntZg(value);
			else
				WriteRaw(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(uint value)
		{
			if (options.HasFlag(SerializeOptions.VarInt))
				WriteVarIntZg(value);
			else
				WriteRaw(value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(long value)
		{
			if (options.HasFlag(SerializeOptions.VarInt))
				WriteVarIntZg(value);
			else
				WriteRaw(value);
		}

		#endregion

		#region 按原始内存排布写入数据流

		/// <summary>按原始内存排布写入非托管数据(数值、枚举)</summary>
		public void WriteRaw<T>(T value) where T : unmanaged
		{
			int count = UnsafeHelper.SizeOf<T>();
			int newPos = _position + count;

			// 检查缓冲区剩余空间
			if (!EnsureCapacity(newPos))
				throw new ArgumentOutOfRangeException();

			var output = _buffer.Slice(_position, count);
			MemoryMarshal.Write(output, ref value);

			// 如果大于1字节，并且开启了大端序模式，则反转字节序
			if (count > 1 && options.HasFlag(SerializeOptions.BigEndian))
				output.Reverse();

			_position = newPos;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteRaw(byte value)
		{
			int newPos = _position + 1;
			if (!EnsureCapacity(newPos))
				throw new ArgumentOutOfRangeException();

			_buffer[_position] = value;
			_position = newPos;
		}

		/// <summary>
		/// 写入字节流，不附加任何额外信息
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteRaw(byte[] input, int offset, int count) => WriteRaw(input.AsSpan(offset, count));

		/// <summary>
		/// 写入字节流，不附加任何额外信息
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteRaw(ReadOnlySpan<byte> input)
		{
			if (input.IsEmpty)
				return;

			int count = input.Length;
			int newPos = _position + count;

			if (!EnsureCapacity(newPos))
				throw new ArgumentOutOfRangeException();

			var output = _buffer.Slice(_position, count);
			input.CopyTo(output);

			_position = newPos;
		}

		/// <summary>
		/// 检查缓冲区剩余空间，如果有需要则尝试进行扩容
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool EnsureCapacity(int count)
		{
			int capacity = _buffer.Length;
			if (count <= capacity)
				return true;

			// 定长缓冲区不可扩展
			if (_xbuffer == null)
				return false;

			if (!ArrayHelper.EnsureCapacity(ref _xbuffer, count))
				return false;

			_buffer = _xbuffer;
			return true;
		}

		#endregion
	}
}

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace ArkSharp
{
	/// <summary>
	/// 快速反序列化器（栈上分配，用后即销毁）
	/// 适用于快速序列化到一段内存buffer
	///
	/// var ds = new Serializer(buffer); // 输出到外部分配的定长缓冲区
	/// 或 var ds = new Serializer(1024);   // 输出到内部创建的变长缓冲区
	/// 
	/// ds.Write(100);
	/// ds.Write(-1.0f);
	/// ds.Write("hello");
	///
	/// var result = ds.GetResult(); // 获取序列化
	/// </summary>
	public ref struct Serializer
	{
		/// <summary>
		/// 是否按大端序进行序列化
		/// </summary>
		public static bool UseBigEndian = false;

		public const int DefaultCapacity = 64;
		private const int MaxArrayPoolRentalSize = 64 * 1024;

		private Span<byte> _buffer;
		private int _position;

		// 可扩展长度的buffer，
		private byte[] _xbuffer;

		/// <summary>
		/// 序列化到外部传入的定长缓冲区
		/// </summary>
		public Serializer(Span<byte> buffer)
		{
			_xbuffer = null;
			_buffer = buffer;
			_position = 0;
		}

		/// <summary>
		/// 序列化到外部传入的定长缓冲区
		/// </summary>
		public Serializer(byte[] buffer, int offset = 0) : this(buffer.AsSpan(offset)) { }

		/// <summary>
		/// 序列化到外部传入的定长缓冲区
		/// </summary>
		public Serializer(byte[] buffer, int offset, int length) : this(buffer.AsSpan(offset, length)) { }

		/// <summary>
		/// 序列化到内部分配的变长缓冲区
		/// </summary>
		public Serializer(int capacity)
		{
			if (capacity <= 0)
				capacity = DefaultCapacity;
			if ((uint)capacity > ArrayHelper.MaxLength)
				capacity = ArrayHelper.MaxLength;

			_xbuffer = new byte[capacity];
			_buffer = _xbuffer;
			_position = 0;
		}

		public int Position => _position;
		public int Capacity => _buffer.Length;

		/// <summary>
		/// 获取序列化结果
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<byte> GetResult() => _buffer.Slice(0, _position);

		/// <summary>
		/// 写入非托管数据(数值、枚举)
		/// </summary>
		public void Write<T>(T value) where T : unmanaged
		{
			int count = UnsafeHelper.SizeOf<T>();
			int newPos = _position + count;

			// 检查缓冲区剩余空间
			if (!EnsureCapacity(newPos))
				throw new ArgumentOutOfRangeException();

			var output = _buffer.Slice(_position, count);
			MemoryMarshal.Write(output, ref value);

			// 默认为小端序，如果需要序列化为大端序则需要做逆序
			if (Serializer.UseBigEndian && count > 1)
				output.Reverse();

			_position = newPos;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(byte value)
		{
			int newPos = _position + 1;
			if (!EnsureCapacity(newPos))
				throw new ArgumentOutOfRangeException();

			_buffer[_position] = value;
			_position = newPos;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write(bool value) => Write((byte)(value ? 1 : 0));

		/// <summary>
		/// 写入字符串：长度(7bit变长)+utf8字节流
		/// </summary>
		public void WriteString(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				Write((byte)0);
				return;
			}

			var encoding = Encoding.UTF8;

			// 参考BinaryWriter代码实现
			// https://github.com/dotnet/runtime/blob/1d1bf92fcf43aa6981804dc53c5174445069c9e4/src/libraries/System.Private.CoreLib/src/System/IO/BinaryWriter.cs#L348C13-L382C10

			if (value.Length <= 127 / 3)
			{
				unsafe
				{
					// Max expansion: each char -> 3 bytes, so 127 bytes max of data, +1 for length prefix
					var ptr = stackalloc byte[128];
					var input = new Span<byte>(ptr, 128);

					int count = encoding.GetBytes(value, input.Slice(1));
					input[0] = (byte)count; // skip WriteVarIntImpl((ulong)count);

					// 不可直接用 Span<byte> input = stackalloc byte[128];
					// 会导致WriteBytes(input)编译错误CS8350，或unsafe编译警告
					WriteBytes(input.Slice(0, count + 1));
				}
			}
			else if (value.Length <= MaxArrayPoolRentalSize / 3)
			{
				var input = ArrayPool<byte>.Shared.Rent(value.Length * 3); // max expansion: each char -> 3 bytes
				int count = encoding.GetBytes(value, input);

				WriteVarIntImpl((ulong)count);
				WriteBytes(input, 0, count);

				ArrayPool<byte>.Shared.Return(input);
			}
			else
			{
				var input = encoding.GetBytes(value);
				int count = input.Length;

				WriteVarIntImpl((ulong)count);
				WriteBytes(input, 0, count);
			}
		}

		/// <summary>
		/// 写入字符串：指定固定2字节长度+utf8字节流
		/// </summary>
		public void WriteStringLen2(string value)
		{
			// TODO
		}

		/// <summary>
		/// 写入变长整数(ZigZag编码)，只支持小端序列
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteInt64V(long val) => WriteVarIntImpl(ZigZagHelper.Encode(val));

		/// <summary>
		/// 写入变长非负整数，只支持小端序列
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void WriteUInt64V(ulong val) => WriteVarIntImpl(val);

		private void WriteVarIntImpl(ulong val)
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

				WriteBytes(input.Slice(0, count));
			}
		}

		/// <summary>
		/// 写入字节流，不附加任何额外信息
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int WriteBytes(byte[] input, int offset, int count) => WriteBytes(input.AsSpan(offset, count));

		/// <summary>
		/// 写入字节流，不附加任何额外信息
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int WriteBytes(ReadOnlySpan<byte> input)
		{
			if (input.IsEmpty)
				return 0;

			int count = input.Length;
			int newPos = _position + count;

			if (!EnsureCapacity(newPos))
				throw new ArgumentOutOfRangeException();

			var output = _buffer.Slice(_position, count);
			input.CopyTo(output);

			_position = newPos;
			return count;
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
	}
}

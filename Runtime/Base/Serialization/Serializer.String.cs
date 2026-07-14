using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace ArkSharp
{
	public ref partial struct Serializer
	{
		/// <summary>
		/// 写入字符串：长度+utf8字节流
		/// 由SerializeOptions控制长度字节数
		/// </summary>
		public void Write(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				WriteLength(0);
				return;
			}

			var encoding = Encoding.UTF8;

			// 参考BinaryWriter代码实现
			// https://github.com/dotnet/runtime/blob/1d1bf92fcf43aa6981804dc53c5174445069c9e4/src/libraries/System.Private.CoreLib/src/System/IO/BinaryWriter.cs#L348C13-L382C10

			int maxBytes = value.Length * 3;
			if (maxBytes <= 127)
			{
				unsafe
				{
					// 不可直接用 Span<byte> input = stackalloc byte[];
					// 会导致WriteBytes(input)编译错误CS8350，或unsafe编译警告
					var ptr = stackalloc byte[127];
					var input = new Span<byte>(ptr, 127);

					int count = encoding.GetBytes(value, input);
					WriteLength(count);
					WriteRaw(input.Slice(0, count));
				}
			}
			else if (maxBytes <= MaxArrayPoolRentalSize)
			{
				var input = ArrayPool<byte>.Shared.Rent(maxBytes);
				try
				{
					int count = encoding.GetBytes(value, input);
					WriteLength(count);
					WriteRaw(input, 0, count);
				}
				finally
				{
					ArrayPool<byte>.Shared.Return(input);
				}
			}
			else
			{
				var input = encoding.GetBytes(value);
				int count = input.Length;
				WriteLength(count);
				WriteRaw(input, 0, count);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void WriteLength(int length)
		{
			if (length < 0)
				throw new ArgumentOutOfRangeException(nameof(length));

			if (options.HasFlag(SerializeOptions.FixedLen4))
			{
				// 4字节定长
				WriteRaw((uint)length);
			}
			else if (options.HasFlag(SerializeOptions.FixedLen2))
			{
				if (length >= ushort.MaxValue)
					throw new ArgumentOutOfRangeException(nameof(length));

				// 2字节定长
				WriteRaw((ushort)length);
			}
			else
			{
				// 变长
				WriteVarUIntImpl((uint)length);
			}
		}
	}
}

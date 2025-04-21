using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace ArkSharp
{
	public ref partial struct Deserializer
	{
		/// <summary>
		/// 读取字符串：长度(7bit变长)+utf8字节流
		/// 长度为0则返回string.Empty
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string ReadString()
		{
			var count = ReadLength();
			if (count <= 0)
				return string.Empty;

			var encoding = Encoding.UTF8;

			var input = _buffer.Slice(_position, count);
			var result = encoding.GetString(input);

			_position += count;
			return result;
		}

		/// <summary>
		/// 读取字符串：长度(7bit变长)+utf8字节流
		/// 长度为0则返回string.Empty
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Read(out string result) => result = ReadString();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal int ReadLength()
		{
			if (options.HasFlag(SerializeOptions.FixedLen4))
			{
				// 4字节定长
				var result = ReadRaw<uint>();
				if (result > int.MaxValue)
					throw new OverflowException();

				return (int)result;
			}
			else if (options.HasFlag(SerializeOptions.FixedLen2))
			{
				// 2字节定长
				return ReadRaw<ushort>();
			}
			else
			{
				// 变长
				var result = ReadVarUIntImpl();
				if (result > int.MaxValue)
					throw new OverflowException();

				return (int)result;
			}
		}
	}
}

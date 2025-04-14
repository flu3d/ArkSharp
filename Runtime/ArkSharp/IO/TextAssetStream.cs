#if UNITY_5_3_OR_NEWER

using System;
using System.IO;
using Unity.Collections;
using UnityEngine;

namespace ArkSharp
{
	/// <summary>
	/// TextAsset的Stream流支持
	/// 需要保证TextAsset生存期比Stream长
	/// </summary>
	public class TextAssetStream : Stream
	{
		private readonly NativeArray<byte> _buffer;
		private int _position;

		public TextAssetStream(TextAsset asset) => _buffer = asset.GetData<byte>();

		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => _buffer.Length;
		public override long Position { get => _position; set => throw new NotSupportedException(); }

		public override void Flush() => throw new InvalidOperationException();
		public override long Seek(long offset, SeekOrigin origin) => throw new InvalidOperationException();
		public override void SetLength(long value) => throw new InvalidOperationException();
		public override void Write(byte[] buffer, int offset, int count) => throw new InvalidOperationException();

		public override int ReadByte()
		{
			int newPos = _position + 1;
			if (newPos > _buffer.Length)
				return -1;

			int result = _buffer[_position];
			_position = newPos;

			return result;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
            base.Read(buffer.AsSpan());

			if (offset < 0)
				throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count));

			int remain = _buffer.Length - _position;
			if (count > remain)
				count = remain;
			if (count <= 0)
				return 0;

			NativeArray<byte>.Copy(_buffer, _position, buffer, offset, count);
			_position += count;

			return count;
		}
	}

	public static class TextAssetStreamUtils
	{
		public static Stream AsStream(this TextAsset asset) => new TextAssetStream(asset);
	}
}

#endif

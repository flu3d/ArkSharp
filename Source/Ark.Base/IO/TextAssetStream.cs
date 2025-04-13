#if UNITY_5_3_OR_NEWER

using System;
using System.IO;
using Unity.Collections;
using UnityEngine;

namespace Ark
{
	public class TextAssetStream : Stream
	{
		private readonly NativeArray<byte> _buffer;
		private int _position;

		public TextAssetStream(TextAsset asset) : this(asset.GetData<byte>()) { }
		public TextAssetStream(NativeArray<byte> buffer) => _buffer = buffer;

		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => _buffer.Length;
		public override long Position { get => _position; set => throw new NotSupportedException(); }

		public override void Flush() => throw new InvalidOperationException();
		public override long Seek(long offset, SeekOrigin origin) => throw new InvalidOperationException();
		public override void SetLength(long value) => throw new InvalidOperationException();
		public override void Write(byte[] buffer, int offset, int count) => throw new InvalidOperationException();

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count));

			int newPos = _position + count;
			if (newPos > _buffer.Length)
			{
				newPos = _buffer.Length;
				count = _buffer.Length - _position;
			}	

			if (count > 0)
			{
				NativeArray<byte>.Copy(_buffer, _position, buffer, offset, count);
				_position = newPos;
			}

			return count;
		}
	}

	public static class TextAssetStreamUtils
	{
		public static Stream AsStream(this TextAsset asset) => new TextAssetStream(asset);
		public static Stream AsStream(this NativeArray<byte> buffer) => new TextAssetStream(buffer);
	}
}

#endif

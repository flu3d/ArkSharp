using System;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
	public static class SpanCharHelper
	{
		/// <summary>
		/// 分割Span字符串，默认返回迭代器而不是数组
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SpanCharSplitEnumerable Split(this ReadOnlySpan<char> s, params char[] separators)
		{
			return Split(s, separators, int.MaxValue, StringSplitOptions.None);
		}

		/// <summary>
		/// 分割Span字符串，默认返回迭代器而不是数组
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SpanCharSplitEnumerable Split(this ReadOnlySpan<char> s, ReadOnlySpan<char> separators, StringSplitOptions options = StringSplitOptions.None)
		{
			return Split(s, separators, int.MaxValue, options);
		}

		/// <summary>
		/// 分割Span字符串，默认返回迭代器而不是数组。
		/// 达到count限制时，最后一项包含未分割的剩余文本。
		/// </summary>
		/// <param name="s">待分割文本。</param>
		/// <param name="separators">分隔符集合。</param>
		/// <param name="count">最多返回的项数。</param>
		/// <param name="options">分割选项。</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SpanCharSplitEnumerable Split(this ReadOnlySpan<char> s, ReadOnlySpan<char> separators, int count = int.MaxValue, StringSplitOptions options = StringSplitOptions.None)
		{
			return new SpanCharSplitEnumerable(new SpanCharSplitEnumerator(s, separators, count, options));
		}
	}

	public ref struct SpanCharSplitEnumerable
	{
		private readonly SpanCharSplitEnumerator enumerator;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal SpanCharSplitEnumerable(SpanCharSplitEnumerator enumerator) => this.enumerator = enumerator;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SpanCharSplitEnumerator GetEnumerator() => enumerator;
	}

	public ref struct SpanCharSplitEnumerator
	{
		private readonly ReadOnlySpan<char> _input;
		private readonly ReadOnlySpan<char> _separators;
		private readonly StringSplitOptions _options;
		private int _count;
		private int _position;

		internal SpanCharSplitEnumerator(ReadOnlySpan<char> input, ReadOnlySpan<char> separators, int count, StringSplitOptions options)
		{
			_input = input;
			_separators = separators;
			_count = count;
			_options = options;
			_position = 0;
			Current = default;
		}

		public ReadOnlySpan<char> Current { get; private set; }

		public bool MoveNext()
		{
			while (_count > 0 && _position <= _input.Length)
			{
				// 最后一个配额直接返回剩余文本，避免继续分割导致内容丢失。
				if (_count == 1)
				{
					if ((_options & StringSplitOptions.RemoveEmptyEntries) != 0)
					{
						while (_position < _input.Length && _separators.IndexOf(_input[_position]) >= 0)
							_position++;

						if (_position >= _input.Length)
							return false;
					}

					Current = _input[_position..];
					_position = _input.Length + 1;
					_count = 0;
					return true;
				}

				int nextPos = _input[_position..].IndexOfAny(_separators);
				if (nextPos == -1)
					nextPos = _input.Length - _position;

				var slice = _input.Slice(_position, nextPos);
				_position += nextPos + 1;

				// 处理空条目
				if (_options == StringSplitOptions.RemoveEmptyEntries && slice.IsEmpty)
					continue;

				Current = slice;
				_count--;
				return true;
			}

			return false;
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ArkSharp
{
	public static class EnumeratorHelper
	{
		/// 让IEnumerator直接支持foreach语法糖
		/// Make 'foreach' works directly with IEnumerator
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerator<T> GetEnumerator<T>(this IEnumerator<T> enumerator) => enumerator;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Enumerable<T> AsEnumerable<T>(this IEnumerator<T> enumerator) => new Enumerable<T>(enumerator);

		public struct Enumerable<T> : IEnumerable<T>
		{
			private readonly IEnumerator<T> enumerator;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal Enumerable(IEnumerator<T> enumerator) => this.enumerator = enumerator;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public IEnumerator<T> GetEnumerator() => enumerator;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			IEnumerator IEnumerable.GetEnumerator() => enumerator;
		}
	}
}

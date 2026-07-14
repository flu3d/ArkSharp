using System;
using System.Runtime.CompilerServices;

#if UNITY_5_3_OR_NEWER
using ProfilerMarker = Unity.Profiling.ProfilerMarker;
#endif

namespace ArkSharp
{
	public static class ProfilerHelper
	{
#if UNITY_5_3_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ProfilerMarker.AutoScope Begin(string name)
		{
			var marker = new ProfilerMarker(name);
			return marker.Auto();
		}
#else
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ProfilerMarkerAutoScope Begin(string name)
		{
			return new ProfilerMarkerAutoScope(name);
		}

		// TODO
		public struct ProfilerMarkerAutoScope : IDisposable
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal ProfilerMarkerAutoScope(string name) {}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Dispose() { }
		}
#endif
	}
}

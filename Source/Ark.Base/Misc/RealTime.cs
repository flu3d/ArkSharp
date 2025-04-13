using System;

namespace Ark
{
	/// <summary>
	/// UTC实时时间
	/// </summary>
	public static class RealTime
	{
		/// <summary>
		/// 当前UTC时间(秒)
		/// </summary>
		public static double now => (double)DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond;

		/// <summary>
		/// 当前UTC时间(毫秒)
		/// </summary>
		public static long ticks => DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;

		/// <summary>
		/// 从Epoch时间开始的秒数，即经典UNIX-TIME
		/// </summary>
		public static long unixTime => (long)(DateTime.UtcNow - epochTime).TotalSeconds;

		/// <summary>
		/// 从Epoch时间开始的毫秒数
		/// </summary>
		public static long unixTimeMS => (long)(DateTime.UtcNow - epochTime).TotalMilliseconds;

		/// <summary>
		/// 从Epoch时间开始的纳秒数
		/// </summary>
		public static long unixTimeNS => (DateTime.UtcNow - epochTime).Ticks * 100;

		/// <summary>
		/// Epoch新纪元时间  0:00:00, January 1, 0001, UTC
		/// </summary>
		public static readonly DateTime epochTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
	}
}

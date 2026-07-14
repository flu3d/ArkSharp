using System;

namespace ArkSharp
{
	public static partial class SerializeHelper
	{
		public static void WriteBlob(ref this Serializer s, byte[] blob)
		{
			int count = (blob != null) ? blob.Length : 0;
			s.WriteLength(count);

			s.WriteRaw(blob);
		}


		public static void ReadBlob(ref this Deserializer s, out byte[] blob)
		{
			blob = null;

			int count = s.ReadLength();
			if (count <= 0)
				return;

			blob = new byte[count];
			if (s.ReadRaw(blob) < count)
				throw new ArgumentOutOfRangeException($"SerializeHelper.ReadBlob() not enough ({count}) bytes");
		}
	}
}

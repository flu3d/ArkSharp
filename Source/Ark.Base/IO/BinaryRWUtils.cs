using System;
using System.IO;

namespace Ark
{
	/// <summary>
	/// 为BinaryReader/BinaryWriter扩展变长int读写支持
	/// </summary>
	public static class BinaryRWUtils
	{
		public static uint ReadUIntV(this BinaryReader reader)
		{
			return (uint)reader.ReadIntV();
		}

		// https://referencesource.microsoft.com/#mscorlib/system/io/binaryreader.cs,f30b8b6e8ca06e0f
		public static int ReadIntV(this BinaryReader reader)
		{
			// Read out an Int32 7 bits at a time.
			// The high bit of the byte when on means to continue reading more bytes.
			int count = 0;
			int shift = 0;
			byte b;
			do
			{
				// Check for a corrupted stream.  Read a max of 5 bytes.
				// In a future version, add a DataFormatException.
				if (shift == 5 * 7)  // 5 bytes max per Int32, shift += 7
					throw new FormatException("Format_Bad7BitInt32");

				// ReadByte handles end of stream cases for us.
				b = reader.ReadByte();
				count |= (b & 0x7F) << shift;
				shift += 7;
			}
			while ((b & 0x80) != 0);

			return count;
		}

		public static void WriteIntV(this BinaryWriter writer, int val)
		{
			writer.WriteUIntV((uint)val);
		}

		// https://referencesource.microsoft.com/#mscorlib/system/io/binarywriter.cs,2daa1d14ff1877bd
		public static void WriteUIntV(this BinaryWriter writer, uint val)
		{
			// Write out an int 7 bits at a time.
			// The high bit of the byte, when on, tells reader to continue reading more bytes.
			while (val >= 0x80)
			{
				writer.Write((byte)(val | 0x80));
				val >>= 7;
			}

			writer.Write((byte)val);
		}
	}
}

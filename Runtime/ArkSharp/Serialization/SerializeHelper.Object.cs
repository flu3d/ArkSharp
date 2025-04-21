namespace ArkSharp
{
	/// <summary>
	/// 可序列化对象接口
	/// </summary>
	public interface IDeserializable
	{
		void Deserialize(ref Deserializer s);
	}

	/// <summary>
	/// 可序列化对象接口
	/// </summary>
	public interface ISerializable
	{
		void Serialize(ref Serializer s);
	}

	public static partial class SerializeHelper
	{
		/// <summary>
		/// 写入可序列化对象。如果对象为null则写入首字符0，否则写入1再调用对象的序列化
		/// </summary>
		public static void WriteObject(this Serializer s, ISerializable obj)
		{
			if (obj == null)
			{
				s.Write(false);
				return;
			}

			s.Write(true);
			obj.Serialize(ref s);
		}

		/// <summary>
		/// 读取可序列化对象。读取首字符为0则返回null，然后再调用对象的反序列化
		/// </summary>
		public static void ReadObject<T>(this Deserializer s, out T result) where T : class, IDeserializable, new()
		{
			result = null;

			var hasValue = s.ReadRaw<bool>();
			if (!hasValue)
				return;

			result = new T();
			result.Deserialize(ref s);
		}
	}
}

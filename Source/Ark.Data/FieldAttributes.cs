using System;

namespace Ark.Data
{
	/// <summary>
	///	需要做二进制序列化的字段
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
	public class SerializeFieldAttribute : Attribute
	{
		public Type originType;

		public SerializeFieldAttribute() { }
		public SerializeFieldAttribute(Type originType) => this.originType = originType;
	}

	/// <summary>
	/// 需要本地化翻译的字段
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
	public class L10nAttribute : Attribute
	{
	}

	/// <summary>
	/// 基础信息字段
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
	public class DefineFieldAttribute : Attribute
	{
	}
}

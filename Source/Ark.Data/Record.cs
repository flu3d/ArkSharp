using System.Collections.Generic;

namespace Ark.Data
{
	/// <summary>
	/// 通用数据表记录基类
	/// </summary>
	public abstract class Record
	{
		public const char PrefixComment = '#';
		public const char PrefixL10n = '@';
		public const char PrefixDataSlot = '$';
	}

	public interface IKeyBase<K>
	{
		K GetKey();
	}

	/// <summary>
	/// 由1个字段作为键值的记录
	/// </summary>
	public interface IKey<K> : IKeyBase<K> {}

	/// <summary>
	/// 由2个字段联合构成键值
	/// </summary>
	public interface IKeyCombine<K, K1, K2> : IKeyBase<K>
	{
		K CombineKey(K1 key1, K2 key2);
	}

	/// <summary>
	/// 由3个字段联合构成键值
	/// </summary>
	public interface IKeyCombine<K, K1, K2, K3> : IKeyBase<K>
{
		K CombineKey(K1 key1, K2 key2, K3 key3);
	}

	/// <summary>
	/// 允许同一个键值有多条记录
	/// </summary>
	public interface IKeyRepeatable<K> : IKeyBase<K> { }

	/// <summary>
	/// 内嵌表记录
	/// </summary>
	public interface INest
	{
		string GetNestPath();

		IReadOnlyList<Table> GetNestTables();
	}

	/// <summary>
	/// Record需要在序列化后执行一些初始化行为
	/// </summary>
	public interface ISerializeCallback
	{
		void OnAfterDeserialize();
	}
}

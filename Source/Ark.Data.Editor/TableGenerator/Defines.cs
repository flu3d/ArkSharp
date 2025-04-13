using System.Collections.Generic;

namespace Ark.Data.Editor
{
	class FieldDefine
	{
		public string name;
		public string desc;
		public string type;
		public string export;

		public string nameOutput;
		public string typeOutput;
		public string typeOrigin;

		public bool IsList;

		public bool IsDefine;
		public bool IsL10n;
		public bool IsDataSlot;
		public bool IsComment;
	}

	class TableDefine
	{
		public readonly string tableName;
		public readonly List<FieldDefine> fields = new List<FieldDefine>(32);

		public FieldDefine defineField;

		public string keyExpression;
		public List<FieldDefine> keyFields;
		public string keyType;
		public bool keyRepeat;

		public bool isNest;

		public TableDefine(string tableName) => this.tableName = tableName;
	}
}

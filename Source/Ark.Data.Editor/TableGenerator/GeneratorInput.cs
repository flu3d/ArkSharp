using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;

namespace Ark.Data.Editor
{
	static partial class TableGenerator
	{
		private static readonly Encoding utf8bom = new UTF8Encoding(true);
		private static readonly Regex regexFieldName = new Regex(@"([_a-zA-Z][_a-zA-Z0-9]*)");

		private static void LoadTable(IDictionary<string, TableDefine> dict, string srcFile, bool isNest)
		{
			Log($"LoadTable {srcFile}");

			var tableName = Path.GetFileNameWithoutExtension(srcFile);

			dict.TryGetValue(tableName, out var table);
			if (table == null)
				dict[tableName] = table = new TableDefine(tableName);

			table.isNest |= isNest;

			using (var steam = new FileStream(srcFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var reader = new StreamReader(steam, utf8bom))
			using (var csv = new CsvReader(reader))
			{
				csv.Read();
				csv.ReadHeader();

				var columns = csv.Context.HeaderRecord;
				for (int i = 0; i < columns.Length; i++)
				{
					if (table.fields.Find(field => field.name == columns[i]) == null)
						table.fields.Add(new FieldDefine { name = columns[i] });
				}

				csv.Read();
				var descs = csv.Context.Record;
				for (int i = 0; i < descs.Length; i++)
				{
					if (i >= columns.Length)
						break;

					var field = table.fields[i];
					if (field.desc != null && field.desc != descs[i])
						Log($"   {field.name}.desc conflict: {field.desc} != {descs[i]}");

					field.desc = descs[i];
				}

				csv.Read();
				var types = csv.Context.Record;
				for (int i = 0; i < types.Length; i++)
				{
					if (i >= columns.Length)
						break;

					var field = table.fields[i];
					if (field.type != null && field.type != types[i])
						LogError($"   {field.name}.type conflict: {field.type} != {types[i]}");

					field.type = types[i];
				}

				csv.Read();
				var exports = csv.Context.Record;
				for (int i = 0; i < exports.Length; i++)
				{
					if (i >= columns.Length)
						break;

					var field = table.fields[i];
					if (field.export != null && field.export != exports[i])
						Log($"   {field.name}.export conflict: {field.export} != {exports[i]}");

					field.export = exports[i];
				}
			}
		}

		private static void Preprocess(TableDefine table, TableGeneratorConfig config)
		{
			Log($"Preprocess {table.tableName}");

			// 处理字段名和类型
			foreach (var field in table.fields)
			{
				if (field.name == Table.Config.CsvDefineField)
				{
					field.IsDefine = true;
					table.defineField = field;
				}

				switch (field.name[0])
				{
					case Record.PrefixComment:
						field.IsComment = true;
						field.nameOutput = field.name.Substring(1);
						break;
					case Record.PrefixL10n:
						field.IsL10n = true;
						field.nameOutput = field.name.Substring(1);
						break;
					case Record.PrefixDataSlot:
						field.IsDataSlot = true;
						field.nameOutput = field.name.Substring(1);
						break;
					default:
						field.nameOutput = field.name;
						break;
				}

				if (string.IsNullOrEmpty(field.type) || field.IsDefine)
				{
					field.typeOutput = "string";
				}
				else
				{
					if (field.type.EndsWith("[]"))
					{
						var elemType = field.type.Substring(0, field.type.Length - 2);
						field.typeOutput = $"List<{elemType}>";
						field.IsList = true;

						if (config.Enum2Int)
						{
							field.typeOrigin = GetTypeOrigin(elemType);
							if (field.typeOrigin != null)
								field.typeOutput = "List<int>";
						}
					}
					else if (field.type.StartsWith("list<", StringComparison.OrdinalIgnoreCase))
					{
						var elemType = field.type.Substring(5, field.type.Length - 6);
						field.typeOutput = $"List<{elemType}>";
						field.IsList = true;

						if (config.Enum2Int)
						{
							field.typeOrigin = GetTypeOrigin(elemType);
							if (field.typeOrigin != null)
								field.typeOutput = "List<int>";
						}
					}
					else
					{
						field.typeOutput = field.type;

						if (config.Enum2Int)
						{
							field.typeOrigin = GetTypeOrigin(field.typeOutput);
							if (field.typeOrigin != null)
								field.typeOutput = "int";
						}
					}

					field.typeOutput = field.typeOutput
						.Replace("int32", "int")
						.Replace("int64", "long")
						.Replace("uint32", "uint")
						.Replace("uint64", "ulong");

					if (config.Float2FP)
					{
						field.typeOutput = field.typeOutput
							.Replace("float", "FP")
							.Replace("vector2", "TSVector2")
							.Replace("vector3", "TSVector3");
					}
				}
			}

			// 处理键值
			if (table.defineField == null)
				throw new Exception($"Table <{table.tableName}> {Table.Config.CsvDefineField} field not found");

			if (table.defineField.type.StartsWith("key="))
			{
				table.keyExpression = table.defineField.type.Substring(4);
			}
			else if (table.defineField.type.StartsWith("mkey="))
			{
				table.keyExpression = table.defineField.type.Substring(5);
				table.keyRepeat = true;
			}
			else
				return;

			var matches = regexFieldName.Matches(table.keyExpression);
			if (matches != null)
			{
				table.keyFields = new List<FieldDefine>(matches.Count);

				for (int i = 0; i < matches.Count; i++)
				{
					var field = table.fields.Find(field => field.nameOutput == matches[i].Value);
					if (field != null && table.keyFields.IndexOf(field) < 0)
						table.keyFields.Add(field);
				}

				var keyField = table.keyFields[0];
				if (keyField != null)
				{
					table.keyType = keyField.typeOutput;

					// 多字段联合键值强制为int类型					
					if (table.keyFields.Count > 1)
						table.keyType = "int";
				}
			}
		}

		private static string GetTypeOrigin(string type)
		{
			switch (type)
			{
				case "bool":
				case "int":
				case "int32":
				case "int64":
				case "uint":
				case "uint32":
				case "uint64":
				case "long":
				case "ulong":
				case "float":
				case "string":
				case "fp":
				case "vector2":
				case "vector3":
					return null;
				default:
					return type;
			}
		}

		private static string ConvertKeyType(string expression, string type)
		{
			return regexFieldName.Replace(expression, $"({type})$1");
		}
	}
}

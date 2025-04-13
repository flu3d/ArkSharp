using System;
using System.Collections.Generic;
using System.IO;

namespace Ark.Data.Editor
{
	static partial class TableGenerator
	{
		private static void WriteRecords(IReadOnlyDictionary<string, TableDefine> dict, StreamWriter output, TableGeneratorConfig config)
		{
			var ns = config.OutputNamespace;

			if (ns != null)
				output.WriteLine($"namespace {ns} {{\n");

			foreach (var kv in dict)
			{
				var table = kv.Value;

				output.WriteLine("[Serializable]");
				output.Write($"public sealed partial class {table.tableName} : Record");

				if (table.keyType != null)
				{
					if (table.keyFields.Count > 1)
					{
						output.Write($", IKeyCombine<{table.keyType}");

						for (int i = 0; i < table.keyFields.Count; i++)
							output.Write($",{table.keyFields[i].typeOutput}");

						output.Write(">");
					}
					else
					{
						output.Write($", IKey<{table.keyType}>");
					}

					if (table.keyRepeat)
						output.Write($", IKeyRepeatable<{table.keyType}>");
				}

				output.WriteLine("\n{");

				if (table.keyType != null)
				{
					if (table.keyFields.Count == 1)
					{
						output.WriteLine($"\t[MethodImpl(MethodImplOptions.AggressiveInlining)] public {table.keyType} GetKey() => {table.keyExpression};");
					}
					else
					{
						output.Write($"\t[MethodImpl(MethodImplOptions.AggressiveInlining)] public {table.keyType} GetKey() => CombineKey(");
						for (int i = 0; i < table.keyFields.Count; i++)
						{
							if (i > 0)
								output.Write(", ");

							output.Write(table.keyFields[i].nameOutput);
						}
						output.WriteLine(");");

						output.Write($"\t[MethodImpl(MethodImplOptions.AggressiveInlining)] public {table.keyType} CombineKey(");
						for (int i = 0; i < table.keyFields.Count; i++)
						{
							if (i > 0)
								output.Write(", ");
							output.Write($"{table.keyFields[i].typeOutput} {table.keyFields[i].nameOutput}");
						}

						var keyExpressionConverted = ConvertKeyType(table.keyExpression, table.keyType);
						output.WriteLine($") => {keyExpressionConverted};");
					}

					output.WriteLine();
				}

				int slotCount = 0;
				string slotType = null;
				var slotFields = new List<FieldDefine>();

				for (int i = 0; i < table.fields.Count; i++)
				{
					var field = table.fields[i];
					if (field.IsDefine)
						continue;
					if (field.IsComment)
						continue;

					if (!string.IsNullOrEmpty(field.desc))
					{
						var desc = field.desc.Replace("\r", "").Replace("\n", " ");
						output.WriteLine($"\t///<summary>{desc}</summary>");
					}

					if (field.IsComment)
					{
						output.Write("\t");
					}
					else
					{
						if (field.IsDataSlot)
						{
							if (slotCount == 0 && slotType == null)
							{
								switch (field.typeOutput)
								{
									case "int":
									case "long":
									case "float":
									case "double":
										break;
									default:
										LogError($"{field.name} {field.type} is NOT support for $DataSlot");
										continue;
								}

								slotType = field.typeOutput;
							}
							else
							{
								if (slotType != field.typeOutput)
								{
									LogError($"{field.name} {field.type} conflict with {slotType} for $DataSlot");
									continue;
								}
							}

							slotFields.Add(field);
							slotCount++;
						}

						/*
						if (config.EnableOdinInspector)
							output.WriteLine("\t[ShowInInspector]");

						output.WriteLine($"\tpublic {field.typeOutput} {field.nameOutput} {{ \n\t\t[MethodImpl(MethodImplOptions.AggressiveInlining)] get => DataSlots[{slotCount}]; \n\t\t[MethodImpl(MethodImplOptions.AggressiveInlining)] set => DataSlots[{slotCount}] = value; \n\t}}");
						*/

						if (field.typeOrigin != null)
							output.Write($"\t[SerializeField(typeof({field.typeOrigin}))] ");
						else
							output.Write("\t[SerializeField] ");

						if (field.IsL10n)
							output.Write("[L10n] ");

						if (field.IsList)
							output.WriteLine($"public IReadOnly{field.typeOutput} {field.nameOutput} = new {field.typeOutput}();");
						else
							output.WriteLine($"public {field.typeOutput} {field.nameOutput};");
					}

					if (i + 1 < table.fields.Count)
						output.WriteLine();
				}

				if (slotCount > 0)
				{
					//output.WriteLine($"\t[SerializeField] public {slotType}[] DataSlots = new {slotType}[{slotCount}];");
					output.WriteLine($"\tpublic const int FieldCount = {slotCount};");

					if (slotCount > 0 && !string.IsNullOrEmpty(config.DataSlotTypePostfix))
					{
						output.WriteLine($"\n\tpublic enum {config.DataSlotTypePostfix}\n\t{{");

						for (int i = 0; i < slotFields.Count; i++)
						{
							var field = slotFields[i];
							var desc = field.desc.Replace("\r", "").Replace("\n", " ");
							output.WriteLine($"\t\t{field.nameOutput} = {i},\t\t\t// {desc}");
						}

						output.WriteLine("\t\tMax");
						output.WriteLine("\t}");
					}
				}

				output.WriteLine("}");
				output.WriteLine();
			}

			if (ns != null)
				output.WriteLine($"}} // {ns}\n");
		}

		private static void WriteTables(IReadOnlyDictionary<string, TableDefine> dict, StreamWriter output, TableGeneratorConfig config)
		{
			var ns = config.TableNamespace ?? config.OutputNamespace;
			var exportList = config.GetExportList();

			if (!string.IsNullOrEmpty(ns))
				output.WriteLine($"namespace {ns} {{\n");

			foreach (var kv in dict)
			{
				var table = kv.Value;

				output.Write($"public sealed partial class {table.tableName}Table : ");

				if (table.keyType != null)
				{
					if (table.keyRepeat)
						output.Write($"TableDictRepeatable<{table.tableName},{table.keyType}");
					else
						output.Write($"TableDict<{table.tableName},{table.keyType}");

					if (table.keyFields.Count > 1)
					{
						for (int i = 0; i < table.keyFields.Count; i++)
							output.Write($",{table.keyFields[i].typeOutput}");
					}

					output.Write(">");
				}
				else
				{
					output.Write($"Table<{table.tableName}>");
				}

				if (config.TableInstanceMethod && !table.isNest)
					output.WriteLine($" {{ public static {table.tableName}Table Instance() => Tables.{table.tableName}; }} ");
				else
					output.WriteLine(" {}");
			}

			// 写入静态表列表
			output.WriteLine();
			output.WriteLine("public static partial class Tables\n{");

			foreach (var kv in dict)
			{
				var table = kv.Value;
				if (table.isNest)
					continue;

				output.WriteLine($"\tpublic static readonly {table.tableName}Table {table.tableName} = new {table.tableName}Table();");
			}

			output.WriteLine();

			output.Write("\tpublic static readonly Table[] AllTables = new Table[] { ");
			foreach (var kv in dict)
			{
				var table = kv.Value;
				if (table.isNest)
					continue;

				if (CanExport(table, exportList))
					output.Write($"{table.tableName},");
			}
			output.WriteLine(" };");

			foreach (var export in exportList)
			{
				output.Write($"\tpublic static readonly Table[] {export}Tables = new Table[] {{ ");
				foreach (var kv in dict)
				{
					var table = kv.Value;
					if (table.isNest)
						continue;

					if (CanExport(table, export))
						output.Write($"{table.tableName},");
				}
				output.WriteLine(" };");
			}

			output.WriteLine("}");

			if (!string.IsNullOrEmpty(ns))
				output.WriteLine($"}} // {ns}\n");
		}

		private static bool CanExport(TableDefine table, string export)
		{
			return table.defineField.export != null
				&& table.defineField.export.Contains(export, StringComparison.OrdinalIgnoreCase);
		}

		private static bool CanExport(TableDefine table, IReadOnlyList<string> exportList)
		{
			if (table.defineField.export == null)
				return false;

			foreach (var export in exportList)
			{
				if (CanExport(table, export))
					return true;
			}

			return false;
		}
	}
}

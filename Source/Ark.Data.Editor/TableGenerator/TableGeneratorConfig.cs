using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector;
using FilePathAttribute = Sirenix.OdinInspector.FilePathAttribute;
#endif

namespace Ark.Data.Editor
{
#if UNITY_EDITOR
	[HideMonoScript]
	public class TableGeneratorConfig : SerializedScriptableObject
#else
	public class TableGeneratorConfig
#endif
	{
#if UNITY_EDITOR
		[FolderPath]
#endif
		public string CsvPath;

#if UNITY_EDITOR
		[FilePath(Extensions = ".cs")]
#endif
		public string OutputPath;
		public string OutputNamespace;

		public enum ExportCategory
		{
			Client = 0,
			Server,
			All,
		}
		public ExportCategory Exports;

#if UNITY_EDITOR
		[PropertySpace]
		[LabelText("Convert float to FP")]
#endif
		public bool Float2FP;

#if UNITY_EDITOR
		[LabelText("Convert Enum to int")]
#endif
		public bool Enum2Int;

#if UNITY_EDITOR
		[LabelText("Enable OdinInspector")]
#endif
		public bool EnableOdinInspector;

#if UNITY_EDITOR
		[LabelText("DataSlot type postfix")]
#endif
		public string DataSlotTypePostfix;

#if UNITY_EDITOR
		[LabelText("Generate Table")]
#endif
		public bool WriteTables;

#if UNITY_EDITOR
		[EnableIf("WriteTables")]
		[Indent]
#endif
		public string TableNamespace;

#if UNITY_EDITOR
		[EnableIf("WriteTables")]
		[Indent]
#endif
		public bool TableInstanceMethod;

#if UNITY_EDITOR
		[PropertySpace]
		[ListDrawerSettings(AlwaysAddDefaultValue = true)]
		[HideReferenceObjectPicker]
#endif
		public List<string> UsingNamespace = new();

		public IReadOnlyList<string> GetExportList()
		{
			var list = new List<string>();

			switch (Exports)
			{
				case ExportCategory.All:
					for (int i = 0; i < (int)ExportCategory.All; i++)
					{
						var d = (ExportCategory)i;
						if (Enum.IsDefined(typeof(ExportCategory), d))
							list.Add(d.ToString());
					}
					break;
				default:
					list.Add(Exports.ToString());
					break;
			}

			return list;
		}

#if UNITY_EDITOR
		[Button(ButtonSizes.Large)]
		[GUIColor(0, 1, 0)]
		void Generate()
		{
			AssetDatabase.SaveAssetIfDirty(this);
			TableGenerator.Run(this);
		}

		[MenuItem("Ark/TableGenerator")]
		static void Open()
		{
			var guid = AssetDatabase.FindAssets($"t:TableGeneratorConfig").FirstOrDefault();
			var path = !string.IsNullOrEmpty(guid)
				? AssetDatabase.GUIDToAssetPath(guid)
				: DefaultPath;

			var config = AssetDatabase.LoadAssetAtPath<TableGeneratorConfig>(path);
			if (config == null)
			{
				config = CreateInstance<TableGeneratorConfig>();
			
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				AssetDatabase.CreateAsset(config, path);
				AssetDatabase.Refresh();
			}

			Selection.activeObject = config;
			EditorGUIUtility.PingObject(config);
		}

		public const string DefaultPath = "Assets/Editor Default Resources/TableGenerator.asset";
#endif
	}
}

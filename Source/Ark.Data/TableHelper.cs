using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
using UnityEngine.AddressableAssets;
#endif

namespace Ark.Data
{
	public static class TableHelper
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static WaitForAll LoadCsvAsync(IReadOnlyList<Table> tables, string rootPath, string csvExt = ".csv")
		{
			rootPath = PathHelper.Normalize(rootPath);
			if (!rootPath.EndsWith('/'))
				rootPath += '/';

			var opList = new AsyncOp[tables.Count];

			for (int i = 0; i < tables.Count; i++)
			{
				var table = tables[i];
				opList[i] = LoadCsvAsync(table, $"{rootPath}{table.GetName()}{csvExt}");
			}

			return new WaitForAll(opList);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static AsyncOp LoadCsvAsync(Table table, string path)
		{
			return CoroutineScheduler.Default.Start(LoadCsvAsyncImpl(table, path));
		}

		private static IEnumerator LoadCsvAsyncImpl(Table table, string path)
		{
			Stream stream = null;

#if UNITY_5_3_OR_NEWER
			var assetLoader = Addressables.LoadAssetAsync<TextAsset>(path);
			yield return assetLoader;

			var asset = assetLoader.Result;
			if (asset == null)
				yield break;

			stream = asset.AsStream();
#else
			try
			{
				stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			}
			catch (Exception e)
			{
				Log.Error($"{table.GetName()}.LoadCsvAsync() {e.Message}\n{e.StackTrace}");
				yield break;
			}
#endif

			var task = Task.Run(() => table.LoadCsv(stream));
			yield return task;

			if (task.IsFaulted)
			{
				foreach (var e in task.Exception.InnerExceptions)
					Log.Error($"{table.GetName()}.LoadCsv() {e.Message}\n{e.StackTrace}");
			}

			task = null;

			stream.Close();
			stream = null;

#if UNITY_5_3_OR_NEWER
			Addressables.Release(asset);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Unload(IReadOnlyList<Table> tables)
		{
			for (int i = 0; i < tables.Count; i++)
				tables[i]?.Unload();
		}

		/// <summary>
		/// 根据数据列表自动填充对象数据字段
		/// </summary>
		public static void WriteRecordsToFields<Target, Record>(Target target, IReadOnlyList<Record> records, 
			Func<Record, string> keyFunc, Func<Record, string> valFunc, bool ignoreCase) where Target : class
		{
			if (records == null || keyFunc == null || valFunc == null)
				return;

			// 如果target不为空则取目标实际类型，否则才取Target类型(可能是实际类型的父类)
			var targetType = (target != null) ? target.GetType() : typeof(Target);
			var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

			for (int i = 0; i < records.Count; i++)
			{
				var d = records[i];
				if (d == null)
					continue;

				var key = keyFunc(d).Trim();
				var val = valFunc(d);

				bool found = false;
				for (int j = 0; j < fields.Length; j++)
				{
					var field = fields[j];
					if (string.Compare(field.Name, key, ignoreCase) == 0)
					{
						found = true;
						try
						{
							var result = val.To(field.FieldType);
							if (field.IsStatic)
								field.SetValue(null, result);
							else
								field.SetValue(target, result);
						}
						catch (Exception e)
						{
#if UNITY_EDITOR
							Log.Warn($"Write field {targetType.Name}.{field.Name}=\"{val}\" error. {e}");
#endif
						}
						break;
					}
				}

#if UNITY_EDITOR
				if (!found)
					Log.Warn($"Write field {targetType.Name}.{key}=\"{val}\" not found");
#endif           
			}
		}
	}
}

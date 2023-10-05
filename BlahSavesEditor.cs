#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using OdinSerializer.Editor;
using UnityEditor;
#endif

namespace BlahSaves
{
public class BlahSavesEditor
{
#if UNITY_EDITOR
	public static void EditorGenerateAOT(params object[] models)
	{
		if (models.Length == 0)
			return;

		var types = new List<Type>();

		foreach (object model in models)
			types.AddRange(EditorGetFieldsTypes(model));

		var result = types.Distinct().ToList();

		Debug.Log("AOT dll types: " + string.Join("; ", result.Select(x => x.Name)));

		AOTSupportUtilities.GenerateDLL(Application.dataPath + "/Plugins/Odin", "AOT", result);
	}

	private static List<Type> EditorGetFieldsTypes(object baseType)
	{
		var types = new List<Type>();
		var type  = baseType.GetType();

		types.Add(type);
		types.AddRange(EditorGetFieldsTypes(type));

		return types;
	}

	private static List<Type> EditorGetFieldsTypes(Type baseType)
	{
		var types = new List<Type>();

		foreach (var field in baseType.GetFields())
		{
			var fieldType = field.FieldType;
			types.Add(fieldType);

			if (fieldType.IsGenericType)
			{
				foreach (var genericArgumentType in fieldType.GetGenericArguments())
				{
					types.Add(genericArgumentType);
					types.AddRange(EditorGetFieldsTypes(genericArgumentType));
				}
			}
			
			if (fieldType.IsClass && fieldType != typeof(string) && fieldType != typeof(object))
			{
				var innerType = fieldType.IsArray ? fieldType.GetElementType() : fieldType;

				var innerTypes = EditorGetFieldsTypes(innerType);
				types.AddRange(innerTypes);
			}
		}

		return types;
	}

	[MenuItem("Blah/Saves/Delete all")]
	public static void EditorDeleteAllSaves()
	{
		var path = $"{BlahSavesHelper.GetPath()}/{BlahSaveLoad.SAVES_FOLDER_NAME}";
		
		Directory.Delete(path, true);
		PlayerPrefs.DeleteKey(BlahSaveLoad.PP_SAVE_FILE_CREATED);
		
		Debug.Log("[BlahSaves] saves deleted");
	}

	[MenuItem("Blah/Saves/Delete main")]
	public static void EditorDeleteMainSave()
	{
		var path = $"{BlahSavesHelper.GetPath()}/{BlahSaveLoad.SAVES_FOLDER_NAME}";

		foreach (string filePath in Directory.EnumerateFiles(
			         path,
			         "*",
			         SearchOption.AllDirectories
		         ))
		{
			if (!filePath.Split('/', '\\')[^1].StartsWith("b_"))
			{
				File.Delete(filePath);
			}	
		}
		Debug.Log("[BlahSaves] main save deleted");
	}

	[MenuItem("Blah/Saves/Delete backup")]
	public static void EditorDeleteBackupSave()
	{
		var path = $"{BlahSavesHelper.GetPath()}/{BlahSaveLoad.SAVES_FOLDER_NAME}";

		foreach (string filePath in Directory.EnumerateFiles(
			         path,
			         "*",
			         SearchOption.AllDirectories
		         ))
		{
			if (filePath.Split('/', '\\')[^1].StartsWith("b_"))
			{
				File.Delete(filePath);
			}
		}
		Debug.Log("[BlahSaves] backup save deleted");
	}
#endif
}
}
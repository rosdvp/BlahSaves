using System;
using System.IO;
using OdinSerializer;
using UnityEngine;
using SerializationUtility = OdinSerializer.SerializationUtility;

namespace BlahSaves
{
public class BlahSaveLoad
{
	public const string PP_SAVE_FILE_CREATED = "SaveFileCreated";
	public const string SAVES_FOLDER_NAME    = "saves";
	
	private readonly string _baseDirPath;

	public BlahSaveLoad()
	{
		_baseDirPath = $"{BlahSavesHelper.GetPath()}/{SAVES_FOLDER_NAME}";
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	private string _currMainFilePath;
	private string _currBackupFilePath;

	public ELoadResult TryLoad<T>(ref T model, ref string log) where T : class
	{
		if (IsMainExist())
		{
			if (IsBackupExist() && IsBackupNewer())
			{
				log += "backup newer: ";
				if (TryLoadFile(_currBackupFilePath, out model, ref log))
					return model != null ? ELoadResult.BackupLoaded : ELoadResult.BackupLoadedNull;
			}
			log += "main: ";
			if (TryLoadFile(_currMainFilePath, out model, ref log))
				return model != null ? ELoadResult.MainLoaded : ELoadResult.MainLoadedNull;
		}
		else
			log += "main: not found; ";

		log += "backup: ";
		if (IsBackupExist())
		{
			if (TryLoadFile(_currBackupFilePath, out model, ref log))
				return model != null ? ELoadResult.BackupLoaded : ELoadResult.BackupLoadedNull;
		}
		else
			log += "not found;";

		return ShouldAnySaveExist ? ELoadResult.SaveLost : ELoadResult.NoSave;
	}

	
	private bool TryLoadFile<T>(string filePath, out T model, ref string log)
	{
		try
		{
			byte[] bytes = File.ReadAllBytes(filePath);
			model =  SerializationUtility.DeserializeValue<T>(bytes, DataFormat.Binary);
			log   += "success;";
			return true;
		}
		catch (Exception e)
		{
			model =  default;
			log   += $"fail: {e.Message};";
			return false;
		}
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public bool TrySaveMain<T>(T model, ref string log)
	{
		if (TrySaveFile(_currMainFilePath, model, ref log))
		{
			PlayerPrefs.SetInt(PP_SAVE_FILE_CREATED, 1);
			return true;
		}
		return false;
	}

	public bool TrySaveBackup<T>(T model, ref string log)
	{
		if (TrySaveFile(_currBackupFilePath, model, ref log))
		{
			PlayerPrefs.SetInt(PP_SAVE_FILE_CREATED, 1);
			return true;
		}
		return false;
	}
	
	private bool TrySaveFile<T>(string filePath, T model, ref string log)
	{
		try
		{
			byte[] bytes = SerializationUtility.SerializeValue(model, DataFormat.Binary);
			File.WriteAllBytes(filePath, bytes);
			log += "success;";
			return true;
		}
		catch (Exception e)
		{
			log += $"fail: {e.Message};";
			return false;
		}
	}

    //-----------------------------------------------------------
	//-----------------------------------------------------------
	private bool ShouldAnySaveExist => PlayerPrefs.GetInt(PP_SAVE_FILE_CREATED) == 1;
	
	public bool IsMainExist() => File.Exists(_currMainFilePath);

	public bool IsBackupExist() => File.Exists(_currBackupFilePath);

	private bool IsBackupNewer()
	{
		var mainDate   = File.GetLastWriteTime(_currMainFilePath);
		var backupDate = File.GetLastWriteTime(_currBackupFilePath);

		int result = DateTime.Compare(backupDate, mainDate);
		return result > 0;
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	/// <summary>In base folder by default.</summary>>
	/// <param name="folderName">If null, go to base folder.</param>
	public void SetTarget(string folderName, string fileName)
	{
		string subDirPath = folderName == null ? _baseDirPath : $"{_baseDirPath}/{folderName}";

		if (!Directory.Exists(subDirPath))
			Directory.CreateDirectory(subDirPath);

		_currMainFilePath   = $"{subDirPath}/{fileName}";
		_currBackupFilePath = $"{subDirPath}/b_{fileName}";
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------

	public enum ELoadResult
	{
		MainLoaded,
		MainLoadedNull,
		BackupLoaded,
		BackupLoadedNull,
		SaveLost,
		NoSave,
	}
}
}
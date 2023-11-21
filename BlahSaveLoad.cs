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
	
	private readonly string _baseDirPath = BlahSavesHelper.GetPath();

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	private string _currMainFilePath;
	private string _currBackupFilePath;

	public ELoadResult TryLoad<T>(ref T model, out string log) where T : class
	{
		bool isMainExist   = IsMainExist();
		bool isBackupExist = IsBackupExist();
		bool isBackupNewer = isMainExist && isBackupExist && IsBackupNewer();

		log = "";
		
		if (isBackupNewer)
		{
			log += "backup newer; ";
			if (TryLoadFile(_currBackupFilePath, out model, ref log))
				return ELoadResult.BackupNewerLoaded;
			log += "main; ";
			if (TryLoadFile(_currMainFilePath, out model, ref log))
				return ELoadResult.BackupNewerFailedMainLoaded;
		}
		else if (isMainExist)
		{
			log += "main; ";
			if (TryLoadFile(_currMainFilePath, out model, ref log))
				return ELoadResult.MainLoaded;
			if (isBackupExist)
			{
				log += "backup; ";
				if (TryLoadFile(_currBackupFilePath, out model, ref log))
					return ELoadResult.MainFailedBackupLoaded;
			}
			else
			{
				log += "no backup; ";
			}
		}
		else if (isBackupExist)
		{
			log += "no main; ";
			
			log += "backup; ";
			if (TryLoadFile(_currBackupFilePath, out model, ref log))
				return ELoadResult.MainFailedBackupLoaded;
		}
		else
		{
			log += "no main; no backup; ";
		}

		if (ShouldAnySaveExist)
		{
			log += "saves lost";
			return ELoadResult.MainFailedBackupFailed;
		}
		return ELoadResult.NoSaves;
	}

	
	private bool TryLoadFile<T>(string filePath, out T model, ref string log)
	{
		try
		{
			byte[] bytes = File.ReadAllBytes(filePath);
			model =  SerializationUtility.DeserializeValue<T>(bytes, DataFormat.Binary);
			if (model == null)
			{
				log += "null; ";
				return false;
			}
			log += "success; ";
			return true;
		}
		catch (Exception e)
		{
			model =  default;
			log   += $"fail: {e.Message}; ";
			return false;
		}
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public bool TrySaveMain<T>(T model, out string log)
	{
		if (TrySaveFile(_currMainFilePath, model, out log))
		{
			PlayerPrefs.SetInt(PP_SAVE_FILE_CREATED, 1);
			return true;
		}
		return false;
	}

	public bool TrySaveBackup<T>(T model, out string log)
	{
		if (TrySaveFile(_currBackupFilePath, model, out log))
		{
			PlayerPrefs.SetInt(PP_SAVE_FILE_CREATED, 1);
			return true;
		}
		return false;
	}
	
	private bool TrySaveFile<T>(string filePath, T model, out string log)
	{
		try
		{
			byte[] bytes = SerializationUtility.SerializeValue(model, DataFormat.Binary);
			File.WriteAllBytes(filePath, bytes);
			log = "success;";
			return true;
		}
		catch (Exception e)
		{
			log = $"fail: {e.Message};";
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
	/// <param name="subFolderName">If null, go to base folder.</param>
	public void SetTarget(string folderName, string subFolderName, string fileName)
	{
		folderName ??= SAVES_FOLDER_NAME;
		
		var path = $"{_baseDirPath}/{folderName}";
		if (!Directory.Exists(path))
			Directory.CreateDirectory(path);
		if (subFolderName != null)
		{
			path = $"{path}/{subFolderName}";
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
		}

		_currMainFilePath   = $"{path}/{fileName}";
		_currBackupFilePath = $"{path}/b_{fileName}";
	}

	//-----------------------------------------------------------
	//-----------------------------------------------------------

}
}
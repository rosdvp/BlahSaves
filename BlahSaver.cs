using System;

namespace BlahSaves
{
public class BlahSaver
{
	private const string SAVE_FILE_NAME = "save.bdt";
	
	private readonly BlahSaveLoad _saveLoad = new();
	private readonly BlahPatcher  _patcher = new();

	private readonly string _currVersion;

	public BlahSaver(string currVersion)
	{
		_currVersion = currVersion;
	}
	//-----------------------------------------------------------
	//-----------------------------------------------------------
	public event Action<string> EvLogInfo;
	public event Action<string> EvLogError;
	
	/// <summary>
	/// If loaded model version is <paramref name="fromVersion"/>,
	/// the <paramref name="action"/> will be invoked.
	/// The version of model will become <paramref name="toVersion"/> and the pipeline will go further.
	/// </summary>
	/// <param name="action">Cast object to model type and perform required modifications.</param>
	public void AddPatch(string fromVersion, string toVersion, Action<object> action)
		=> _patcher.AddPatch(fromVersion, toVersion, action);

	/// <summary>
	/// Tries to load the model from main save or backup.<br/>
	/// On success, applies patches provided via <see cref="AddPatch"/>.<br/>
	/// </summary>
	/// <param name="model">
	/// On success, valid model.<br/>
	/// On fail, null.
	/// </param>
	/// <param name="loadLog">
	/// Supplementary logs for result.
	/// Duplicates <see cref="EvLogInfo"/> and <see cref="EvLogError"/>.
	/// </param>
	public ELoadResult Load<T>(out T model, out string loadLog) 
		where T: class, IBlahSaveModelVersion
	{
		_saveLoad.SetTarget(null, null, SAVE_FILE_NAME);

		model = null;
		
		var result = _saveLoad.TryLoad(ref model, out loadLog);
		switch (result)
		{
			case ELoadResult.NoSaves:
			case ELoadResult.MainLoaded:
				EvLogInfo?.Invoke($"load; {result}; {loadLog}");
				break;
			case ELoadResult.BackupNewerLoaded:
			case ELoadResult.BackupNewerFailedMainLoaded:
			case ELoadResult.MainFailedBackupLoaded:
			case ELoadResult.MainFailedBackupFailed:
				EvLogError?.Invoke($"load; {result}; {loadLog}");
				break;
			default:
				throw new Exception($"{result} is not supported");
		}

		if (model != null)
		{
			_patcher.ApplyPatches(model, out string patchLog);
			EvLogInfo?.Invoke($"patch; {patchLog}");
		}
		
		return result;
	}

	/// <summary>
	/// Tries to load custom model without patching.
	/// </summary>
	/// <param name="folderName">Folder with saves. Use null for default.</param>
	/// <param name="subFolderName">Folder in folder with saves. Use null if there is no such folder.</param>
	/// <remarks>May return null.</remarks>
	public T TryLoadRaw<T>(string folderName, string subFolderName, string fileName) where T : class
	{
		_saveLoad.SetTarget(folderName, subFolderName, fileName);

		T model = null;
		var result = _saveLoad.TryLoad(ref model, out string loadLog);
		EvLogInfo?.Invoke($"load raw; {result}; {loadLog}");

		return model;
	}

	/// <summary>
	/// Saves model to main save file.<br/>
	/// Model's version will be set automatically.
	/// </summary>
	public void SaveMain<T>(T model) where T : IBlahSaveModelVersion
	{
		_saveLoad.SetTarget(null, null, SAVE_FILE_NAME);

		model.Version = _currVersion;

		bool isSuccess = _saveLoad.TrySaveMain(model, out string log);
		if (isSuccess)
			EvLogInfo?.Invoke($"save main; {log}");
		else
			EvLogError?.Invoke($"save main; {log}");
	}

	/// <summary>
	/// Saves model to backup save file.<br/>
	/// Model's version will be set automatically.
	/// </summary>
	public void SaveBackup<T>(T model) where T : IBlahSaveModelVersion
	{
		_saveLoad.SetTarget(null, null, SAVE_FILE_NAME);

		model.Version = _currVersion;

		bool isSuccess = _saveLoad.TrySaveBackup(model, out string log);
		if (isSuccess)
			EvLogInfo?.Invoke($"save backup; {log}");
		else
			EvLogError?.Invoke($"save backup; {log}");
	}

	/// <summary>
	/// Checks whether custom model exist.
	/// </summary>
	/// <param name="folderName">Folder with saves. Use null for default.</param>
	/// <param name="subFolderName">Folder in folder with saves. Use null if there is no such folder.</param>
	/// <remarks><b>Do not</b> use in common pipeline, only for complex patching.</remarks>
	public bool IsSaveExist(string folderName, string subFolderName, string fileName)
	{
		_saveLoad.SetTarget(folderName, subFolderName, fileName);
		return _saveLoad.IsMainExist() || _saveLoad.IsBackupExist();
	}
	
	/// <returns>Available space in megabytes on hard drive.</returns>
	public float GetAvailableSpaceMbOnDisk() => BlahSavesHelper.GetSpaceMb();
}
}
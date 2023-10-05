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
	public event Action<string> EvLogWarning;
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
	/// On fail, creates new model.
	/// </summary>
	public T LoadOrCreate<T>() where T: class, IBlahSaveModelVersion, new()
	{
		_saveLoad.SetTarget(null, SAVE_FILE_NAME);
		
		var log = "load; ";

		T   model  = null;
		var result = _saveLoad.TryLoad(ref model, ref log);

		switch (result)
		{
			case BlahSaveLoad.ELoadResult.MainLoaded:
				EvLogInfo?.Invoke(log);
				break;
			case BlahSaveLoad.ELoadResult.MainLoadedNull:
				EvLogError?.Invoke(log);
				break;
			case BlahSaveLoad.ELoadResult.BackupLoaded:
				EvLogWarning?.Invoke(log);
				break;
			case BlahSaveLoad.ELoadResult.BackupLoadedNull:
				EvLogError?.Invoke(log);
				break;
			case BlahSaveLoad.ELoadResult.SaveLost:
				EvLogError?.Invoke(log);
				break;
			case BlahSaveLoad.ELoadResult.NoSave:
				EvLogInfo?.Invoke(log);
				break;
			default:
				throw new Exception($"{result} is not supported");
		}

		log = "patch; ";
		if (model != null)
		{
			_patcher.ApplyPatches(model, ref log);
			EvLogInfo?.Invoke(log);
		}

		return model ?? new T();
	}

	/// <summary>
	/// Tries to load custom model without patching.
	/// </summary>
	public T TryLoadRaw<T>(string fileName) where T : class
	{
		_saveLoad.SetTarget(null, fileName);

		var log = "load raw; ";

		T model = null;
		_saveLoad.TryLoad(ref model, ref log);
		EvLogInfo?.Invoke(log);

		return model;
	}

	/// <summary>
	/// Saves model to main save file.<br/>
	/// Model's version will be set automatically.
	/// </summary>
	public void SaveMain<T>(T model) where T : IBlahSaveModelVersion
	{
		_saveLoad.SetTarget(null, SAVE_FILE_NAME);
        
		var log = "save main: ";
		model.Version = _currVersion;
		if (!_saveLoad.TrySaveMain(model, ref log))
			EvLogError?.Invoke(log);
	}

	/// <summary>
	/// Saves model to backup save file.<br/>
	/// Model's version will be set automatically.
	/// </summary>
	public void SaveBackup<T>(T model) where T : IBlahSaveModelVersion
	{
		_saveLoad.SetTarget(null, SAVE_FILE_NAME);

		var log = "save backup: ";
		model.Version = _currVersion;
		if (!_saveLoad.TrySaveBackup(model, ref log))
			EvLogError?.Invoke(log);
	}

	/// <summary>
	/// Checks whether custom model exist.
	/// </summary>
	/// <remarks><b>Do not</b> use in common pipeline, only for complex patching.</remarks>
	public bool IsSaveExist(string folderName, string fileName)
	{
		_saveLoad.SetTarget(folderName, fileName);
		return _saveLoad.IsMainExist() || _saveLoad.IsBackupExist();
	}
	
	/// <returns>Available space in megabytes on hard drive.</returns>
	public float GetAvailableSpaceMbOnDisk() => BlahSavesHelper.GetSpaceMb();
}
}
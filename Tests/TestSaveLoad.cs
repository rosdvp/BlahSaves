using System;
using NUnit.Framework;
using UnityEngine;

namespace BlahSaves.Tests
{
public class TestSaveLoad
{
	[Test]
	public void TestSaveMain_MainLoaded()
	{
		BlahSavesEditor.EditorDeleteAllSaves();

		var saver = new BlahSaver("0");
		saver.EvLogInfo    += Debug.Log;
		saver.EvLogWarning += log => throw new Exception(log);
		saver.EvLogError   += log => throw new Exception(log);

		var saved = new Model();
		saved.IntVal = 5;

		saver.SaveMain(saved);
		saver.LoadOrCreate(out Model loaded);
		
		Assert.AreEqual(5, loaded.IntVal);
	}
	
	[Test]
	public void TestSaveMainBackup_MainLoaded()
	{
		BlahSavesEditor.EditorDeleteAllSaves();

		var saver = new BlahSaver("0");
		saver.EvLogInfo    += Debug.Log;
		saver.EvLogWarning += log => throw new Exception(log);
		saver.EvLogError   += log => throw new Exception(log);

		var saved = new Model();
		saved.IntVal = 5;
		saver.SaveBackup(saved);
		saved.IntVal = 10;
		saver.SaveMain(saved);
		
		saver.LoadOrCreate(out Model loaded);
		
		Assert.AreEqual(10, loaded.IntVal);
	}

	[Test]
	public void TestSaveBackup_BackupLoaded()
	{
		BlahSavesEditor.EditorDeleteAllSaves();

		string logWarning = null;
		
		var saver = new BlahSaver("0");
		saver.EvLogInfo    += Debug.Log;
		saver.EvLogWarning += log =>
		{
			logWarning = log;
			Debug.LogWarning(logWarning);
		};
		saver.EvLogError   += log => throw new Exception(log);

		var saved = new Model();
		saved.IntVal = 5;

		saver.SaveBackup(saved);
		saver.LoadOrCreate(out Model loaded);
		
		Assert.AreEqual(5, loaded.IntVal);
		Assert.IsTrue(logWarning.Contains("backup: success"));
	}

	[Test]
	public void TestSaveMainBackup_DeleteMain_BackupLoaded()
	{
		BlahSavesEditor.EditorDeleteAllSaves();

		string logWarning = null;
		
		var saver = new BlahSaver("0");
		saver.EvLogInfo    += Debug.Log;
		saver.EvLogWarning += log =>
		{
			logWarning = log;
			Debug.LogWarning(log);
		};
		saver.EvLogError   += log => throw new Exception(log);

		var saved = new Model();
		saved.IntVal = 5;
		saver.SaveMain(saved);

		saved.IntVal = 10;
		saver.SaveBackup(saved);
		
		BlahSavesEditor.EditorDeleteMainSave();
		
		saver.LoadOrCreate(out Model loaded);
		
		Assert.AreEqual(10, loaded.IntVal);
		Assert.IsTrue(logWarning.Contains("backup: success"));
	}
	
	[Test]
	public void TestSaveLost_NewModel()
	{
		BlahSavesEditor.EditorDeleteAllSaves();

		string logError = null;
		
		var saver = new BlahSaver("0");
		saver.EvLogInfo    += Debug.Log;
		saver.EvLogWarning += Debug.LogWarning;
		saver.EvLogError   += log =>
		{
			logError = log;
			Debug.LogWarning(logError);
		};

		var saved = new Model();
		saved.IntVal = 5;
		saver.SaveMain(saved);
		saver.SaveBackup(saved);
		
		BlahSavesEditor.EditorDeleteMainSave();
		BlahSavesEditor.EditorDeleteBackupSave();
		
		saver.LoadOrCreate(out Model loaded);
		
		Assert.AreEqual(0, loaded.IntVal);
		Assert.IsTrue(logError != null);
	}

	[Test]
	public void TestBackupNewer_BackupLoaded()
	{
		BlahSavesEditor.EditorDeleteAllSaves();

		string logWarning = null;
		
		var saver = new BlahSaver( "0");
		saver.EvLogInfo += Debug.Log;
		saver.EvLogWarning += log =>
		{
			logWarning = log;
			Debug.LogWarning(log);
		};
		saver.EvLogError += log => throw new Exception(log);

		var saved = new Model();
		saved.IntVal = 5;
		saver.SaveMain(saved);

		saved.IntVal = 10;
		saver.SaveBackup(saved);
		
		saver.LoadOrCreate(out Model loaded);
		
		Assert.AreEqual(10, loaded.IntVal);
		Assert.IsTrue(logWarning.Contains("backup newer: success"));
	}
	

	[Serializable]
	public class Model : IBlahSaveModelVersion
	{
		public int IntVal;
		
		[SerializeField]
		private string _version;
		public string Version
		{
			get => _version;
			set => _version = value;
		}
	}
}
}

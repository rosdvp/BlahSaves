using System;
using NUnit.Framework;
using UnityEditor.VersionControl;
using UnityEngine;

namespace BlahSaves.Tests
{
public class TestSaveLoad
{
	[Test]
	public void TestNoSaves_ResultNoSaves()
	{
		BlahSavesEditor.EditorDeleteAllSaves();

		var saver = new BlahSaver("0");
        
		var result = saver.Load(out Model loaded, out _);
		
		Assert.AreEqual(ELoadResult.NoSaves, result);
		Assert.IsNull(loaded);
	}
	
	[Test]
	public void TestSaveOnlyMain_MainLoaded()
	{
		BlahSavesEditor.EditorDeleteAllSaves();

		var saver = new BlahSaver("0");

		saver.SaveMain(new Model {IntVal = 5});
		var result = saver.Load(out Model loaded, out _);
		
		Assert.AreEqual(ELoadResult.MainLoaded, result);
		Assert.AreEqual(5, loaded.IntVal);
	}
	
	[Test]
	public void TestSaveMainAndBackup_MainLoaded()
	{
		BlahSavesEditor.EditorDeleteAllSaves();

		var saver = new BlahSaver("0");

		saver.SaveBackup(new Model() { IntVal = 5});
		saver.SaveMain(new Model() { IntVal = 10});
		
		var result = saver.Load(out Model loaded, out _);
		
		Assert.AreEqual(ELoadResult.MainLoaded, result);
		Assert.AreEqual(10, loaded.IntVal);
	}

	[Test]
	public void TestSaveOnlyBackup_BackupLoaded()
	{
		BlahSavesEditor.EditorDeleteAllSaves();

		string logWarning = null;
		
		var saver = new BlahSaver("0");

		saver.SaveBackup(new Model { IntVal = 5 });
		
		var result = saver.Load(out Model loaded, out _);
		
		Assert.AreEqual(ELoadResult.MainFailedBackupLoaded, result);
		Assert.AreEqual(5, loaded.IntVal);
	}

	[Test]
	public void TestSaveMainAndBackup_DeleteMain_BackupLoaded()
	{
		BlahSavesEditor.EditorDeleteAllSaves();

		string logWarning = null;
		
		var saver = new BlahSaver("0");

		saver.SaveBackup(new Model { IntVal = 10 });
		saver.SaveMain(new Model { IntVal   = 5 });
		
		BlahSavesEditor.EditorDeleteMainSave();
		
		var result = saver.Load(out Model loaded, out _);
		
		Assert.AreEqual(ELoadResult.MainFailedBackupLoaded, result);
		Assert.AreEqual(10, loaded.IntVal);
	}
	
	[Test]
	public void TestSaveOnlyInvalidMain_NothingLoaded()
	{
		BlahSavesEditor.EditorDeleteAllSaves();

		var saver = new BlahSaver("0");

		saver.SaveMain(new WrongModel());
		
		var result = saver.Load(out Model loaded, out _);
		
		Assert.AreEqual(ELoadResult.MainFailedBackupFailed, result);
		Assert.IsNull(loaded);
	}
	
	[Test]
	public void TestSaveInvalidMainAndValidBackup_BackupLoaded()
	{
		BlahSavesEditor.EditorDeleteAllSaves();

		var saver = new BlahSaver("0");

		saver.SaveBackup(new Model { IntVal = 5 });
		saver.SaveMain(new WrongModel());
		
		var result = saver.Load(out Model loaded, out _);
		
		Assert.AreEqual(ELoadResult.MainFailedBackupLoaded, result);
		Assert.AreEqual(5, loaded.IntVal);
	}
	
	[Test]
	public void TestSaveMainAndBackup_DeleteBoth_SavesLost()
	{
		BlahSavesEditor.EditorDeleteAllSaves();
		
		var saver = new BlahSaver("0");

		saver.SaveMain(new Model { IntVal   = 5 });
		saver.SaveBackup(new Model { IntVal = 10 });
		
		BlahSavesEditor.EditorDeleteMainSave();
		BlahSavesEditor.EditorDeleteBackupSave();
		
		var result = saver.Load(out Model loaded, out _);
		
		Assert.AreEqual(ELoadResult.MainFailedBackupFailed, result);
		Assert.IsNull(loaded);
	}

	[Test]
	public void TestSaveBackupAfterMain_BackupLoaded()
	{
		BlahSavesEditor.EditorDeleteAllSaves();

		var saver = new BlahSaver("0");

		saver.SaveMain(new Model { IntVal   = 5});
		saver.SaveBackup(new Model { IntVal = 10 });
		
		var result = saver.Load(out Model loaded, out _);
		
		Assert.AreEqual(ELoadResult.BackupNewerLoaded, result);
		Assert.AreEqual(10, loaded.IntVal);
	}
	
	[Test]
	public void TestSaveInvalidBackupAfterValidMain_MainLoaded()
	{
		BlahSavesEditor.EditorDeleteAllSaves();

		var saver = new BlahSaver("0");

		saver.SaveMain(new Model { IntVal = 5 });
		saver.SaveBackup(new WrongModel());
		
		var result = saver.Load(out Model loaded, out _);
		
		Assert.AreEqual(ELoadResult.BackupNewerFailedMainLoaded, result);
		Assert.AreEqual(5, loaded.IntVal);
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

	[Serializable]
	public class WrongModel : IBlahSaveModelVersion
	{
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

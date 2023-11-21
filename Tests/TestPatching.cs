using System;
using NUnit.Framework;
using UnityEngine;

namespace BlahSaves.Tests
{
public class TestPatching
{
	[Test]
	public void TestOnePatch_Patched()
	{
		var saver = PrepareSaver("0", true);

		var loader = PrepareSaver("1", false);
		loader.AddPatch("0",
		                "1",
		                rawModel =>
		                {
			                if (rawModel is Model model)
				                model.BoolVal = true;
		                }
		);
		
		var saved = new Model();
		saved.IntVal  = 5;
		saved.BoolVal = false;
		
		saver.SaveMain(saved);

		loader.Load(out Model loaded, out _);
		
		Assert.AreEqual(5, loaded.IntVal);
		Assert.AreEqual(true, loaded.BoolVal);
	}
	
	[Test]
	public void TestThreePatches_Patched()
	{
		var saver = PrepareSaver("0", true);

		var loader = PrepareSaver("3", false);
		loader.AddPatch("0",
		                "1",
		                rawModel =>
		                {
			                if (rawModel is Model model)
				                model.IntVal = 6;
		                }
		);
		loader.AddPatch("1",
		                "2",
		                rawModel =>
		                {
			                if (rawModel is Model model)
				                model.IntVal = 7;
		                }
		);
		loader.AddPatch("2",
		                "3",
		                rawModel =>
		                {
			                if (rawModel is Model model)
			                {
				                model.IntVal  = 8;
				                model.BoolVal = true;
			                }
		                }
		);
		
		var saved = new Model();
		saved.IntVal  = 5;
		saved.BoolVal = false;
		
		saver.SaveMain(saved);

		loader.Load(out Model loaded, out _);
		
		Assert.AreEqual(8, loaded.IntVal);
		Assert.AreEqual(true, loaded.BoolVal);
	}


	private BlahSaver PrepareSaver(string version, bool withSavesDelete)
	{
		if (withSavesDelete)
			BlahSavesEditor.EditorDeleteAllSaves();
		return new BlahSaver(version);
	}


	[Serializable]
	public class Model : IBlahSaveModelVersion
	{
		public int  IntVal;
		public bool BoolVal;

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
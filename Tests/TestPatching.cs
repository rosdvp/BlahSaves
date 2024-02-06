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
		var saved = new Model();
		saved.IntVal  = 5;
		saved.BoolVal = false;

		var saver = PrepareSaver(true);
		saver.SaveMain(saved);

		saver.AddPatch(
			"p1",
			rawModel =>
			{
				if (rawModel is Model model && model.BoolVal == false)
				{
					model.BoolVal = true;
					return true;
				}
				return false;
			}
		);
		saver.Load(out Model loaded, out string log);
		
		Debug.Log(log);
		
		Assert.AreEqual(5, loaded.IntVal);
		Assert.AreEqual(true, loaded.BoolVal);
	}

	[Test]
	public void TestThreePatches_Patched()
	{
		var saved = new Model();
		saved.IntVal  = 5;
		saved.BoolVal = false;

		var saver = PrepareSaver(true);
		saver.SaveMain(saved);

		saver.AddPatch(
			"p1",
			rawModel =>
			{
				if (rawModel is Model model && model.IntVal == 4)
				{
					model.IntVal = 3;
					return true;
				}
				return false;
			}
		);
		saver.AddPatch(
			"p2",
			rawModel =>
			{
				if (rawModel is Model model && model.IntVal == 5)
				{
					model.IntVal = 6;
					return true;
				}
				return false;
			}
		);
		saver.AddPatch(
			"p3",
			rawModel =>
			{
				if (rawModel is Model model && model.IntVal == 6)
				{
					model.IntVal = 7;
					return true;
				}
				return false;
			}
		);
		saver.Load(out Model loaded, out string log);
		
		Debug.Log(log);

		Assert.AreEqual(7, loaded.IntVal);
		Assert.AreEqual(false, loaded.BoolVal);
	}


	private BlahSaver PrepareSaver(bool withSavesDelete)
	{
		if (withSavesDelete)
			BlahSavesEditor.EditorDeleteAllSaves();
		return new BlahSaver();
	}


	[Serializable]
	public class Model : IBlahSaveModel
	{
		public int  IntVal;
		public bool BoolVal;
	}
}
}
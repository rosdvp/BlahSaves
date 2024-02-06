using System;
using System.Collections.Generic;

namespace BlahSaves
{
public class BlahPatcher
{
	private List<Patch> _patches = new();

	public void AddPatch(string name, Func<IBlahSaveModel, bool> action)
	{
		_patches.Add(new Patch
			{
				Name = name,
				Func = action
			}
		);
	}

	public void ApplyPatches(IBlahSaveModel model, out string log)
	{
		log = "";
		foreach (var patch in _patches)
			if (patch.Func.Invoke(model))
				log += $"applied {patch.Name}; ";
		log += "done; ";
	}

	private class Patch
	{
		public string                     Name;
		public Func<IBlahSaveModel, bool> Func;
	}
}
}
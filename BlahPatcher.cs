using System;
using System.Collections.Generic;

namespace BlahSaves
{
public class BlahPatcher
{
	private List<Patch> _patches = new();

	public void AddPatch(string fromVersion, string toVersion, Action<object> action)
	{
		_patches.Add(new Patch
			{
				FromVersion = fromVersion,
				ToVersion   = toVersion,
				Action      = action
			}
		);
	}

	public void ApplyPatches(object model, out string log)
	{
		log = "";
		
		var counter = 0;
		while (counter < 1000)
		{
			string version = null;
			if (model is IBlahSaveModelVersion modelVerBefore)
				version = modelVerBefore.Version;
			
			var patch = _patches.Find(p => p.FromVersion == version);
			if (patch == null)
			{
				log += $"done v: {version};";
				break;
			}
			
			patch.Action?.Invoke(model);
			if (model is IBlahSaveModelVersion modelVerAfter)
				modelVerAfter.Version = patch.ToVersion;
			log += $"{patch.FromVersion} -> {patch.ToVersion}; ";

			counter += 1;
		}
	}

	private class Patch
	{
		public string FromVersion;
		public string ToVersion;
		public Action<object> Action;
	}
}
}
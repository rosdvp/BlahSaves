
# Blah Saves
The purpose of this Unity package is to simplify the process of game saving/loading with backups and saves migration from one data structure to another.

* The package depends on Odin Serializer.

## Usage
* The whole content of the Model is binary serialized in one file "saves/saves.bdt" (or "b_saves.bdt" for backup).
* For Android: the path to the internal app storage is retrieved from native layer.
* For other platforms: Application.persistentDataPath is used.

### Standard pipieline
```csharp
// Serializing class should implement the interface.
[Serializable]
class Model : IBlahSaveModel
{
    public int X;
}
var model = new Model();

// Create an instance.
var saver = new BlahSaver();

// Subscribe to informational logs.
saver.EvLogInfo += (string log) => Debug.Log(log);
// Subscribe to error logs.
saver.EvLogError += (string log) => Debug.Log(log);

// Serailize model into main savings file.
saver.SaveMain(model);
// Serialize model into backup savings file.
saver.SaveBackup(model);

// Try to deserialize model to object and get additional logs.
ELoadResult result = saver.Load<Model>(out Model loadedModel, out string logs);

if (result == ELoadResult.MainLoaded)
    // Model successfully deserialized from main savings file.
else if (result == ELoadResult.BackupNewerLoaded)
    // Timestamp of backup is newer than timestamp of main file,
    // so backup is loaded.
else if (result == ELoadResult.BackupNewerFailedMainLoaded)
    // Timestamp of backup is newer than timestamp of main file,
    // however backup is corrupted so main file is loaded.
else if (result == ELoadResult.MainFailedBackupLoaded)
    // Main file is corrupted (or missing), but backup is loaded successfully.
else if (result == ELoadResult.MainFailedBackupFailed)
    // Both main file and backup file are corrupted (or missing).
else if (result == ELoadResult.NoSaves)
    // Both main file and backup file do not exist and there is no evidence that they should exist.

```

### Migration

You can use patching mechanism to migrate data between different layouts.

```csharp
[Serializable]
class Model : IBlahSaveModel
{
    public int Version;
    
    public int ObsoleteField;
    public string NewField;
}

var saver = new BlahSaver();
// Add patches in order they should be applied.
saver.AddPatch("v0-v1", PatchV0ToV1);
saver.AddPath("v1-v2", PatchV1ToV2);

// Added patches are applied after model is successfully deserialized.
var result = saver.Load<Model>(out Model patchedModel, out string logs);


bool PatchV0ToV1(IBlahSaveModel rawModel)
{
    var model = (Model)rawModel;
    if (model.Version != 0)
        return false; // Patch is not applied.

    // Suppose, you want to convert old field into a new one.
    model.NewField = model.ObsoleteField.ToString();
    model.Version = 1;
    return true; // Patch is applied.
}

bool PatchV1ToV2(IBlahSaveModel rawModel)
{
    var model = (Model)rawModel;
    if (model.Version != 1)
        return false; // Patch is not applied.
    // Suppose, nothing should be changed.
    model.Version = 2;
    return true;
}
```
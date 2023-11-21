namespace BlahSaves
{
public enum ELoadResult
{
	MainLoaded,
	BackupNewerLoaded,
	BackupNewerFailedMainLoaded,
	MainFailedBackupLoaded,
	MainFailedBackupFailed,
	NoSaves,
}
}
using UnityEngine;

namespace BlahSaves
{
internal static class BlahSavesHelper
{
	public static string GetPath()
	{
#if UNITY_EDITOR
		return Application.persistentDataPath;
#elif UNITY_ANDROID
		return GetAndroidPath();
#else
		return Application.persistentDataPath;
#endif
	}

	public static float GetSpaceMb()
	{
#if UNITY_EDITOR
		return float.MaxValue;
#elif UNITY_ANDROID
		return GetAndroidSpaceMb();
#else
		return float.MaxValue;
#endif
	}


#if UNITY_ANDROID
	private static string GetAndroidPath()
	{
		using var player    = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		using var activity  = player.GetStatic<AndroidJavaObject>("currentActivity");
		using var context   = activity.Call<AndroidJavaObject>("getApplicationContext");
		using var filesDir  = context.Call<AndroidJavaObject>("getFilesDir");
		var       filesPath = filesDir.Call<string>("getCanonicalPath");
		return filesPath;
	}

	private static float GetAndroidSpaceMb()
	{
		using var player   = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
		using var context  = activity.Call<AndroidJavaObject>("getApplicationContext");
		using var filesDir = context.Call<AndroidJavaObject>("getFilesDir");
		var    path     = filesDir.Call<string>("getPath");

		using var statsFs        = new AndroidJavaObject("android.os.StatFs", path);
		var       blockSize      = statsFs.Call<long>("getBlockSizeLong");
		var       blocksCount    = statsFs.Call<long>("getAvailableBlocksLong");
		long      bytesAvailable = blockSize * blocksCount;
		return bytesAvailable / (1024.0f * 1024.0f);
	}
#endif
}
}
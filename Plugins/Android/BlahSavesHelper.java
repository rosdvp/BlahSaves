package com.blah.saves;

import android.app.Activity;
import android.content.Context;
import android.os.StatFs;

import java.io.File;
import java.io.IOException;

public class BlahSavesHelper
{
    public static String GetPath(Activity activity) throws IOException {
        Context context = activity.getApplicationContext();
        File dir = context.getFilesDir();
        return dir.getCanonicalPath();
    }

    public static float GetAvailableSpaceMB(Activity activity) {
        Context context = activity.getApplicationContext();
        File dir = context.getFilesDir();
        
        StatFs stat = new StatFs(dir.getPath());
        long bytesAvailable = 0;
        bytesAvailable = stat.getBlockSizeLong() * stat.getAvailableBlocksLong();
        return bytesAvailable / (1024.f * 1024.f);
    }
}
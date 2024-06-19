using System;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class AssetConfigManager
{
    private static AssetConfigManager instance;
    public static AssetConfigManager Instance
    {
        get
        {
            if (instance == null)
                instance = new AssetConfigManager();
            return instance;
        }
    }

    AssetConfigScriptableObject assetConfigData = null;
    long loadConfigTime = 0;

    public AssetConfigScriptableObject GetAssetConfigScriptableObject()
    {
        if (loadConfigTime == 0)
        {
            return LoadConfigData();
        }

        string timeS = PlayerPrefs.GetString("AssetConfigSavePathTime", "0");
        if (long.TryParse(timeS, out long saveTime))
        {
            if (saveTime == 0)
            {
                return null;
            }
            else if (saveTime >= loadConfigTime)
            {
                return LoadConfigData();
            }
        }

        if (assetConfigData == null)
        {
            LoadConfigData();
        }
        return assetConfigData;
    }

    private AssetConfigScriptableObject LoadConfigData()
    {
        string value = PlayerPrefs.GetString("AssetConfigSavePath", "");
        if (!string.IsNullOrEmpty(value))
        {
            assetConfigData = AssetDatabase.LoadAssetAtPath<AssetConfigScriptableObject>(value) as AssetConfigScriptableObject;
            loadConfigTime = DateTime.Now.Ticks;
        }
        return assetConfigData;
    }
}
#endif

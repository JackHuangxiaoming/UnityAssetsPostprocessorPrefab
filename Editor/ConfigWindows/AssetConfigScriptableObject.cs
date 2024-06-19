using Cinemachine;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;

public class AssetConfigScriptableObject : ScriptableObject
{
    #region Base 

    /// <summary>
    /// 是否启用资源检查
    /// </summary>    
    [Tooltip("是否启用资源检查"), TabGroup("Base")]
    public bool EnableAssetCheck = true;
    /// <summary>
    /// 是否启用模型自动配置
    /// </summary>
    [Tooltip("是否启用模型自动配置"), TabGroup("Base")]
    public bool EnableModelsConfig = true;
    /// <summary>
    /// 是否启动材质球自动配置
    /// </summary>
    [Tooltip("是否启动材质球自动配置"), TabGroup("Base")]
    public bool EnableMaterialConfig = true;
    /// <summary>
    /// 是否启动预制件自动创建更新
    /// </summary>
    [Tooltip("是否启动预制件自动创建更新"), TabGroup("Base")]
    public bool EnableLODPrefabCreate = true;

    [Tooltip("资源根节点"), TabGroup("Base")]
    public string AssetRootPaht = "Assets/___Meta48/Content/Arts/RoomUGC/";

    public bool SaveBaseConfig(bool enableAssetCheck, bool enableModelsConfig, bool enableMaterialConfig, bool enableLODPrefabCreate, string assetRootPaht)
    {
        if (string.IsNullOrEmpty(assetRootPaht))
        {
            Debug.LogError($"SaveBaseConfig 错误 工作根文件夹设置错误 ：{assetRootPaht}");
            return false;
        }

        EnableAssetCheck = enableAssetCheck;
        EnableModelsConfig = enableModelsConfig;
        EnableMaterialConfig = enableMaterialConfig;
        EnableLODPrefabCreate = enableLODPrefabCreate;
        AssetRootPaht = assetRootPaht;
        return true;
    }
    #endregion

    #region AssetCheck    
    [FoldoutGroup("AssetCheck")]
    [ReadOnly]
    public string pattern_Extensions_Mat = "(.mat)$";
    [FoldoutGroup("AssetCheck")]
    [ReadOnly]
    public string pattern_Extensions_FBX = ".(FBX|fbx)$";
    [FoldoutGroup("AssetCheck")]
    [ReadOnly]
    public string pattern_Extensions_Texture = "(.tga)$";
    [FoldoutGroup("AssetCheck")]
    [ReadOnly]
    public string pattern_Extensions_Prefab = ".(prefab)$";
    [FoldoutGroup("AssetCheck")]
    [ReadOnly]
    public string patternNameMat = @"^(Mat_).+(_LOD\d*|\d+)$";
    [FoldoutGroup("AssetCheck")]
    [ReadOnly]
    public string patternNameFBX = @"^(Sm_|SM_).+(_LOD\d*|_collider)$";
    [FoldoutGroup("AssetCheck")]
    [ReadOnly]
    public string patternNameTexture = @"^(T_).+_(D|E|M|N)$";
    [FoldoutGroup("AssetCheck")]
    [ReadOnly]
    public string patternNamePrefab = @"^(P_)(.*)$|^(S[Mm])_.+(_LOD\d*|_collider)$";

    [FoldoutGroup("AssetCheck")]
    public string[] Extensions_Texture = new string[] { ".tga" };
    [FoldoutGroup("AssetCheck")]
    public string[] Pattern_Mat = new string[] { "Mat_" };
    [FoldoutGroup("AssetCheck")]
    public string[] Pattern_Mat_Tag = new string[] { "_LOD\\d*", "\\d+" };
    [FoldoutGroup("AssetCheck")]
    public string[] Pattern_FBX = new string[] { "Sm_", "SM_" };
    [FoldoutGroup("AssetCheck")]
    public string[] Pattern_FBX_Tag = new string[] { "_LOD\\d*", "_collider" };
    [FoldoutGroup("AssetCheck")]
    public string[] Pattern_Texture = new string[] { "T_" };
    [FoldoutGroup("AssetCheck")]
    public string[] Pattern_Texture_Tag = new string[] { "D", "E", "M", "N" };
    [FoldoutGroup("AssetCheck")]
    public string[] Pattern_Prefab = new string[] { "P_" };
    [FoldoutGroup("AssetCheck")]
    public string[] Pattern_Prefab_Tag = new string[] { ".*" };
    [FoldoutGroup("AssetCheck")]
    public string[] Pattern_Prefab1 = new string[] { "S[Mm]" };
    [FoldoutGroup("AssetCheck")]
    public string[] Pattern_Prefab_Tag1 = new string[] { "_LOD\\d*", "_collider" };

    public bool SaveExtensions_Texture(string[] tags)
    {
        string format = "({0})$";
        if (tags.Any(x => string.IsNullOrEmpty(x)))
        {
            Debug.LogError("SaveExtensions_Texture 保存异常：" + tags);
            return false;
        }
        Extensions_Texture = tags;
        pattern_Extensions_Texture = string.Format(format, string.Join("|", Extensions_Texture));
        return true;
    }
    public bool SavePatternName_Mat(string[] frist, string[] tags)
    {
        string format = @"^({0}).+({1})$";
        if (frist.Any(x => string.IsNullOrEmpty(x)))
        {
            Debug.LogError("SavePatternName_Mat 保存异常：" + frist);
            return false;
        }
        if (tags.Any(x => string.IsNullOrEmpty(x)))
        {
            Debug.LogError("SavePatternName_Mat 保存异常：" + tags);
            return false;
        }
        Pattern_Mat = frist;
        Pattern_Mat_Tag = tags;
        patternNameMat = string.Format(format, string.Join("|", frist), string.Join("|", tags));
        return true;
    }
    public bool SavePatternName_FBX(string[] frist, string[] tags)
    {
        string format = @"^({0}).+({1})$";
        if (frist.Any(x => string.IsNullOrEmpty(x)))
        {
            Debug.LogError("SavePatternName_FBX 保存异常：" + frist);
            return false;
        }
        if (tags.Any(x => string.IsNullOrEmpty(x)))
        {
            Debug.LogError("SavePatternName_FBX 保存异常：" + tags);
            return false;
        }
        Pattern_FBX = frist;
        Pattern_FBX_Tag = tags;
        patternNameFBX = string.Format(format, string.Join("|", frist), string.Join("|", tags));
        return true;
    }
    public bool SavePatternName_Texture(string[] frist, string[] tags)
    {
        string format = @"^({0}).+_({1})$";
        if (frist.Any(x => string.IsNullOrEmpty(x)))
        {
            Debug.LogError("SavePatternName_Texture 保存异常：" + frist);
            return false;
        }
        if (tags.Any(x => string.IsNullOrEmpty(x)))
        {
            Debug.LogError("SavePatternName_Texture 保存异常：" + tags);
            return false;
        }
        Pattern_Texture = frist;
        Pattern_Texture_Tag = tags;
        patternNameTexture = string.Format(format, string.Join("|", frist), string.Join("|", tags));
        return true;
    }
    public bool SavePatternName_Prefab(string[] frist0, string[] tags0, string[] frist1, string[] tags1)
    {
        string format = @"^({0})({1})$|^({2})_.+({3})$";
        if (frist0.Any(x => string.IsNullOrEmpty(x)))
        {
            Debug.LogError("SavePatternName_Prefab 保存异常：" + frist0);
            return false;
        }
        if (tags0.Any(x => string.IsNullOrEmpty(x)))
        {
            Debug.LogError("SavePatternName_Prefab 保存异常：" + tags0);
            return false;
        }
        if (frist1.Any(x => string.IsNullOrEmpty(x)))
        {
            Debug.LogError("SavePatternName_Prefab 保存异常：" + frist1);
            return false;
        }
        if (tags1.Any(x => string.IsNullOrEmpty(x)))
        {
            Debug.LogError("SavePatternName_Prefab 保存异常：" + tags1);
            return false;
        }
        Pattern_Prefab = frist0;
        Pattern_Prefab_Tag = tags0;
        Pattern_Prefab1 = frist1;
        Pattern_Prefab_Tag1 = tags1;
        patternNamePrefab = string.Format(format, string.Join("|", frist0), string.Join("|", tags0), string.Join("|", frist1), string.Join("|", tags1));
        return true;
    }
    #endregion

    #region Models

    [Tooltip("自动材质球设置：裁剪材质球命名前缀"), FoldoutGroup("Models")]
    public string ClipMatPrefix = "Mat_";
    [Tooltip("自动材质球设置：裁剪材质球命名后缀"), FoldoutGroup("Models")]
    public string ClipMatSuffix = "_LOD";
    [Tooltip("自动材质球设置：查找的贴图名字前缀"), FoldoutGroup("Models")]
    public string FormatTexturePrefix = "T_";

    [Tooltip("自动勾选第二套UV 后缀"), FoldoutGroup("Models")]
    public string AutoGenerateUV2 = "LOD0";

    internal bool SaveModelsConfig(string clipMatPrefix, string clipMatSuffix, string formatTexturePrefix, string autoGenerateUV2)
    {
        if (string.IsNullOrEmpty(clipMatPrefix) || string.IsNullOrEmpty(clipMatSuffix) || string.IsNullOrEmpty(formatTexturePrefix) || string.IsNullOrEmpty(autoGenerateUV2))
        {
            Debug.LogError($"SaveModelsConfig 错误 ：{clipMatPrefix} {clipMatSuffix} {formatTexturePrefix} {autoGenerateUV2}");
            return false;
        }
        ClipMatPrefix = clipMatPrefix;
        ClipMatSuffix = clipMatSuffix;
        FormatTexturePrefix = formatTexturePrefix;
        AutoGenerateUV2 = autoGenerateUV2;
        return true;
    }

    #endregion

    #region LODPrefab    
    [Tooltip("自动创建预制件 ：向上查找工作文件夹"), FoldoutGroup("LODPrefab")]
    public string PrefabMeshesFolder = "/Meshes";
    [Tooltip("自动创建预制件 ：创建工作文件夹"), FoldoutGroup("LODPrefab")]
    public string PrefabGenerateFolder = "/Prefabs";
    [Tooltip("自动创建预制件 ：物理碰撞体Layer"), FoldoutGroup("LODPrefab")]
    public string PrefabColliderLayer = "Physics_World_Default";

    internal bool SaveLODPrefabConfig(string prefabMeshesFolder, string prefabGenerateFolder, string prefabColliderLayer)
    {
        if (string.IsNullOrEmpty(prefabMeshesFolder) || string.IsNullOrEmpty(prefabGenerateFolder) || string.IsNullOrEmpty(prefabColliderLayer))
        {
            Debug.LogError($"SaveLODPrefabConfig 错误 ：{prefabMeshesFolder} {prefabGenerateFolder} {prefabColliderLayer}");
            return false;
        }
        PrefabMeshesFolder = prefabMeshesFolder;
        PrefabGenerateFolder = prefabGenerateFolder;
        PrefabColliderLayer = prefabColliderLayer;
        return true;
    }
    #endregion
}

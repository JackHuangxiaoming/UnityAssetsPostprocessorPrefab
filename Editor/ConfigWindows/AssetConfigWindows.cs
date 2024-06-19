using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

internal class AssetConfigWindows : EditorWindow
{
    [MenuItem("Tools/AssetTool/资产配置面板", false, 1)]
    public static void ShowAssetsConfigWindows()
    {
        EditorWindow.GetWindow(typeof(AssetConfigWindows), false, "资产配置");
    }

    SerializedObject serializedObject;
    public static AssetConfigScriptableObject CurrentProjectAsset;//当前想项目使用的配置
    public static AssetConfigScriptableObject LastCurrentProjectAsset;


    bool foldout_AssetCheck = false;
    bool foldout_AssetModels = false;
    bool foldout_LODPrefab = false;

    GUIStyle redTextStyle;
    Vector2 scrollPos = Vector2.zero;

    #region Base

    [Tooltip("是否启用资源检查")]
    public bool EnableAssetCheck = true;
    [Tooltip("是否启用模型自动配置")]
    public bool EnableModelsConfig = true;
    [Tooltip("是否启动材质球自动配置")]
    public bool EnableMaterialConfig = true;
    [Tooltip("是否启动预制件自动创建更新")]
    public bool EnableLODPrefabCreate = true;
    [Tooltip("资源根节点")]
    public string AssetRootPaht = "Assets/___Meta48/Content/Arts/RoomUGC";

    #endregion
    #region 资源检查
    [Tooltip("贴图文件后缀名")]
    public string[] Extensions_Texture;
    [Tooltip("材质球命名规则 开头")]
    public string[] Pattern_Mat;
    [Tooltip("材质球命名规则 结尾")]
    public string[] Pattern_Mat_Tag;
    [Tooltip("模型FBX命名规则 开头")]
    public string[] Pattern_FBX;
    [Tooltip("模型FBX命名规则 结尾")]
    public string[] Pattern_FBX_Tag;
    [Tooltip("贴图命名规则 开头")]
    public string[] Pattern_Texture;
    [Tooltip("贴图命名规则 结尾")]
    public string[] Pattern_Texture_Tag;
    [Tooltip("预制件命名规则 开头0")]
    public string[] Pattern_Prefab;
    [Tooltip("预制件命名规则 结尾0")]
    public string[] Pattern_Prefab_Tag;
    [Tooltip("预制件命名规则 开头1")]
    public string[] Pattern_Prefab1;
    [Tooltip("预制件命名规则 结尾1")]
    public string[] Pattern_Prefab_Tag1;
    #endregion

    #region Models

    [Tooltip("自动材质球设置：裁剪材质球命名前缀")]
    public string ClipMatPrefix = "Mat_";
    [Tooltip("自动材质球设置：裁剪材质球命名后缀")]
    public string ClipMatSuffix = "_LOD";
    [Tooltip("自动材质球设置：查找的贴图名字前缀")]
    public string FormatTexturePrefix = "T_";
    [Tooltip("自动勾选第二套UV 后缀")]
    public string AutoGenerateUV2 = "LOD0";

    #endregion

    #region LODPrefab    
    [Tooltip("自动创建预制件 ：向上查找工作文件夹")]
    public string PrefabMeshesFolder = "Meshes";
    [Tooltip("自动创建预制件 ：创建工作文件夹")]
    public string PrefabGenerateFolder = "Prefabs";
    [Tooltip("自动创建预制件 ：物理碰撞体Layer")]
    public string PrefabColliderLayer = "Physics_World_Default";
    #endregion


    private void OnEnable()
    {
        string value = PlayerPrefs.GetString("AssetConfigSavePath", "");
        if (!string.IsNullOrEmpty(value))
        {
            CurrentProjectAsset = AssetDatabase.LoadAssetAtPath<AssetConfigScriptableObject>(value) as AssetConfigScriptableObject;
            LastCurrentProjectAsset = CurrentProjectAsset;
        }
        if (redTextStyle == null)
        {
            redTextStyle = new GUIStyle(EditorStyles.boldLabel);
            redTextStyle.normal.textColor = Color.red;
            redTextStyle.fontStyle = FontStyle.Bold;
        }
    }
    private void OnGUI()
    {
        DrawHorizontalLine();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("当前项目配置：", EditorStyles.boldLabel);
        CurrentProjectAsset = (AssetConfigScriptableObject)EditorGUILayout.ObjectField(CurrentProjectAsset, typeof(AssetConfigScriptableObject), false);
        if (GUILayout.Button("New Asset"))
        {
            CreateNewScriptableObject();
        }
        EditorGUILayout.EndHorizontal();

        DrawHorizontalLine();

        if (CurrentProjectAsset == null)
        {
            if (LastCurrentProjectAsset == CurrentProjectAsset)
                return;
            LastCurrentProjectAsset = null;
            EditorGUILayout.LabelField("No ScriptableObject selected.", EditorStyles.label);
            PlayerPrefs.SetString("AssetConfigSavePath", "");
            PlayerPrefs.SetString("AssetConfigSavePathTime", "0");
            PlayerPrefs.SetString("AssetConfigSavePathRoot", "");
            return;
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Config:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(CurrentProjectAsset.name, redTextStyle);
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            CheckSerializedObjectData();

            if (CurrentProjectAsset != LastCurrentProjectAsset)
            {
                LastCurrentProjectAsset = CurrentProjectAsset;
                PlayerPrefs.SetString("AssetConfigSavePath", AssetDatabase.GetAssetPath(CurrentProjectAsset));
                PlayerPrefs.SetString("AssetConfigSavePathTime", DateTime.Now.Ticks.ToString());
                PlayerPrefs.SetString("AssetConfigSavePathRoot", CurrentProjectAsset.AssetRootPaht);
            }
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        DrawBase();
        DrawHorizontalLine();

        foldout_AssetCheck = EditorGUILayout.Foldout(foldout_AssetCheck, "AssetCheck");
        if (foldout_AssetCheck)
        {
            DrawAssetCheck();
            DrawHorizontalLine();
        }
        foldout_AssetModels = EditorGUILayout.Foldout(foldout_AssetModels, "AssetModels");
        if (foldout_AssetModels)
        {
            DrawAssetModels();
            DrawHorizontalLine();
        }
        foldout_LODPrefab = EditorGUILayout.Foldout(foldout_LODPrefab, "LODPrefab");
        if (foldout_LODPrefab)
        {
            DrawLODPrefab();
            DrawHorizontalLine();
        }

        EditorGUILayout.EndScrollView();
    }
    private void DrawBase()
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("EnableAssetCheck"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("EnableModelsConfig"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("EnableMaterialConfig"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("EnableLODPrefabCreate"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("AssetRootPaht"));

        serializedObject.ApplyModifiedProperties();

        // 保存按钮
        if (GUILayout.Button("Save Base Config"))
        {
            bool sucess = CurrentProjectAsset.SaveBaseConfig(EnableAssetCheck, EnableModelsConfig, EnableMaterialConfig, EnableLODPrefabCreate, AssetRootPaht);
            if (sucess)
            {
                EditorUtility.SetDirty(CurrentProjectAsset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Save Base Config", "Save Config Success", "OK");
            }
        }
    }
    private void DrawLODPrefab()
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("PrefabMeshesFolder"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("PrefabGenerateFolder"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("PrefabColliderLayer"));

        serializedObject.ApplyModifiedProperties();

        // 保存按钮
        if (GUILayout.Button("Save LOD Prefab Create Config"))
        {
            bool sucess = CurrentProjectAsset.SaveLODPrefabConfig(PrefabMeshesFolder, PrefabGenerateFolder, PrefabColliderLayer);
            if (sucess)
            {
                EditorUtility.SetDirty(CurrentProjectAsset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Save LOD Prefab Create Config", "Save Config Success", "OK");
            }
        }
    }
    private void DrawAssetModels()
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ClipMatPrefix"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ClipMatSuffix"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("FormatTexturePrefix"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("AutoGenerateUV2"));

        serializedObject.ApplyModifiedProperties();

        // 保存按钮
        if (GUILayout.Button("Save Asset Models Config"))
        {
            bool sucess = CurrentProjectAsset.SaveModelsConfig(ClipMatPrefix, ClipMatSuffix, FormatTexturePrefix, AutoGenerateUV2);
            if (sucess)
            {
                EditorUtility.SetDirty(CurrentProjectAsset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Save Asset Models Config", "Save Config Success", "OK");
            }
        }
    }
    private void DrawAssetCheck()
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Extensions_Texture"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Pattern_Mat"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Pattern_Mat_Tag"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Pattern_FBX"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Pattern_FBX_Tag"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Pattern_Texture"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Pattern_Texture_Tag"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Pattern_Prefab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Pattern_Prefab_Tag"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Pattern_Prefab1"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Pattern_Prefab_Tag1"));
        serializedObject.ApplyModifiedProperties();

        // 保存按钮
        if (GUILayout.Button("Save Asset Check Config"))
        {
            bool sucess = CurrentProjectAsset.SaveExtensions_Texture(Extensions_Texture);
            sucess &= CurrentProjectAsset.SavePatternName_Mat(Pattern_Mat, Pattern_Mat_Tag);
            sucess &= CurrentProjectAsset.SavePatternName_FBX(Pattern_FBX, Pattern_FBX_Tag);
            sucess &= CurrentProjectAsset.SavePatternName_Texture(Pattern_Texture, Pattern_Texture_Tag);
            sucess &= CurrentProjectAsset.SavePatternName_Prefab(Pattern_Prefab, Pattern_Prefab_Tag, Pattern_Prefab1, Pattern_Prefab_Tag1);
            if (sucess)
            {
                EditorUtility.SetDirty(CurrentProjectAsset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Save Asset Check Config", "Save Config Success", "OK");
            }
        }
    }

    private void CheckSerializedObjectData()
    {
        if (serializedObject == null || CurrentProjectAsset != LastCurrentProjectAsset)
        {
            serializedObject = new SerializedObject(this);
            EnableAssetCheck = CurrentProjectAsset.EnableAssetCheck;
            EnableModelsConfig = CurrentProjectAsset.EnableModelsConfig;
            EnableMaterialConfig = CurrentProjectAsset.EnableMaterialConfig;
            EnableLODPrefabCreate = CurrentProjectAsset.EnableLODPrefabCreate;
            AssetRootPaht = CurrentProjectAsset.AssetRootPaht;

            Extensions_Texture = CurrentProjectAsset.Extensions_Texture;
            Pattern_Mat = CurrentProjectAsset.Pattern_Mat;
            Pattern_Mat_Tag = CurrentProjectAsset.Pattern_Mat_Tag;
            Pattern_FBX = CurrentProjectAsset.Pattern_FBX;
            Pattern_FBX_Tag = CurrentProjectAsset.Pattern_FBX_Tag;
            Pattern_Texture = CurrentProjectAsset.Pattern_Texture;
            Pattern_Texture_Tag = CurrentProjectAsset.Pattern_Texture_Tag;
            Pattern_Prefab = CurrentProjectAsset.Pattern_Prefab;
            Pattern_Prefab_Tag = CurrentProjectAsset.Pattern_Prefab_Tag;
            Pattern_Prefab1 = CurrentProjectAsset.Pattern_Prefab1;
            Pattern_Prefab_Tag1 = CurrentProjectAsset.Pattern_Prefab_Tag1;

            ClipMatPrefix = CurrentProjectAsset.ClipMatPrefix;
            ClipMatSuffix = CurrentProjectAsset.ClipMatSuffix;
            FormatTexturePrefix = CurrentProjectAsset.FormatTexturePrefix;
            AutoGenerateUV2 = CurrentProjectAsset.AutoGenerateUV2;

            PrefabMeshesFolder = CurrentProjectAsset.PrefabMeshesFolder;
            PrefabGenerateFolder = CurrentProjectAsset.PrefabGenerateFolder;
            PrefabColliderLayer = CurrentProjectAsset.PrefabColliderLayer;

            serializedObject.Update();
        }
        else
        {
            serializedObject.Update();
        }
    }

    void CreateNewScriptableObject()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save ScriptableObject", "NewAssetConfig", "asset", "Save Assets Config ScriptableObject");
        if (!string.IsNullOrEmpty(path))
        {
            CurrentProjectAsset = ScriptableObject.CreateInstance<AssetConfigScriptableObject>();
            AssetDatabase.CreateAsset(CurrentProjectAsset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
    void DrawHorizontalLine()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();
    }

}
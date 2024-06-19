using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
#if UNITY_EDITOR

    internal class AssetModesImporterProcessor : AssetPostprocessor
    {
        static string pattern = @"(LOD\d*|collider)\.(FBX|fbx)$";
        static string patternMat = @"^(Mat_)";
        static string patternMatLast = @"(_LOD\d*)$";

        //=========================================================================================//

        [MenuItem("Assets/AssetTool/重置预制件【Regular和Model】Transform", false, 2)]
        private static void PreproceessPrefabTransform()
        {
            // 获取当前选中的所有对象
            Object[] selectedObjects = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.DeepAssets);
            int resetCount = 0;
            // 遍历选中的对象，筛选出预制件

            foreach (Object obj in selectedObjects)
            {
                // 检查对象是否为预制件
                PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(obj);
                if (prefabType == PrefabAssetType.Regular || prefabType == PrefabAssetType.Variant)
                {
                    //GameObject prefab = PrefabUtility.InstantiatePrefab(obj) as GameObject;
                    GameObject prefab = obj as GameObject;
                    if (prefab != null)
                    {
                        resetCount++;
                        prefab.transform.Reset();
                        PrefabUtility.SavePrefabAsset((prefab));
                    }
                }
            }

            Debug.Log("重置选中Transform数量：" + resetCount);
        }

        //=========================================================================================//

        [MenuItem("Assets/AssetTool/修正选中Material", false, 3)]
        private static void PreproceessPrefabMaterial()
        {
            AssetConfigScriptableObject ACConfig = AssetConfigManager.Instance.GetAssetConfigScriptableObject();
            if (ACConfig == null)
                return;
            int resetCount = 0;
            UnityEngine.Material[] materials = Selection.GetFiltered<UnityEngine.Material>(SelectionMode.DeepAssets);
            foreach (var mat in materials)
            {
                if (PostProcessMaterial(mat, ACConfig))
                    resetCount++;
            }
            Debug.Log("修正选中Material数量：" + resetCount);
        }

        static string[] Mat_Text_Map = new string[] { "D", "N", "M", "E" };



        private void OnPostprocessMaterial(Material material)
        {
            AssetConfigScriptableObject ACConfig = AssetConfigManager.Instance.GetAssetConfigScriptableObject();
            if (ACConfig == null || !ACConfig.EnableMaterialConfig)
                return;
            if (!assetPath.StartsWith(ACConfig.AssetRootPaht))
            {
                return;
            }
            PostProcessMaterial(material, ACConfig);
        }

        private static bool PostProcessMaterial(Material material, AssetConfigScriptableObject ACConfig = null)
        {
            if (material == null)
                return false;
            if (ACConfig == null)
            {
                ACConfig = AssetConfigManager.Instance.GetAssetConfigScriptableObject();
                if (ACConfig == null)
                    return false;
            }
            if (Regex.IsMatch(material.name, ACConfig.patternNameMat, RegexOptions.IgnoreCase))
            {
                Shader shader = Shader.Find("Meta48/Scene/SceneBase");
                if (shader == null)
                    return false;
                string MatName = material.name.Split(ACConfig.ClipMatPrefix)[1];
                if (Regex.IsMatch(MatName, patternMatLast, RegexOptions.IgnoreCase))
                {
                    MatName = MatName.Substring(0, MatName.LastIndexOf(ACConfig.ClipMatSuffix));
                }
                material.shader = shader;
                material.SetColor("_BaseColor", new Color(1, 1, 1, 1));
                material.SetFloat("_Metallic", 1f);
                material.SetFloat("_Smoothness", 1f);
                material.SetFloat("_OcclusionStrength", 1f);
                material.SetColor("_EmissionColor", new Color(0, 0, 0, 1));

                //material.SetFloat("_IsAlpaClip", 0);//??????????????????????
                material.SetFloat("_Cutoff", 0.5f);
                material.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Back);
                foreach (string item in Mat_Text_Map)
                {
                    string texName = string.Format(ACConfig.FormatTexturePrefix + "{0}_{1}", MatName, item);
                    string[] assets = AssetDatabase.FindAssets(texName);
                    if (assets.Length <= 0)
                        continue;
                    Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(assets[0]));
                    if (tex == null)
                        continue;
                    switch (item)
                    {
                        case "D":
                            material.SetTexture("_BaseMap", tex);
                            break;
                        case "N":
                            material.SetTexture("_BumpMap", tex);
                            break;
                        case "M":
                            material.SetTexture("_ParamMap", tex);
                            break;
                        case "E":
                            material.SetTexture("_EmissionMap", tex);
                            material.SetColor("_EmissionColor", new Color(1, 1, 1, 1) * 2);
                            break;
                    }
                }
                return true;
            }
            return false;
        }

        //=========================================================================================//

        [MenuItem("Assets/AssetTool/修正选中模型设置", false, 3)]
        private static void PreprocessModel()
        {
            AssetConfigScriptableObject ACConfig = AssetConfigManager.Instance.GetAssetConfigScriptableObject();
            if (ACConfig == null)
                return;
            int resetCount = 0;
            UnityEngine.Object[] objs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
            foreach (var obj in objs)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                if (UpdateModesSetting(assetPath, ACConfig))
                    resetCount++;
            }
            Debug.Log("修正选中模型设置数量：" + resetCount);
        }

        // This method is called before importing a model
        private void OnPreprocessModel()
        {
            AssetConfigScriptableObject ACConfig = AssetConfigManager.Instance.GetAssetConfigScriptableObject();
            if (ACConfig == null || !ACConfig.EnableModelsConfig)
                return;
            UpdateModesSetting(assetPath, ACConfig);
        }

        private static bool UpdateModesSetting(string assetPath, AssetConfigScriptableObject ACConfig = null)
        {
            if (ACConfig == null)
            {
                ACConfig = AssetConfigManager.Instance.GetAssetConfigScriptableObject();
                if (ACConfig == null)
                    return false;
            }
            if (!assetPath.StartsWith(ACConfig.AssetRootPaht))
            {
                return false;
            }
            if (Regex.IsMatch(assetPath, pattern, RegexOptions.IgnoreCase))
            {
                ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                if (importer != null)
                {
                    bool isCollider = assetPath.Contains("collider") || assetPath.Contains("Collider");
                    importer.useFileScale = true;
                    importer.globalScale = 1;
                    importer.useFileUnits = true;
                    importer.bakeAxisConversion = false;
                    importer.importBlendShapes = false;
                    importer.importVisibility = true;
                    importer.importCameras = false;
                    importer.importLights = false;
                    importer.preserveHierarchy = false;
                    importer.sortHierarchyByName = true;
                    importer.meshCompression = isCollider ? ModelImporterMeshCompression.Medium : ModelImporterMeshCompression.Off;
                    importer.isReadable = isCollider;
                    importer.meshOptimizationFlags = MeshOptimizationFlags.Everything;
                    importer.addCollider = false;
                    importer.keepQuads = false;
                    importer.weldVertices = false;
                    importer.indexFormat = ModelImporterIndexFormat.Auto;
                    importer.importBlendShapes = false;
                    importer.importBlendShapeNormals = ModelImporterNormals.Import;
                    importer.normalCalculationMode = ModelImporterNormalCalculationMode.AreaAndAngleWeighted;
                    importer.normalSmoothingSource = ModelImporterNormalSmoothingSource.PreferSmoothingGroups;
                    importer.normalSmoothingAngle = 60;
                    importer.importTangents = ModelImporterTangents.CalculateMikk;
                    importer.swapUVChannels = false;
                    importer.generateSecondaryUV = assetPath.Contains(ACConfig.AutoGenerateUV2);
                    importer.strictVertexDataChecks = false;

                    if (isCollider)
                    {
                        importer.materialImportMode = ModelImporterMaterialImportMode.None; //这个不设了，不然会多很多额外的材质，保证和美术工程一致
                    }
                    else
                    {
                        importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;//模型方保证一定有材质 所以统一设置
                    }
                    importer.useSRGBMaterialColor = true;
                    importer.materialSearch = ModelImporterMaterialSearch.Everywhere;
                    importer.materialLocation = ModelImporterMaterialLocation.External;
                    importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
                    // 保存设置
                    importer.SaveAndReimport();
                    return true;
                }
            }
            return false;
        }
    }
#endif
}

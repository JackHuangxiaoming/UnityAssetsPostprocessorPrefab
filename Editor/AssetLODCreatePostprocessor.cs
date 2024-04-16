using UnityEngine;
using UnityEditor;
using System.IO;
using NUnit.Framework;
using System.Collections.Generic;
using System;
namespace Assets.EDITOR
{

    public class AssetLODCreatePostprocessor : AssetPostprocessor
    {
        // This method is called before importing a model
        private void OnPreprocessModel()
        {
            ModelImporter importer = assetImporter as ModelImporter;
            if (importer != null)
            {
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                importer.materialSearch = ModelImporterMaterialSearch.Everywhere;
                importer.materialLocation = ModelImporterMaterialLocation.External;
                importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
                // 保存设置
                importer.SaveAndReimport();
            }
        }
        private static void CheckAllAssets(string name, List<string> assetsPaths)
        {
            //查找 是否有相同名字的资源            
            string[] assets = AssetDatabase.FindAssets(name);
            GameObject prefab;
            GameObject prefabLOD = null;
            GameObject prefabCollider = null;
            Meta48Environment meta48_Collider = null;
            Meta48Environment meta48_LOD = null;
            LODGroup meta48_LODGroup = null;

            List<GameObject> destroyOBJ = new List<GameObject>();

            string prefabName = Path.GetFileNameWithoutExtension(assetsPaths[0]);
            int index = prefabName.LastIndexOf("_");
            if (index == -1)
                return;
            prefabName = prefabName.Substring(0, index);

            string savePrefabPath = assetsPaths[0].Replace("Meshes/UGC_Tudi_01", "Prefabs/UGC_Tudi_01");
            savePrefabPath = savePrefabPath.Replace("/" + Path.GetFileName(savePrefabPath), "");
            if (!Directory.Exists(savePrefabPath))
                Directory.CreateDirectory(savePrefabPath);

            //处理完整预制件
            if (assets.Length == 0)
            {
                // Create a prefab from the imported asset
                prefab = new GameObject(prefabName);
                destroyOBJ.Add(prefab);
                //Collider
                prefabCollider = new GameObject(name + "_collider");
                destroyOBJ.Add(prefabCollider);
                meta48_Collider = prefabCollider.AddComponent<Meta48Environment>();
                meta48_Collider.m_environmenAreaType = emEnvironmentAreaType.OUTDOOR;
                meta48_Collider.m_environmenType = emEnvironmentType.Physics;
                //LOD
                prefabLOD = new GameObject(name + "_LOD");
                destroyOBJ.Add(prefabLOD);
                meta48_LOD = prefabLOD.AddComponent<Meta48Environment>();
                meta48_LOD.m_environmenAreaType = emEnvironmentAreaType.OUTDOOR;
                meta48_LOD.m_environmenType = emEnvironmentType.Building;
                meta48_LODGroup = prefabLOD.AddComponent<LODGroup>();

                prefabLOD.transform.SetParent(prefab.transform);
                prefabCollider.transform.SetParent(prefab.transform);
            }
            else
            {
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assets[0]);
                prefab.transform.Reset();
                //destroyOBJ.Add(prefab);
                Meta48Environment[] meta48s = prefab.GetComponentsInChildren<Meta48Environment>();
                foreach (var meta48 in meta48s)
                {
                    if (meta48.name.Contains("LOD"))
                    {
                        meta48_LOD = meta48;
                        meta48_LODGroup = meta48.GetComponent<LODGroup>();
                    }
                    else
                        meta48_Collider = meta48;
                }
            }
            PrefabUtility.SaveAsPrefabAsset(prefab, savePrefabPath + "/" + prefabName + ".prefab");
            if (prefab == null || meta48_LOD == null || meta48_Collider == null)
                return;

            List<GameObject> LODGroupOBJ = new List<GameObject>();
            List<GameObject> ColliderGroupOBJ = new List<GameObject>();
            foreach (string path in assetsPaths)
            {
                string t_name = Path.GetFileNameWithoutExtension(path);
                string s_path = savePrefabPath + "/" + t_name + ".prefab";


                bool isLOD = path.Contains("LOD");
                if (isLOD)
                {
                    GameObject t_prefab = new GameObject(t_name);
                    destroyOBJ.Add(t_prefab);
                    GameObject t_prefabSub = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    t_prefabSub.transform.Reset();
                    //destroyOBJ.Add(t_prefabSub);
                    GameObject t_prefabSubInstance = GameObject.Instantiate(t_prefabSub);
                    t_prefabSubInstance.transform.Reset();
                    //destroyOBJ.Add(t_prefabSubInstance);
                    MeshRenderer[] t_renderer = t_prefabSubInstance.GetComponents<MeshRenderer>();
                    foreach (Renderer item in t_renderer)
                        item.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                    t_prefabSubInstance.transform.SetParent(t_prefab.transform);
                    PrefabUtility.SaveAsPrefabAsset(t_prefab, s_path);
                }
                else
                {
                    GameObject t_prefabSub = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    t_prefabSub.transform.Reset();
                    //destroyOBJ.Add(t_prefabSub);
                    GameObject t_prefab = GameObject.Instantiate(t_prefabSub);
                    t_prefab.transform.Reset();
                    destroyOBJ.Add(t_prefab);
                    var t_mr = t_prefab.GetComponent<MeshRenderer>();
                    if (t_mr != null)
                        GameObject.DestroyImmediate(t_mr);
                    var t_mesh = t_prefab.AddComponent<MeshCollider>();
                    //t_mesh.sharedMesh = t_prefabSub.GetComponent<MeshFilter>().sharedMesh;
                    t_prefab.layer = LayerMask.NameToLayer("Physics_World_Default");
                    PrefabUtility.SaveAsPrefabAsset(t_prefab, s_path);                    
                }

                GameObject ro_prefab = AssetDatabase.LoadAssetAtPath<GameObject>(s_path);
                ro_prefab.transform.Reset();
                //destroyOBJ.Add(ro_prefab);
                GameObject r_prefab = null;
                if (isLOD)
                {
                    r_prefab = PrefabUtility.InstantiatePrefab(ro_prefab, meta48_LOD.transform) as GameObject;
                    r_prefab.transform.Reset();
                    //destroyOBJ.Add(r_prefab);
                    //r_prefab.transform.SetParent(meta48_LOD.transform);
                    LODGroupOBJ.Add(r_prefab);
                }
                else
                {
                    r_prefab = PrefabUtility.InstantiatePrefab(ro_prefab, meta48_Collider.transform) as GameObject;
                    r_prefab.transform.Reset();
                    //destroyOBJ.Add(r_prefab);
                    //r_prefab.transform.SetParent(meta48_Collider.transform);
                    ColliderGroupOBJ.Add(r_prefab);
                }
                PrefabUtility.ApplyPrefabInstance(r_prefab, InteractionMode.AutomatedAction);
            }
            LODGroupOBJ.Sort((x, y) => { return x.name.CompareTo(y.name); });
            meta48_LOD.m_lodGroup = LODGroupOBJ.ToArray();
            meta48_Collider.m_lodGroup = ColliderGroupOBJ.ToArray();
            if (LODGroupOBJ.Count > 0 && meta48_LODGroup != null)
            {
                LOD[] lods = new LOD[LODGroupOBJ.Count];
                for (int i = 0; i < lods.Length; i++)
                {
                    switch (i)
                    {
                        case 0:
                            lods[i] = new LOD(0.7f, null);
                            break;
                        case 1:
                            lods[i] = new LOD(0.6f, null);
                            break;
                        case 2:
                            lods[i] = new LOD(0, null);
                            break;
                    }
                }
                meta48_LODGroup.SetLODs(lods);
                meta48_LODGroup.RecalculateBounds();
            }

            PrefabUtility.SaveAsPrefabAsset(prefab, savePrefabPath + "/" + prefabName + ".prefab");


            foreach (var item in destroyOBJ)
            {
                GameObject.DestroyImmediate(item);
            }
        }
        private static void CheckOnce(string assetPath)
        {
            string extension = Path.GetExtension(assetPath).ToLower();
            if (extension != ".fbx")
                return;

            string savePrefabPath = CheckPrefabDir(assetPath);
            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            string s_path = savePrefabPath + "/" + assetName + ".prefab";

            //Filter Name
            //int index_ = assetName.LastIndexOf("_");
            //if (index_ < 0)
            //    return;
            //assetName = assetName.Substring(0, index_);
            //string assetNameP = assetName.Replace("Sm_", "P_");



            // Create a prefab from the imported asset
            GameObject prefab = new GameObject(assetName);
            PrefabUtility.SaveAsPrefabAsset(prefab, s_path);

            string[] asstePaths = AssetDatabase.FindAssets(assetName, new string[1] { savePrefabPath });
            if (asstePaths.Length != 0)
                return;

            GameObject I_Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(s_path);

            bool isLOD = assetName.Contains("LOD");
            if (isLOD)
            {
                MeshRenderer[] t_renderer = prefab.GetComponents<MeshRenderer>();
                foreach (Renderer item in t_renderer)
                {
                    item.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                }
            }
            else
            {
                var t_mr = I_Prefab.GetComponent<MeshRenderer>();
                if (t_mr != null)
                    GameObject.DestroyImmediate(t_mr);
                var t_mesh = I_Prefab.AddComponent<MeshCollider>();
                //t_mesh.sharedMesh = t_prefabSub.GetComponent<MeshFilter>().sharedMesh;
            }


            PrefabUtility.SaveAsPrefabAsset(prefab, s_path);
        }

        private static string CheckPrefabDir(string path)
        {
            string savePrefabPath = path.Replace("Meshes/UGC_Tudi_01", "Prefabs/UGC_Tudi_01");
            savePrefabPath = savePrefabPath.Replace("/" + Path.GetFileName(savePrefabPath), "");
            if (!Directory.Exists(savePrefabPath))
                Directory.CreateDirectory(savePrefabPath);
            return savePrefabPath;
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            Dictionary<string, List<string>> resDic = new Dictionary<string, List<string>>();
            foreach (string assetPath in importedAssets)
            {
                if (!assetPath.Contains(".FBX"))
                    continue;
                string assetsName = Path.GetFileNameWithoutExtension(assetPath);
                int index_ = assetsName.LastIndexOf("_");
                if (index_ < 0)
                    continue;
                assetsName = assetsName.Substring(0, index_);
                assetsName = assetsName.Replace("Sm_", "P_");
                if (!resDic.ContainsKey(assetsName))
                    resDic[assetsName] = new List<string>();
                resDic[assetsName].Add(assetPath);
            }

            foreach (var res in resDic)
            {
                CheckAllAssets(res.Key, res.Value);
            }
        }

        // This method is called after importing any assets
        private void OnPostprocessAssetbundleNameChanged(string assetPath, string previousAssetBundleName, string newAssetBundleName)
        {
            // Ensure that only folders are processed
            if (!assetImporter.importSettingsMissing && Path.GetExtension(assetPath) != "")
                return;

            // Ensure that only specific file types are processed (e.g., textures, models, etc.)
            // You can customize this check based on your requirements
            string extension = Path.GetExtension(assetPath).ToLower();
            if (extension != ".png" && extension != ".jpg" && extension != ".fbx" && extension != ".prefab")
                return;

            // Get the name of the asset without the extension
            string assetName = Path.GetFileNameWithoutExtension(assetPath);

            // Check if there's a prefab with the same name
            string prefabPath = "Assets/Prefabs/" + assetName + ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            // If prefab exists, add the imported asset as a child
            if (prefab != null)
            {
                GameObject assetObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                GameObject assetInstance = PrefabUtility.InstantiatePrefab(assetObject) as GameObject;
                assetInstance.transform.parent = prefab.transform;
                PrefabUtility.SavePrefabAsset(prefab);
                GameObject.DestroyImmediate(assetInstance);
            }
        }
    }
}

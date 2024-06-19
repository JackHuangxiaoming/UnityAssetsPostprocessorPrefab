using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using NUnit.Framework;
namespace Assets.EDITOR
{
#if UNITY_EDITOR
    [DefaultExecutionOrder(999999999)]
    public class AssetLODPrefabProcessor : AssetPostprocessor
    {

        private static void CheckAllAssets(string name, List<string> assetsPaths, AssetConfigScriptableObject ACConfig)
        {
            GameObject prefab;
            GameObject prefabLOD = null;
            GameObject prefabCollider = null;
            Meta48Environment meta48_Collider = null;
            Meta48Environment meta48_LOD = null;
            LODGroup meta48_LODGroup = null;

            List<GameObject> destroyOBJ = new List<GameObject>();

            #region 自动向上查找Meshs 然后同级 同路径创建预制件 否则在当前目录下创建
            string prefabName = Path.GetFileNameWithoutExtension(assetsPaths[0]);
            int index = prefabName.LastIndexOf("_");
            if (index == -1)
                return;
            prefabName = prefabName.Substring(0, index);
            string savePrefabPath = assetsPaths[0].Split(prefabName)[0];
            int lastMeshsI = savePrefabPath.LastIndexOf(ACConfig.PrefabMeshesFolder);
            if (lastMeshsI == -1)
            {
                //表示向上查找没有找到Meshes文件夹 所以在当前目录下创建
                savePrefabPath = savePrefabPath + $"{ACConfig.PrefabGenerateFolder}/";
            }
            else
            {
                string f_s = savePrefabPath.Substring(0, lastMeshsI);
                string l_s = savePrefabPath.Substring(lastMeshsI);
                //向上查找Meshes文件夹 所以在Meshes文件夹同级的Prefab 同路径创建
                savePrefabPath = f_s + l_s.Replace(ACConfig.PrefabMeshesFolder, ACConfig.PrefabGenerateFolder);
            }
            #endregion

            if (!Directory.Exists(savePrefabPath))
                Directory.CreateDirectory(savePrefabPath);

            List<GameObject> LODGroupOBJ = new List<GameObject>();
            List<GameObject> ColliderGroupOBJ = new List<GameObject>();
            //查找 是否有相同名字的资源            
            string[] assets = AssetDatabase.FindAssets(name, new string[] { savePrefabPath });            
            string asset = assets.FirstOrDefault(x => AssetDatabase.GUIDToAssetPath(x).EndsWith($"{name}.prefab"));
            bool isMultipleImport = false;
            #region 处理预制件组
            if (string.IsNullOrEmpty(asset))
            {
                // Create a prefab from the imported asset
                prefab = new GameObject(name);
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
                isMultipleImport = true;
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(asset));
                prefab.transform.Reset();
                prefab = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                destroyOBJ.Add(prefab);
                Meta48Environment[] meta48s = prefab.GetComponentsInChildren<Meta48Environment>();
                foreach (var meta48 in meta48s)
                {
                    if (meta48.name.Contains("LOD"))
                    {
                        meta48_LOD = meta48;
                        meta48_LODGroup = meta48.GetComponent<LODGroup>();
                        LODGroupOBJ.AddRange(meta48_LOD.m_lodGroup.Where(x => { return x != null; }).ToArray());
                    }
                    else if (meta48.name.Contains("collider"))
                    {
                        meta48_Collider = meta48;
                        ColliderGroupOBJ.AddRange(meta48_Collider.m_lodGroup.Where(x => { return x != null; }).ToArray());
                    }
                }
                if (meta48_Collider == null)
                {
                    prefabCollider = new GameObject(name + "_collider");
                    destroyOBJ.Add(prefabCollider);
                    meta48_Collider = prefabCollider.AddComponent<Meta48Environment>();
                    meta48_Collider.m_environmenAreaType = emEnvironmentAreaType.OUTDOOR;
                    meta48_Collider.m_environmenType = emEnvironmentType.Physics;
                    prefabCollider.transform.SetParent(prefab.transform);
                }
            }
            #endregion

            if (prefab == null || meta48_LOD == null || meta48_Collider == null)
                return;

            foreach (string path in assetsPaths)
            {
                string t_name = Path.GetFileNameWithoutExtension(path);
                string s_path = savePrefabPath + "/" + t_name + ".prefab";


                bool isLOD = path.Contains("LOD");
                if (isLOD)
                {
                    GameObject t_prefabSub = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    t_prefabSub.transform.Reset();
                    //destroyOBJ.Add(t_prefabSub);
                    GameObject t_prefabSubInstance = (GameObject)PrefabUtility.InstantiatePrefab(t_prefabSub);
                    t_prefabSubInstance.name = t_prefabSub.name;
                    t_prefabSubInstance.transform.Reset();
                    destroyOBJ.Add(t_prefabSubInstance);
                    MeshRenderer[] t_renderer = t_prefabSubInstance.GetComponentsInChildren<MeshRenderer>();
                    foreach (Renderer item in t_renderer)
                        item.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                    PrefabUtility.SaveAsPrefabAsset(t_prefabSubInstance, s_path);
                }
                else
                {
                    GameObject t_prefabSub = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    t_prefabSub.transform.Reset();
                    //destroyOBJ.Add(t_prefabSub);
                    GameObject t_prefabSubInstance = (GameObject)PrefabUtility.InstantiatePrefab(t_prefabSub);
                    t_prefabSubInstance.name = t_prefabSub.name;
                    t_prefabSubInstance.transform.Reset();
                    destroyOBJ.Add(t_prefabSubInstance);
                    var t_mr = t_prefabSubInstance.GetComponentsInChildren<MeshRenderer>();
                    foreach (Renderer mr_s in t_mr)
                        GameObject.DestroyImmediate(mr_s);
                    var t_meshF = t_prefabSubInstance.GetComponentsInChildren<MeshFilter>();
                    foreach (MeshFilter mf_s in t_meshF)
                    {
                        mf_s.AddComponent<MeshCollider>().sharedMesh = mf_s.sharedMesh;
                        mf_s.gameObject.layer = LayerMask.NameToLayer(ACConfig.PrefabColliderLayer);
                        GameObject.DestroyImmediate(mf_s);
                    }
                    PrefabUtility.SaveAsPrefabAsset(t_prefabSubInstance, s_path);
                }

                GameObject ro_prefab = AssetDatabase.LoadAssetAtPath<GameObject>(s_path);
                ro_prefab.transform.Reset();
                GameObject r_prefab = null;
                if (isLOD)
                {
                    if (LODGroupOBJ.Any(x => { return x.name == ro_prefab.name; }))
                    {
                        var obj = LODGroupOBJ.Find(x => { return x.name == ro_prefab.name; });
                        LODGroupOBJ.Remove(obj);
                        GameObject.DestroyImmediate(obj);
                    }
                    r_prefab = PrefabUtility.InstantiatePrefab(ro_prefab, meta48_LOD.transform) as GameObject;
                    r_prefab.transform.Reset();
                    LODGroupOBJ.Add(r_prefab);
                }
                else
                {
                    if (ColliderGroupOBJ.Any(x => { return x.name == ro_prefab.name; }))
                    {
                        var obj = ColliderGroupOBJ.Find(x => { return x.name == ro_prefab.name; });
                        ColliderGroupOBJ.Remove(obj);
                        GameObject.DestroyImmediate(obj);
                    }
                    r_prefab = PrefabUtility.InstantiatePrefab(ro_prefab, meta48_Collider.transform) as GameObject;
                    r_prefab.transform.Reset();
                    ColliderGroupOBJ.Add(r_prefab);
                }
                if (r_prefab != null)
                {
                    PrefabUtility.ApplyPrefabInstance(r_prefab, InteractionMode.AutomatedAction);
                    destroyOBJ.Add(r_prefab);
                }
            }

            #region Meta48 Environment
            LODGroupOBJ.Sort((x, y) => { return x.name.CompareTo(y.name); });
            meta48_LOD.m_lodGroup = LODGroupOBJ.ToArray();
            if (ColliderGroupOBJ.Count > 0)
                meta48_Collider.m_lodGroup = ColliderGroupOBJ.ToArray();
            else
                meta48_Collider.transform.SetParent(null);
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
            #endregion


            if (isMultipleImport)
                PrefabUtility.ApplyPrefabInstance(prefab, InteractionMode.UserAction);
            else
                PrefabUtility.SaveAsPrefabAsset(prefab, savePrefabPath + "/" + name + ".prefab");

            foreach (var item in destroyOBJ)
            {
                GameObject.DestroyImmediate(item);
            }
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            AssetConfigScriptableObject ACConfig = AssetConfigManager.Instance.GetAssetConfigScriptableObject();
            if (ACConfig == null || !ACConfig.EnableLODPrefabCreate)
                return;
            Dictionary<string, List<string>> resDic = CheckAssetsState(importedAssets, ACConfig);
            foreach (var res in resDic)
            {
                CheckAllAssets(res.Key, res.Value, ACConfig);
            }
            //resDic = CheckAssetsState(deletedAssets);
            //foreach (var res in resDic)
            //{
            //    CheckDeleteAllAssets(res.Key, res.Value);
            //}
        }

        private static Dictionary<string, List<string>> CheckAssetsState(string[] importedAssets, AssetConfigScriptableObject ACConfig)
        {
            Dictionary<string, List<string>> resDic = new Dictionary<string, List<string>>();
            foreach (string assetPath in importedAssets)
            {
                if (string.IsNullOrEmpty(assetPath))
                    continue;
                if (!assetPath.StartsWith(ACConfig.AssetRootPaht))
                    continue;
                if (!Regex.IsMatch(assetPath, "(.FBX|.fbx)", RegexOptions.IgnoreCase))
                    continue;
                string assetsName = Path.GetFileNameWithoutExtension(assetPath);
                int index_ = assetsName.LastIndexOf("_");
                if (index_ < 0)
                    continue;
                assetsName = assetsName.Substring(0, index_);
                if (assetsName.StartsWith("SM_"))
                    assetsName = assetsName.Replace("SM_", "P_");
                else if (assetsName.StartsWith("Sm_"))
                    assetsName = assetsName.Replace("Sm_", "P_");
                else
                {
                    Debug.Log("资源名字不对 SM_ Sm_ 开头不匹配：" + assetsName);
                    continue;
                }
                if (!resDic.ContainsKey(assetsName))
                    resDic[assetsName] = new List<string>();
                resDic[assetsName].Add(assetPath);
            }

            return resDic;
        }

        [MenuItem("Assets/AssetTool/创建选中资源的预制件", false, 4)]
        private static void PostprocessSelectAssets()
        {
            AssetConfigScriptableObject ACConfig = AssetConfigManager.Instance.GetAssetConfigScriptableObject();
            if (ACConfig == null)
                return;
            UnityEngine.Object[] objs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
            List<string> importedAssets = new List<string>(objs.Length);
            foreach (var obj in objs)
            {
                importedAssets.Add(AssetDatabase.GetAssetPath(obj));
            }
            Dictionary<string, List<string>> resDic = CheckAssetsState(importedAssets.ToArray(), ACConfig);
            foreach (var res in resDic)
            {
                CheckAllAssets(res.Key, res.Value, ACConfig);
            }
        }
    }
#endif

}

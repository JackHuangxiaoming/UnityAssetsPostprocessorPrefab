using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Assets.Editor
{
    internal class AssetNameProcessor : AssetPostprocessor
    {
        [MenuItem("Assets/AssetTool/检查资源规范【名字|Transform】", false, 0)]
        private static void PreproceessPrefabTransform()
        {
            UnityEngine.Object[] objs = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets);
            List<string> paths = new List<string>(objs.Length);
            foreach (var obj in objs)
            {
                paths.Add(AssetDatabase.GetAssetPath(obj));
            }
            AssetConfigScriptableObject ACConfig = AssetConfigManager.Instance.GetAssetConfigScriptableObject();
            if (ACConfig == null)
                return;
            CheckPostProcessAssets(paths.ToArray(), ACConfig);
        }
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            AssetConfigScriptableObject ACConfig = AssetConfigManager.Instance.GetAssetConfigScriptableObject();
            if (ACConfig == null || !ACConfig.EnableAssetCheck)
                return;
            CheckPostProcessAssets(importedAssets, ACConfig);
        }

        /// <summary> 
        /// 资源检查
        /// </summary>
        /// <param name="importedAssets"></param>
        private static void CheckPostProcessAssets(string[] importedAssets, AssetConfigScriptableObject ACConfig)
        {
            if (ACConfig == null)
                return;
            List<string> errorRes = new List<string>();
            string fileName;
            string fileNameNoExtension;
            int resCount = 0;
            foreach (string asset in importedAssets)
            {
                if (string.IsNullOrEmpty(asset))
                    continue;
                if (!asset.StartsWith(ACConfig.AssetRootPaht))
                    continue;
                resCount++;
                fileName = Path.GetFileName(asset);
                fileNameNoExtension = Path.GetFileNameWithoutExtension(asset);
                if (Regex.IsMatch(fileName, ACConfig.pattern_Extensions_Mat))
                {
                    if (!Regex.IsMatch(fileNameNoExtension, ACConfig.patternNameMat))
                        errorRes.Add(string.Format("资源名字不符合规范：{0}", asset));
                }
                else if (Regex.IsMatch(fileName, ACConfig.pattern_Extensions_FBX))
                {
                    if (!Regex.IsMatch(fileNameNoExtension, ACConfig.patternNameFBX))
                        errorRes.Add(string.Format("资源名字不符合规范：{0}", asset));
                    GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(asset);
                    if (fbx != null && (fbx.transform.position != Vector3.zero || fbx.transform.rotation != Quaternion.identity || fbx.transform.localScale != Vector3.one))
                        errorRes.Add(string.Format("FBX位置坐标信息不符合规范：{0}", asset));
                }
                else if (Regex.IsMatch(fileName, ACConfig.pattern_Extensions_Texture))
                {
                    if (!Regex.IsMatch(fileNameNoExtension, ACConfig.patternNameTexture))
                        errorRes.Add(string.Format("资源名字不符合规范：{0}", asset));
                }
                else if (Regex.IsMatch(fileName, ACConfig.pattern_Extensions_Prefab))
                {
                    if (!Regex.IsMatch(fileNameNoExtension, ACConfig.patternNamePrefab))
                        errorRes.Add(string.Format("资源名字不符合规范：{0}", asset));
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(asset);
                    if (prefab != null && (prefab.transform.position != Vector3.zero || prefab.transform.rotation != Quaternion.identity || prefab.transform.localScale != Vector3.one))
                        errorRes.Add(string.Format("Prefab位置坐标信息不符合规范：{0}", asset));
                }
            }
            if (resCount > 0)
            {
                Debug.Log($"<color=red>资源检查完毕</color> 总数量：{resCount} 不合格条目：<color=red> {errorRes.Count} </color>");
                foreach (var item in errorRes)
                {
                    Debug.Log($"<color=red>{item}</color>");
                }
            }
        }
    }
}
#endif

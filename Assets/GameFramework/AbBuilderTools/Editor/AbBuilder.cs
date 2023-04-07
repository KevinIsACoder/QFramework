using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
public static class AbBuilder
{
    static string prefabPath = "Assets/GameAssets/Prefabs";
    static string outPath = "Assets/StreamingAssets";
    static string materialPath = "Assets/GameAssets/Atlas";
    static List<AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>();
    public static void BuildAb()
    {
        assetBundleBuilds.Clear();
        var materialBuild = GetAssetBuild(materialPath);
        var prefabBuild = GetAssetBuild(prefabPath);
        assetBundleBuilds.Add(materialBuild);
        assetBundleBuilds.Add(prefabBuild);
        BuildPipeline.BuildAssetBundles(outPath, assetBundleBuilds.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);
    }

    static AssetBundleBuild GetAssetBuild(string path)
    {
        var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
        var astNames = new string[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            if(files[i].EndsWith(".meta")) 
                continue;
            astNames[i] = files[i].Replace("\\", "/");
            Debug.Log("Asset Name Is++++++++++++ " + astNames[i]);
        }
        var index = path.LastIndexOf("/");
        var assetbuild = new AssetBundleBuild()
        {
            assetBundleName = path.Substring(index + 1),
            assetNames = astNames
        };
        // Debug.Log("AssetBundle Name {0}" + assetbuild.assetBundleName);
        // for (int i = 0; i < assetbuild.assetNames.Length; i++)
        // {
        //     Debug.Log("Asset Name Is++++++++++++ " + assetbuild.assetNames[i]);
        // }
        return assetbuild;
    }
}

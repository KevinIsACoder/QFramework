using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
public static class AbBuilder
{
    static string path = "Assets/Image";
    static string outPath = "Assets/StreamingAssets";
    static string prefabPath = "Assets/Prefabs";
    static List<AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>();
    public static void BuildAb()
    {
        var assetbuild = GetAssetBuild(path);
        var prefabBuild = GetAssetBuild(prefabPath);
        assetBundleBuilds.Add(assetbuild);
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
        }
        var index = path.LastIndexOf("/");
        var assetbuild = new AssetBundleBuild()
        {
            assetBundleName = path.Substring(index + 1),
            assetNames = astNames
        };
        return assetbuild;
    }
}

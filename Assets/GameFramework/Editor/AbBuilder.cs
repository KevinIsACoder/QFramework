using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
public static class AbBuilder
{
    static string path_1 = "Assets/Image/Image_1";
    static string path_2 = "Assets/Image/Image_2";
    static string outPath = "Assets/StreamingAssets";
    static string prefabPath = "Assets/Prefabs";
    static List<AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>();
    public static void BuildAb()
    {
        assetBundleBuilds.Clear();
        // var assetbuild = GetAssetBuild(path_1);
        // var assetbuild_2 = GetAssetBuild(path_2);
        var prefabBuild = GetAssetBuild(prefabPath);
        // assetBundleBuilds.Add(assetbuild);
        // assetBundleBuilds.Add(assetbuild_2);
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

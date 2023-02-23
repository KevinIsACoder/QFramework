using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[XLua.LuaCallCSharp]
public static class GameManager {
    private static string[] _sBuildInAssets;
    private static HashSet<string> _sBuildInCommonAssets;

    public static string GameName { private set; get; }
    public static int GameVersion { private set; get; }
    public static int CommonVersion { private set; get; }

    public static void PrepareAssets(string name) {
        GameName = name;
#if !UNITY_EDITOR || USE_ASSETBUNDLE
        if (_sBuildInAssets == null)
        {
            var builtinAssets = Resources.Load<TextAsset>("PkgAssets");
            _sBuildInAssets = builtinAssets == null ? new string[0] : builtinAssets.text.Split('\n');
        }
//#if GAME_DEBUG
        Debug.Log("Build-in assets length:" + _sBuildInAssets.Length);
        foreach (var asset in _sBuildInAssets)
        {
            Debug.Log("Built-in asset:" + asset);
        }
//#endif
        _sBuildInCommonAssets ??= PrepareCommonAssets();
        var assets = PrepareGameAssets(name);
        assets.UnionWith(_sBuildInCommonAssets);
        Spark.Assets.Initialize(assets);
#endif
    }

    private static HashSet<string> PrepareCommonAssets() {
        var assets = new HashSet<string>();
        if (IsBuildInAssetsValidity("Common", out var version)) {
            Debug.Log("Use build-in common assets!");
            foreach (var path in _sBuildInAssets) {
                bool flag;
                if (path.EndsWith(".lua")) {
                    flag = (path.StartsWith("Lua/Game/Common/") || !path.StartsWith("Lua/Game/"));
                } else {
                    flag = (path.StartsWith("Game/Common/") || !path.StartsWith("Game/"));
                }
                if (flag) {
                    assets.Add(path);
                }
            }
        }
        CommonVersion = version;
        Debug.Log("Common Resources Version: " + version);
        return assets;
    }

    private static HashSet<string> PrepareGameAssets(string name) {
        var assets = new HashSet<string>();
        if (IsBuildInAssetsValidity(name, out var version)) {
            Debug.Log($"Use build-in {name} assets!");
            foreach (var path in _sBuildInAssets) {
                if (path.StartsWith($"Game/{name}") || path.StartsWith($"Lua/Game/{name}")) {
                    assets.Add(path);
                }
            }
        }
        GameVersion = version;
        Debug.Log("Game Resources Version: " + version);
        return assets;
    }

    //检查使用内置资源还是使用下载资源.
    private static bool IsBuildInAssetsValidity(string name, out int version)
    {
        var updVerCode = -1;
        var buildInVerCode = -1;
        var gameFile = name + "_Mainfest";

        // 优先获取内置游戏版本，如果不存在就获取外部的
        var buildInVersionFile = Resources.Load<TextAsset>(gameFile);
        Debug.Log($"{gameFile} build-in assets exist:{buildInVersionFile != null}");
        if (buildInVersionFile != null) {
            var buildInVersionData = (Hashtable) Spark.JSON.Parse(buildInVersionFile.text);
            buildInVerCode = int.Parse(buildInVersionData["gameVersion"].ToString());
        } else
        {
        }

        // 获取下载的游戏目录，不存在就获取内部的（必须由原生下载完成后调用该方法）
        var updVersionFile = Spark.FileHelper.ReadString($"{SparkHelper.assetsPath}/{gameFile}");
        Debug.Log($"{gameFile} upd assets exist:{updVersionFile != null}");
        if (updVersionFile != null) {
            var updVersionData = (Hashtable) Spark.JSON.Parse(updVersionFile);
            updVerCode = int.Parse(updVersionData["gameVersion"].ToString());
        } else
        {
        }

        Debug.Log($"{name}=>buildInVerCode:{buildInVerCode}, updVerCode:{updVerCode}");
        version = Mathf.Max(buildInVerCode, updVerCode);
        return buildInVerCode >= updVerCode;
    }

    public static void SetAssetsPath(string path)
    {
        SparkHelper.assetsPath = path;
        SparkLua.rootPath = path + "/Lua";
    }
}
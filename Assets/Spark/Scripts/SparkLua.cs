using System;
using System.Collections.Generic;
using UnityEngine;
using Spark;
using XLua;
using LuaDLL = XLua.LuaDLL.Lua;

public static class SparkLua
{
    public static LuaEnv Env { get; private set; }
    public static LuaTable G => Env.Global;
    public static Func<string, byte[]> loader { get; set; }

    private static List<string> m_PackagePathList = new List<string>();
    private static Dictionary<string, string> m_FullPathList = new Dictionary<string, string>();

    [XLua.CSharpCallLua]
    public delegate LuaTable CreateLuaBehaviourDelegate(string className);
    private static CreateLuaBehaviourDelegate m_CreateLuaBehaviour;
    
    static SparkLua()
    {
#if ENABLE_IL2CPP
		Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.None);
		Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
		Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.None);
		Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
		Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
#endif

        //SparkHelper.onInternalReload += () =>
        //{
        //    if (m_LuaEnv != null)
        //    {
        //        m_FullPathList.Clear();
        //        m_LuaEnv.DoString(@"
        //            local function reload()
        //                local loaded = package.loaded
        //                for k, v in pairs(loaded) do
        //                    loaded[k] = nil
        //                end
        //                for k, v in pairs(__spark_reload_global) do
        //                    rawset(_G, k, nil)
        //                    __spark_reload_global[k] = nil
        //                end
        //             -- collectgarbage()
        //            end
        //            reload()
        //        ", "SparkLuaReload");
        //    }
        //};

        Env = new LuaEnv();
        Env.AddLoader(LoadFile);
        Env.DoString(@"__spark_reload_global = {}", "SparkLuaInit");
        Env.DoString(@"function __create_luabehaviour(name) return require(name)() end", "CreateLuaBehaviour");
        m_CreateLuaBehaviour = Env.Global.Get<CreateLuaBehaviourDelegate>("__create_luabehaviour");

        // 内置middleclass
        G.Set("class", Env.DoString(Resources.Load<TextAsset>("middleclass.lua").bytes, "SparkInitClass")[0]);

#if UNITY_ANDROID
        G.Set("__ANDROID", true);
#endif

#if (UNITY_IOS || UNITY_IPHONE)
	    G.Set("__IOS", true);
#endif

#if UNITY_WEBGL
		G.Set("__WEBGL", true );
#endif

#if UNITY_EDITOR
        G.Set("__EDITOR", true);
#endif
#if USE_ASSETBUNDLE
		G.Set("__USE_ASSETBUNDLE", true);
#endif
        G.Set("__UNITY_VERSION", Application.unityVersion);
    }

    private static string m_RootPath = string.Empty;

    public static string rootPath
    {
        get
        {
            if (string.IsNullOrEmpty(m_RootPath))
            {
#if UNITY_EDITOR
                return Application.dataPath + "/Lua";
#else
				return SparkHelper.assetsPath + "/Lua";
#endif
            }
            return m_RootPath;
        }
        set
        {
            m_RootPath = value;
        }
    }
    
    public static void AddPackagePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return;

        if (!m_PackagePathList.Contains(path))
        {
            m_PackagePathList.Add(path);
        }
    }

    /// <summary>
    /// middleclass
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static LuaTable CreateInstance(LuaTable clazz)
    {
        var oldTop = LuaDLL.lua_gettop(clazz.L);
        LuaDLL.lua_getref(clazz.L, clazz.Ref);
        LuaDLL.lua_pcall(clazz.L, 0, 1, 0);
        var r = LuaDLL.luaL_ref(clazz.L);
        var ret = new LuaTable(r, Env);
        LuaDLL.lua_settop(clazz.L, oldTop);
        return ret;
    }
    
    public static LuaTable CreateLuaBehaviour(string scriptName)
    {
        return m_CreateLuaBehaviour(scriptName);
    }

    private static byte[] LoadBytes(string path)
    {
        var data = Spark.Assets.LoadBytes($"{rootPath}/{path}");
        if (data == null)
        {
            if (loader != null) {
                data = loader(path);
            } else {
                var textAsset = Spark.Assets.LoadAsset<TextAsset>(path + ".bytes");
                if (textAsset != null)
                    data = textAsset.bytes;
            }
        }
        return data;
    }

    private static byte[] LoadFile(ref string fileName)
    {
#if UNITY_EDITOR
        if (fileName == "emmy_core")
        {
            return null;
        }
#endif
        if (fileName.IndexOf(".", StringComparison.Ordinal) >= 0)
        {
            fileName = fileName.Replace('.', '/');
        }
        if (!fileName.EndsWith(".lua"))
        {
            fileName += ".lua";
        }
        byte[] data = null;
        foreach (var path in m_PackagePathList)
        {
            data = LoadBytes(path + "/" + fileName);
            if (data != null)
                return data;
        }
        data = LoadBytes(fileName);
        if (data != null)
        {
            return data;
        }
        throw new UnityException($"Can't find {fileName}");
    }
}


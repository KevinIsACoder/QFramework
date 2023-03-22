using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using System;
using UnityEditor;
using Object = UnityEngine.Object;
using System.IO;
namespace QFramwork
{
    [LuaCallCSharp]
    public static class Assets
    {
        public static Object LoadAssets(string path, Type type)
        {
            Object asset = null;
            if (!string.IsNullOrEmpty(path))
            {
#if UNITY_EDITOR
                asset = AssetDatabase.LoadAssetAtPath(path, type);
#else
#endif
            }
            return asset;
        }
        
        public static T LoadAssets<T>(string path) where T : UnityEngine.Object
        {
            return LoadAssets(path, typeof(T)) as T;
        }

        public static byte[] ReadBytes(string path)
        {
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }
            return null;
        }
    }   
}

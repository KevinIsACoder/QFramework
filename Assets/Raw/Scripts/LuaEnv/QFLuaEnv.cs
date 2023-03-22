using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
namespace QFramwork
{
    public static class QFLuaEnv
    {
        public static LuaEnv luaEnv
        {
            get;
            set;
        }
        
        static QFLuaEnv()
        {
            luaEnv = new LuaEnv();
            AddLoader();
        }
        
        private static string luaPath
        {
            get
            {
#if UNITY_EDITOR
                return "Assets/Lua";
#else
                return "";
#endif
                return "";
            }
        }

        public static void AddLoader()
        {
            luaEnv.AddLoader(LoadLuaFile);
        }

        public static byte[] LoadLuaFile(ref string path)
        {
            var bytes = Assets.ReadBytes($"{luaPath}/{path}");
            Debug.Log("LoadLuaFile+++++++++ " + $"{luaPath}/{path}" + " " + bytes);
            if (bytes == null)
            {
                var textAssets = Assets.LoadAssets<TextAsset>($"{luaPath}/{path}");
                if (textAssets != null)
                {
                    return textAssets.bytes;
                }   
            }
            return bytes;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RealStatePtr = System.IntPtr;
using LuaAPI = XLua.LuaDLL.Lua;

namespace XLua
{
    public partial class ObjectTranslator
    {
        class SparkLua_Gen_Namespace_
        {
            static SparkLua_Gen_Namespace_()
            {
                XLua.LuaEnv.AddIniter(Init);
            }

            static private void Init(LuaEnv luaenv, ObjectTranslator translator)
            {
                translator.GenerateCSNamespaces_(luaenv);
            }
        }

        private SparkLua_Gen_Namespace_ __sparklua_gen_namespace = new SparkLua_Gen_Namespace_();

        public void GenerateCSNamespaces_(LuaEnv env)
        {
            // UnityEngine.Debug.LogWarning("load GenerateCSNamespaces_");
            // var t1 = Spark.TimeHelper.totalMilliseconds;
            HashSet<string> set = new HashSet<string>();
            foreach (var kv in delayWrap)
            {
                var name = kv.Key.FullName;
                if (name.IndexOf("[[") > 0) continue;
                if (name.IndexOf(".") > 0)
                {
                    int idx = -1;
                    do
                    {
                        idx = name.IndexOf('.', idx + 1);
                        if (idx < 0) break;
                        set.Add(name.Substring(0, idx));
                    } while (true);
                }
                set.Add(name);
            }
            // 
            var l = env.L;
            int top = LuaAPI.lua_gettop(l);
            LuaAPI.xlua_getglobal(l, "CS"); // _G.CS
            LuaAPI.xlua_pushasciistring(l, "__fns");
            LuaAPI.lua_createtable(l, 0, set.Count);
            //var table = env.NewTable();
            foreach (var ns in set)
            {
                LuaAPI.xlua_pushasciistring(l, ns);
                LuaAPI.lua_pushboolean(l, true);
                LuaAPI.lua_rawset(l, -3);  // t[ns]=true
            }
            LuaAPI.lua_rawset(l, -3);    // CS.__fns=t
            LuaAPI.lua_pop(l, 1);
            LuaAPI.lua_settop(l, top);
            // var diff = Spark.TimeHelper.totalMilliseconds - t1;
            // UnityEngine.Debug.Log(string.Format("load GenerateCSNamespaces_, cost time: <color=yellow>{0}</color>, {1}", diff, set.Count));
        }
    }

}

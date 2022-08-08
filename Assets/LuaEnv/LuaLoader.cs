/*
 * @Author: zhendong liang
 * @Date: 2022-08-08 20:59:50
 * @LastEditors: Do not edit
 * @LastEditTime: 2022-08-08 21:28:42
 * @Description: 用于加载lua代码
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
public static class LuaLoader
{
    private static LuaEnv env;

    [CSharpCallLua]
    public delegate LuaTable CreateLuaBehaviourDelegate(string scriptName);
    private static CreateLuaBehaviourDelegate m_CreateLuaBehaviour;
    static LuaLoader()
    {
        env = new LuaEnv();
        env.AddLoader(LoadFile);
        env.DoString(@"__spark_reload_global = {}", "SparkLuaInit");
        env.DoString(@"function __create_luabehaviour(name) return require(name)() end", "CreateLuaBehaviour");
        m_CreateLuaBehaviour = env.Global.Get<CreateLuaBehaviourDelegate>("__create_luabehaviour");
    }

    static byte[] LoadFile(ref string fileName)
    {
        fileName = fileName.Replace(".", "/").Replace("\\", "/");
        if(!fileName.EndsWith(".lua"))
            fileName = fileName + ".lua";
        byte[] byteData = null;

        return byteData;
    }

    public static byte[] LoadBytes(string path)
    {
        byte[] byteData = null;

        return byteData;
    }

    static LuaTable CreateLuaBehaviour(string scriptName)
    {
        return m_CreateLuaBehaviour(scriptName);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using QFramwork;
public class Main : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
//         // 配置Bugly参数，启动时在原生端执行的
//         // BuglyAgent.ConfigAutoQuitApplication(false);
//         // BuglyAgent.EnableExceptionHandler();
//
//         SparkHelper.onReload += () => Debug.Log("reload");
//
//         SparkLua.Env.AddBuildin("cmsgpack", XLua.LuaDLL.Lua.LoadMsgPack);
//         SparkLua.Env.AddBuildin("rapidjson", XLua.LuaDLL.Lua.LoadRapidJson);
//
// #if UNITY_EDITOR
//         SparkLua.rootPath = Application.dataPath + "/SparkAssets/Lua";
// #else
// 		SparkLua.rootPath = SparkHelper.assetsPath + "/Lua";
// #endif
//         
//         //SparkLua.AddPackagePath("Game");
//
//         SparkLua.loader = (path) => {
//             var data = Resources.Load<TextAsset>("Lua/" + path);
//             return data != null ? data.bytes : null;
//         };
//
        QFLuaEnv.AddLoader();
        Launch();
    }

    private void Update()
    {
        //SparkLua.Env.Tick();
        QFLuaEnv.luaEnv.Tick();
    }

    void Launch()
    {
#if GAME_DEBUG
        DG.Tweening.DOTween.useSafeMode = false;
        SparkLua.G.Set("__DEBUG", true);
#endif

#if GAME_SOCKET
        SparkLua.G.Set("__SOCKET", true);
#endif

        // __CORE_VERSION
        // 1:
        //  * 初始版本
        // 2:
        //  * PhysicsCustom增加CapsuleCast和SphereCast方法
        //  * 增加SphereCollider导出
        //  * LocalizationManager增加GetCurrentValue方法
        // 3:
        //  * 优化xlua
        //  * LocalizationManager增加SetCurrentLanguage方法
        // 4:
        //  * Update事件里调用xlua的Tick
        //  * Assets的PrefabObject增加OnDestroy逻辑，处理Prefab对象不会自动清除的Bug
        //  * 界面关闭时，自动处理SubViewRoot的清理工作
        // 5:
        //  * GameManager增加gameName、gameVersion、commonVersion字段
        //  * LuaException错误汇报增加资源版本的信息
        // 6:
        //  * 修复gameVersion和commonVersion赋值出错的bug

        // 7:
        //  * 导出了SetDesignResolution和DestroyContainerPool方法
        SparkLua.G.Set("__CORE_VERSION", 7);

        //SparkLua.Env.DoString(Resources.Load<TextAsset>("Scripts/Launcher.lua").text);
        var textAssets = Resources.Load<TextAsset>("Scripts/Launcher.lua");
        Debug.Log(textAssets.text);
        QFLuaEnv.luaEnv.DoString(textAssets.text);
    }
}

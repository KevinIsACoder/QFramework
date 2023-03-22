using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

[LuaCallCSharp]
public class AndroidPlatform : PlatformBase
{
    // public AndroidJavaClass GetJavaClass(string className)
    // {
    //     AndroidJavaClass jc = new AndroidJavaClass(className);
    //     return jc;
    // }
    //
    // public AndroidJavaObject GetJavaStaticObject(AndroidJavaClass jc, string objName)
    // {
    //     return jc.GetStatic<AndroidJavaObject>(objName);
    // }
    //
    // public AndroidJavaObject GetJavaObject(AndroidJavaClass jc, string objName)
    // {
    //     return jc.Get<AndroidJavaObject>(objName);
    // }
    //
    //调用java类静态方法，无返回值
    public void CallVoidApi(string jcName, string funName, params object[] args)
    {
        AndroidJavaClass jc = new AndroidJavaClass(jcName);
        jc.CallStatic(funName, args);
    }

    //调用java类静态方法，无返回值
    public string CallStringApi(string jcName, string funName, params object[] args)
    {
        AndroidJavaClass jc = new AndroidJavaClass(jcName);
        return jc.CallStatic<string>(funName, args);
    }

    //
    //新添加
    // public AndroidJavaObject GetCurrentJavaObject()
    // {
    //     AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    //     return jc.GetStatic<AndroidJavaObject>("currentActivity");
    // }

    // public void CallSdkApi(string apiName, params object[] args)  //没有返回值的Call  
    // {
    //     AndroidJavaObject jo = GetCurrentJavaObject();
    //     jo.Call(apiName, args);
    // }

    /// <summary>
    /// Unity给原生发送消息的接口
    /// </summary>
    /// <param name="msg">Json结构</param>
    /// <returns>Json结构的string</returns>
    public override string SendMessageToNative(string msg)
    {
        AndroidJavaClass jc = new AndroidJavaClass("com.ushowmeida.unitylib.message.UnityMessageReceiver");
        return jc.CallStatic<string>("onMessageReceive", msg);
    }
}
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public static class NativeMsgHelper
{
#if UNITY_ANDROID
    private static AndroidJavaClass jc = new AndroidJavaClass("com.ushowmeida.unitylib.message.UnityMessageReceiver");
#elif UNITY_IOS
        [DllImport("__Internal")]
        private static extern string sendMessageToNative(string msg);
#endif

    public static string SendMsg2Native(string json)
    {
        // {messageName="gameReadyOk", data={}}

#if UNITY_ANDROID
        return jc.CallStatic<string>("onMessageReceive", json);
#elif UNITY_IOS
        return sendMessageToNative(json);
#else
        return null;
#endif
    }

    public static string SendMsg2Native(Hashtable json)
    {
        return SendMsg2Native(Spark.JSON.Stringify(json));
    }

    //发送错误的时候给原生提供报错信息
    //gameType : 游戏类型 gameData : 收到的原生数据 exception : 报错消息
    public static void ReportException(string gameType, string gameData, string exception)
    {
        SendMsg2Native(new Hashtable()
        {
                {"messageName", "reportException"},
                {"data", new Hashtable{
                        {"gameType", gameType},
                        {"gameData", gameData},
                        {"exception", exception}
                }},
        });
    }
}
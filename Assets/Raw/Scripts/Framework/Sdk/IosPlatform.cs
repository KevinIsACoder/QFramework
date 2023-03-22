using System.Runtime.InteropServices;
using XLua;

[LuaCallCSharp]
public class IosPlatform : PlatformBase
{
#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern string sendMessageToNative(string msg);

    /// <summary>
    /// Unity给原生发送消息的接口
    /// </summary>
    /// <param name="msg"></param>
    public override string SendMessageToNative(string msg)
    {
        return sendMessageToNative(msg);
    }

#endif
}
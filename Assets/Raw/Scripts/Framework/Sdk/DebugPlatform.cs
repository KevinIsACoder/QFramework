using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

[LuaCallCSharp]
public class DebugPlatform : PlatformBase
{
    /// <summary>
    /// Unity模拟原生环境发送消息
    /// </summary>
    /// <param name="msg">Json结构</param>
    /// <returns>空字符串</returns>
    public override string SendMessageToNative(string msg)
    {
#if UNITY_EDITOR
        FakeSocket.Get().SendMsgToMessage(msg);
#endif
        return "";
    }
}
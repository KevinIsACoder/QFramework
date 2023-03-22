using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformBase
{
    public virtual string SendMessageToNative(string msg)
    {
        Debug.Log("SendMessageToNative 未实现 msg=" + msg);
        return null;
    }
}
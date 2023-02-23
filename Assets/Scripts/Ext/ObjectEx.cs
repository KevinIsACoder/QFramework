using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ObjectEx
{
    public static void SafeRelease(this Object obj)
    {
        if (obj != null)
        {
#if UNITY_EDITOR
            Object.DestroyImmediate(obj);
#else
            Object.Destroy(obj);
#endif
        }
    }
}

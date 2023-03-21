using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AbBuilderMenu
{
    [MenuItem("AbBuilder/BuildImageAb", false, 1)]
    static void BuildAb()
    {
        var a = 10;
        object b = a;
        b = (int)b + 10;
        Debug.Log(a + " " + b);
        AbBuilder.BuildAb();
    }
}

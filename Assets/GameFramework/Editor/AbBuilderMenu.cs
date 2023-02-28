using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AbBuilderMenu
{
    [MenuItem("AbBuilder/BuildImageAb", false, 1)]
    static void BuildAb()
    {
        AbBuilder.BuildAb();
    }
}

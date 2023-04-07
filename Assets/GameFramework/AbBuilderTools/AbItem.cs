using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class AbItem
{
    [SerializeField] 
    private string bundleName;
    [SerializeField] 
    private string[] assetNames;
    [SerializeField] 
    private string[] dependences;
    [SerializeField] 
    private float bundleSize;
    [SerializeField] 
    private int bundleReference;
}

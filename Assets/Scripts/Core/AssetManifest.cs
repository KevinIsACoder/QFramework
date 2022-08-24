/*
 * @Author: zhendong liang
 * @Date: 2022-08-17 16:12:48
 * @LastEditors: Do not edit
 * @LastEditTime: 2022-08-24 15:51:00
 * @Description: 资源序列化
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace QFramWork
{
    [Serializable]
    public class AssetManifest : ScriptableObject
    {
        [Serializable]
        public struct Assets
        {
            [SerializeField]
            public string assetName;
            [SerializeField]
            public string path;
        }

        [Serializable]
        public class Bundle
        {   
            [SerializeField]
            public string bundleName;
            [SerializeField]
            public Assets[] assets;
            [SerializeField]
            public int dependents;
            [SerializeField]
            public int[] depends;
        }

        [SerializeField]
        public Bundle[] bundles;
        [SerializeField]
        public string[] directories;
    }
}

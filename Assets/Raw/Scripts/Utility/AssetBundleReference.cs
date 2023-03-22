/*
 * @Author: zhendong liang
 * @Date: 2022-08-25 15:30:39
 * @LastEditors: Do not edit
 * @LastEditTime: 2022-08-29 16:37:21
 * @Description: AssetBundle
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
namespace QFramWork
{
    public class AssetBundleReference
    {
        private AssetManifest.Bundle m_Bundle;

        private Queue<System.WeakReference> m_LoadedAssets = new Queue<System.WeakReference>();
        public string bundleName;
        public int refrenceCount
        {
            get;
            private set;
        }

        private AssetBundle m_AssetBundle;
        public AssetBundle AssetBundle
        {
            get
            {
                return m_AssetBundle;
            }

            set
            {
                m_AssetBundle = value;
            }
        }

        private AssetBundleCreateRequest m_AssetBundleRequest;

        private bool isDone
        {
            get
            {
                if (m_AssetBundleRequest != null && m_AssetBundleRequest.isDone)
                {
                    m_AssetBundle = m_AssetBundleRequest.assetBundle;
                }

                if(m_AssetBundle == null)
                {
                    return false;
                }
                return true;
            }
        }

        public AssetBundleReference(AssetManifest.Bundle bundle)
        {
            m_Bundle = bundle;
        }
        //销毁的资源在这里释放
        public void ReleaseObject(Object obj)
        {
            for(int i = 0, count = m_LoadedAssets.Count; i < count; ++i)
            {
                var objRef = m_LoadedAssets.Dequeue();
                if(objRef.Target != obj)
                {
                    m_LoadedAssets.Enqueue(objRef);
                }
            }
        }
        //bundle里加载的资源放在这里
        public void CollectObject(System.WeakReference obj)
        {
            m_LoadedAssets.Enqueue(obj);
        }

        public void Clear()
        {
            m_LoadedAssets.Clear();
        }
    }
}
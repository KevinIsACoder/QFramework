/*
 * @Author: zhendong liang
 * @Date: 2022-08-17 15:30:56
 * @LastEditors: Do not edit
 * @LastEditTime: 2022-08-24 11:23:27
 * @Description: 资源加载管理器
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
namespace QFramWork
{
    [LuaCallCSharp]
    public class AssetsManager : Singleton<AssetsManager>
    {
        public T LoadAssets<T>(string path, System.Type type) where T : Object
        {
            return LoadAssets(path, type) as T;
        }

        public void LoadAssetAsync<T>(string path, System.Type type, System.Action<T> callback) where T : Object
        {
            StartCoroutine(LoadAssetsAsync(path, type, callback));
        }

        public Object LoadAssets(string path, System.Type type)
        {
            Object obj = null;
#if UNITY_EDITOR
            obj = UnityEditor.AssetDatabase.LoadAssetAtPath(path, type);
            if(obj == null)
            {
                obj = Resources.Load(path, type);
            }
#else

#endif
            return obj;
        }

        private IEnumerator LoadAssetsAsync<T>(string path, System.Type type, System.Action<T> callback) where T : Object
        {
            yield return null;
        }

        private static void UnLoadAssets(string ab)
        {
            
        }

        //异步卸载资源
        private static IEnumerator UnLoadUnUsedAssets()
        {
            yield return null;
        }
    }

}
/*
 * @Author: zhendong liang
 * @Date: 2022-08-17 15:30:56
 * @LastEditors: Do not edit
 * @LastEditTime: 2022-08-29 19:04:48
 * @Description: 资源加载管理器
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using XLua;
using System.IO;
namespace QFramWork
{
    [LuaCallCSharp]
    public class AssetsManager : Singleton<AssetsManager>
    {
        private static Dictionary<int, AssetBundleReference> m_ManifestBundles = new Dictionary<int, AssetBundleReference>(); //存储bundle信息
        private static readonly ObjectPool<System.WeakReference> m_objPool = new ObjectPool<System.WeakReference>(() => new System.WeakReference(null), null, (v) => {v.Target = null;});
        private Dictionary<KeyValuePair<string, System.Type>, System.WeakReference> m_AssetReference = new Dictionary<KeyValuePair<string, System.Type>, System.WeakReference>();
        private static string[] m_ManifestAssetDirectories = new string[0];

        [DisallowMultipleComponent]
        public class PrefabObject : MonoBehaviour
        {
            [HideInInspector, SerializeField]
            internal string path = string.Empty;

            private GameObject m_Prefab = null;

            private Dictionary<Object, int> m_prefabInstance = new Dictionary<Object, int>();

            void Awake()
            {
                System.WeakReference objRef = null;
                AssetBundleReference abRef = null;
                
            }

            void OnDestroy()
            {

            }
        }
        
        void Awake()
        {
#if UNITY_EDITOR
        if(!Application.isPlaying)
            return;
#endif
            SceneManager.sceneUnloaded += (scene) => {      //卸载场景的时候释放掉不需要的资源
                StartCoroutine(UnLoadUnUsedAssets());
            }; 
        }
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

        private static string GetAssetName(ref string path, out AssetBundleReference abRef)
        {
            string assetName = string.Empty;
            if(!path.EndsWith(".prefab"))
                path += ".prefab";
            
            KeyValuePair<int, int> kv;
             
            return path;
        }
    }

}
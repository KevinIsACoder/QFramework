using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using WeakReference = System.WeakReference;

namespace Spark
{
	[XLua.LuaCallCSharp]
	public static class Assets
	{
		public sealed class Async<T> : IEnumerator
			where T : Object
		{
			public T asset
			{
				get; internal set;
			}
			public bool isDone
			{
				get; internal set;
			}

			object IEnumerator.Current
			{
				get
				{
					return asset;
				}
			}
			bool IEnumerator.MoveNext()
			{
				return !isDone;
			}
			void IEnumerator.Reset()
			{
				asset = null;
			}
		}

		private static AssetsBehaviour m_AssetsBehaviour;

		static Assets()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			SparkHelper.onInternalReload += () => {
				UnloadAllAssets();
			};

			SceneManager.sceneUnloaded += (scene) => {
				UnloadUnusedAssets();
			};
			
			m_AssetsBehaviour = SparkHelper.CreateModule<AssetsBehaviour>();
		}

		private static string[] m_ManifestAssetDirectories = new string[0];
		private static Dictionary<int, AssetBundleReference> m_ManifestBundles = new Dictionary<int, AssetBundleReference>();
		private static Dictionary<string, KeyValuePair<int, int>> m_ManifestAssets = new Dictionary<string, KeyValuePair<int, int>>();
		// private static Dictionary<string, KeyValuePair<string, bool>> m_StoredAssets = new Dictionary<string, KeyValuePair<string, bool>>();
		private static readonly ObjectPool<WeakReference> m_ObjRefPool = new ObjectPool<WeakReference>(() => new WeakReference(null), null, (v) => v.Target = null);
		private static Dictionary<KeyValuePair<string, System.Type>, WeakReference> m_AssetTypeReferences = new Dictionary<KeyValuePair<string, System.Type>, WeakReference>();

		public static string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();

			stringBuilder.Append("AssetBundle#").Append(m_ManifestBundles.Count).Append("\n");
			int count = 0;
			int aliveCount = 0;
			foreach (var bundle in m_ManifestBundles.Values)
			{
				stringBuilder.Append("\t").Append("#").Append(count).Append(" ");

				var b = bundle.assetBundle;
				if (b == null)
				{
					stringBuilder.Append("NULL");
				}
				else
				{
					aliveCount++;
					stringBuilder.Append(b.name);
				}
				stringBuilder.Append("\n");
				count++;
			}
			stringBuilder.Append("Total AssetBundle ").Append(aliveCount).Append("\n");
			
			stringBuilder.Append("Objects#").Append(m_AssetTypeReferences.Count).Append("\n");
			count = 0;
			aliveCount = 0;
			foreach (var kv in m_AssetTypeReferences)
			{
				stringBuilder.Append("\t").Append("#").Append(count).Append(" ").Append(kv.Key.Key).Append(":").Append(kv.Key.Value).Append(" ");

				var wr = kv.Value;
				if (!IsAlive(wr))
				{
					stringBuilder.Append("NULL");
				}
				else
				{
					aliveCount++;
					stringBuilder.Append(wr.Target);
				}
				stringBuilder.Append("\n");
				count++;
			}
			stringBuilder.Append("Total Objects ").Append(aliveCount).Append("\n");

			return stringBuilder.ToString();
		}

		[DisallowMultipleComponent]
		private class PrefabObject : MonoBehaviour
		{
			[HideInInspector, SerializeField]
			internal string path = string.Empty;

			// 记录Prefab，防止Prefab被回收
			private GameObject m_Prefab = null;
			
			static private Dictionary<Object, int> m_PrefabInstances = new Dictionary<Object, int>();

			void Awake()
			{
				hideFlags = HideFlags.NotEditable;
				AssetBundleReference bundle = null;
				var assetName = GetAssetName(ref path, out bundle);
				if (bundle != null) {
					m_Prefab = TryGetCacheValue(assetName, typeof(GameObject)) as GameObject;
					if (SparkHelper.ObjectValidCheck(m_Prefab)) {
						m_PrefabInstances[m_Prefab] = m_PrefabInstances.ContainsKey(m_Prefab) ? (m_PrefabInstances[m_Prefab] + 1) : 1;
					}
					// 无需再将实例对象保存到bundle内，因为prefab引用一直存在
					// bundle.CollectObject(gameObject);
				}
			}

			private void OnDestroy()
			{
				if (!SparkHelper.ObjectValidCheck(m_Prefab)) return;
				
				var count = m_PrefabInstances[m_Prefab];
				if (count > 1) {
					m_PrefabInstances[m_Prefab] = count - 1;
				} else {
					// 移除引用
                    m_PrefabInstances.Remove(m_Prefab);
					
					// 删除挂载的脚本，以便重复加载时再挂载
                    Object.Destroy(m_Prefab.GetComponent<PrefabObject>());

                    // 清除Bundle上记录的索引，以便bundle可以执行
                    AssetBundleReference bundle = null;
                    string assetName = GetAssetName(ref path, out bundle);
                    if (bundle != null) bundle.ReleaseObject(m_Prefab);

                    // 清除加载过的资源索引
                    WeakReference objRef;
                    var key = new KeyValuePair<string, System.Type>(assetName, typeof(GameObject));
					if (m_AssetTypeReferences.TryGetValue(key, out objRef)) {
                        m_ObjRefPool.Release(objRef);
                        m_AssetTypeReferences.Remove(key);
                    }
#if SPARK_ASSET_DEBUG
					Debug.LogWarningFormat("Assets.DestroyPrefab: {0}, {1}", m_Prefab, path);
#endif
                }
                m_Prefab = null;
			}
		}

		static bool m_IsSparkIndexLoaded = false;
		static HashSet<string> m_BuiltinAssets;

		public static void Initialize(HashSet<string> builtinAssets) {
            // unload all loaded assetbundles & assets
            // UnloadAllAssets();
            // Resources.UnloadUnusedAssets();
            // UnloadUnusedAssets();

            m_BuiltinAssets = builtinAssets;
			if (!m_IsSparkIndexLoaded) {
				// 生成AssetBundle的资源索引结构
				if (IsBuiltin(SparkHelper.SparkAssetIndexName) || File.Exists(GetAssetPath(SparkHelper.SparkAssetIndexName))) {
					var ab = AssetBundle.LoadFromFile(GetAssetPath(SparkHelper.SparkAssetIndexName));
					if (ab != null) {
						var manifest = ab.LoadAsset<AssetsManifest>(SparkHelper.SparkAssetIndexName);
						if (manifest != null) {
							var bundles = manifest.bundles;
							for (int i = 0, count = bundles.Length; i < count; i++) {
								var bundle = bundles[i];
								if (!m_ManifestBundles.ContainsKey(i)) {
									m_ManifestBundles[i] = new AssetBundleReference(bundle);
									foreach (var asset in bundle.assets) {
										m_ManifestAssets[asset.path] = new KeyValuePair<int, int>(asset.index, i);
									}
								}
							}
							m_ManifestAssetDirectories = manifest.directories;
						}
						ab.Unload(false);
					}
				}
				m_IsSparkIndexLoaded = true;
			}
		}

		public static T LoadAsset<T>(string path) where T : Object
		{
			return LoadAsset(path, typeof(T)) as T;
		}

		public static Object LoadAsset(string path, System.Type type)
		{
			AssetBundleReference bundle = null;
			string assetName = GetAssetName(ref path, out bundle);
			Object obj = TryGetCacheValue(assetName, type);
			if (obj == null) {
#if !UNITY_EDITOR || USE_ASSETBUNDLE
				if (bundle != null) {
					obj = bundle.LoadAsset(path, assetName, type);
				} else
#endif
				{
					// Load From Resources/SparkAssets
					int index = path.LastIndexOf("/Resources/");
					if (index > 0) {
						path = path.Substring(index + 11);
					}
					obj = Resources.Load(Path.ChangeExtension(path, null), type);
#if UNITY_EDITOR && !USE_ASSETBUNDLE
					if (obj == null)
						obj = SparkEditor.EditorAssets.LoadAsset(path, type);
#endif
#if SPARK_ASSET_DEBUG
					if (obj != null)
						Debug.LogWarningFormat("Assets.LoadAsset ({0}) from Resources.", assetName);
#endif
				}
				TrySetCacheValue(assetName, type, obj);
			}
			return obj;
		}

		public static Async<T> LoadAssetAsync<T>(string path) where T : Object
		{
			Async<T> async = new Async<T>();
			LoadAssetAsync<T>(path, (obj) => {
				async.asset = obj;
				async.isDone = true;
			});
			return async;
		}
		public static void LoadAssetAsync<T>(string path, System.Action<T> callback) where T : Object
		{
			m_AssetsBehaviour.StartCoroutine(LoadAssetAsync<T>(path, typeof(T), callback));
		}
		[XLua.BlackList]
		public static Async<Object> LoadAssetAsync(string path, System.Type type)
		{
			Async<Object> async = new Async<Object>();
			LoadAssetAsync(path, type, (obj) => {
				async.asset = obj;
				async.isDone = true;
			});
			return async;
		}
		public static void LoadAssetAsync(string path, System.Type type, System.Action<Object> callback)
		{
			m_AssetsBehaviour.StartCoroutine(LoadAssetAsync<Object>(path, type, callback));
		}

		private static IEnumerator LoadAssetAsync<T>(string path, System.Type type, System.Action<T> callback) where T : Object
		{
			for (int i = 0, limit = Random.Range(1, 5); i < limit; i++)
				yield return null; // 等待N帧

			AssetBundleReference bundle = null;
			string assetName = GetAssetName(ref path, out bundle);
			Object obj = TryGetCacheValue(assetName, type);
			if (obj == null) {
#if !UNITY_EDITOR || USE_ASSETBUNDLE
				if (bundle != null) {
					yield return bundle.LoadAssetAsync(path, assetName, type, (asset) => {
						obj = asset;
					});
				} else
#endif
				{
					// Load From Resources/SparkAssets
					int index = path.LastIndexOf("/Resources/");
					if (index > 0) {
						path = path.Substring(index + 11);
					}
					var req = Resources.LoadAsync(Path.ChangeExtension(path, null), type);
					yield return req;
					obj = req.asset;
#if UNITY_EDITOR && !USE_ASSETBUNDLE
					if (obj == null) {
						obj = SparkEditor.EditorAssets.LoadAsset(path, type);
					}
#endif
#if SPARK_ASSET_DEBUG
					if (obj != null)
						Debug.LogWarningFormat("Assets.LoadAsset ({0}) from Resources.", assetName);
#endif
				}
				if (obj != TryGetCacheValue(assetName, type)) {
					TrySetCacheValue(assetName, type, obj);
				}
			}

			if (callback != null) {
				callback((T)obj);
			}
		}

		public static void LoadScene(string path)
		{
#if !UNITY_EDITOR || USE_ASSETBUNDLE
			AssetBundleReference bundle;
			GetAssetName(ref path, out bundle);
			if (bundle != null) {
				bundle.Ref(true);
			}
#endif
		}

		public static IEnumerator LoadSceneAsync(string path)
		{
#if !UNITY_EDITOR || USE_ASSETBUNDLE
			AssetBundleReference bundle;
			GetAssetName(ref path, out bundle);
			if (bundle != null) {
				yield return bundle.RefAsync(true);
			}
#else
			yield return null;
#endif
		}

        public static byte[] LoadBytes(string path)
        {
            if (path.StartsWith(SparkHelper.assetsPath))
            {
				if (m_BuiltinAssets != null && m_BuiltinAssets.Contains(path.Substring(SparkHelper.assetsPath.Length + 1))) {
					return null;
				}
            }
            else if (!File.Exists(path))
            {
                return null;
            }
#if SPARK_ASSET_DEBUG
			Debug.LogFormat("Assets.LoadBytes ({0}).", path);
#endif
			return FileHelper.ReadBytes(path);
        }

        private static bool IsAlive(WeakReference reference)
		{
			var target = reference.Target;
			return target != null && !target.Equals(null);
		}

		private static Object TryGetCacheValue(string assetName, System.Type type)
		{
			WeakReference objRef;
			var key = new KeyValuePair<string, System.Type>(assetName, type);
			if (m_AssetTypeReferences.TryGetValue(key, out objRef)) {
				if (!IsAlive(objRef)) {
					m_ObjRefPool.Release(objRef);
					m_AssetTypeReferences.Remove(key);
				} else {
#if SPARK_ASSET_DEBUG
					Debug.LogWarningFormat("Assets.LoadFromCache ({0}): {1}", assetName, objRef.Target as Object);
#endif
					return objRef.Target as Object;
				}
			}
			return null;
		}
		private static void TrySetCacheValue(string assetName, System.Type type, Object obj)
		{
			if (obj == null) {
#if UNITY_EDITOR
				if (!assetName.EndsWith(".lua.bytes")) {
					Logger.Warning(string.Format("Cannot load resource '{0}'.", assetName));
				}
#endif
			} else {
				var objRef = m_ObjRefPool.Get();
				objRef.Target = obj;
				m_AssetTypeReferences[new KeyValuePair<string, System.Type>(assetName, type)] = objRef;
#if SPARK_ASSET_DEBUG
				Debug.LogWarningFormat("Assets.CacheAsset: {0} => {1}", assetName, obj);
#endif
			}
		}
		public static void UnloadAllAssets()
		{
			m_AssetsBehaviour.StopAllCoroutines();
			foreach (var objRef in m_AssetTypeReferences.Values) {
				m_ObjRefPool.Release(objRef);
			}
			m_AssetTypeReferences.Clear();
			foreach (var entry in m_ManifestBundles) {
				entry.Value.Clear();
			}
		}

		public static void UnloadUnusedAssets()
		{
#if SPARK_ASSET_DEBUG
			Debug.LogWarning("Assets.UnloadUnusedAssets ...");
#endif
			SparkHelper.StartCoroutine(UnloadUnusedAssetsAsync());
		}

		private static IEnumerator UnloadUnusedAssetsAsync()
		{
			yield return new WaitForSeconds(0.3f);
			var asyncOp = Resources.UnloadUnusedAssets();
			yield return asyncOp;

			// Check Loaded Prefabs
			var temp = new Queue<KeyValuePair<string, System.Type>>();
            foreach (var entry in m_AssetTypeReferences)
            {
                var objRef = entry.Value;
                if (!IsAlive(objRef))
                {
                    m_ObjRefPool.Release(objRef);
                    temp.Enqueue(entry.Key);
#if SPARK_ASSET_DEBUG
					Debug.LogWarningFormat("Assets.UnusedAsset: {0}", entry.Key);
#endif
                }
#if SPARK_ASSET_DEBUG
				else {
					Debug.LogWarningFormat("Assets.UsedAsset: {0} => {1}", entry.Key, objRef.Target);
				}
#endif
            }
            while (temp.Count > 0) {
				m_AssetTypeReferences.Remove(temp.Dequeue());
			}

			// Check Loaded AssetBundles
			foreach (var entry in m_ManifestBundles) {
				entry.Value.Unref(true);
			}
#if SPARK_ASSET_DEBUG
			foreach (var entry in m_ManifestBundles) {
				var bundle = entry.Value;
				if (bundle.referenceCount > 0) {
					Debug.LogWarningFormat("Assets.UsedBundle: name={0}, ref={1}, ab={2}", bundle.name, bundle.referenceCount, bundle.assetBundle != null);
				}
			}
#endif
			// 回收
			ObjectCache.RemoveUnusedObjects();
		}

		private static string GetAssetName(ref string path, out AssetBundleReference bundle)
		{
			if (!Path.HasExtension(path)) {
				path = Path.ChangeExtension(path, ".prefab");
			}

			var index = path.LastIndexOf("/SparkAssets/");
			if (index >= 0) {
				path = path.Substring(index + 13);
			}

			KeyValuePair<int, int> kv;
			if (m_ManifestAssets.TryGetValue(path, out kv)) {
				bundle = m_ManifestBundles[kv.Value];
				return kv.Key == 0 ? path : (m_ManifestAssetDirectories[kv.Key - 1] + path);
			}

			bundle = null;
			return path;
		}

		public static bool IsBuiltin(string path) {
			return m_BuiltinAssets != null && m_BuiltinAssets.Contains(path);
		}

		public static string GetAssetPath(string path)
		{
			if (IsBuiltin(path)) {
#if SPARK_ASSET_DEBUG
				Debug.Log("(IsBuiltin)load from:" + SparkHelper.staticAssetsPath + "/" + path);
#endif
				return SparkHelper.staticAssetsPath + "/" + path;
			}
#if SPARK_ASSET_DEBUG
			Debug.Log("load from:" + SparkHelper.assetsPath + "/" + path);
#endif
			return SparkHelper.assetsPath + "/" + path;
		}

		private class AssetBundleReference
		{
			private bool m_Used;
			public string name
			{
				get
				{
					return m_Bundle.name;
				}
			}
			public int referenceCount
			{
				get;
				private set;
			}
			private AssetBundle m_AssetBundle;
			public AssetBundle assetBundle
			{
				get
				{
					return m_AssetBundle;
				}
				private set
				{
					m_AssetBundle = value;
					if (value != null && m_Bundle.name == SparkHelper.SparkUnityShadersName && m_Shaders == null) {
#if SPARK_ASSET_DEBUG
						Debug.LogWarningFormat("Assets.LoadAllShaders: {0}", m_Bundle.name);
#endif
						++referenceCount;
						m_Shaders = m_AssetBundle.LoadAllAssets<Shader>();
					}
				}
			}

			private AssetBundleCreateRequest m_AssetBundleCreateRequest;
			private bool isDone
			{
				get
				{
					if (m_AssetBundleCreateRequest != null && m_AssetBundleCreateRequest.isDone) {
						assetBundle = m_AssetBundleCreateRequest.assetBundle;
						m_AssetBundleCreateRequest = null;
					}
					if (assetBundle == null)
						return false;
					foreach (var index in m_Bundle.depends) {
						if (!m_ManifestBundles[index].isDone)
							return false;
					}
					return true;
				}
			}

			private Shader[] m_Shaders;
			private AssetsManifest.Bundle m_Bundle;
			private Queue<WeakReference> m_LoadedAssets;
			private Dictionary<KeyValuePair<string, System.Type>, AssetBundleRequest> m_Requests;

			public AssetBundleReference(AssetsManifest.Bundle bundle)
			{
				m_Bundle = bundle;
			}

			public IEnumerator LoadAssetAsync(string path, string assetName, System.Type type, System.Action<Object> callback)
			{
				++referenceCount;
				yield return RefAsync(true);
				--referenceCount;

				AssetBundleRequest request;
				KeyValuePair<string, System.Type> key = new KeyValuePair<string, System.Type>(assetName, type);
				Object obj = TryGetCacheValue(assetName, type);
				if (obj == null) {
					if (m_Requests == null) {
						m_Requests = new Dictionary<KeyValuePair<string, System.Type>, AssetBundleRequest>();
					}
					if (!m_Requests.TryGetValue(key, out request)) {
						var t = assetName.EndsWith(".prefab") ? typeof(GameObject) : type;
						request = assetBundle.LoadAssetAsync(assetName, t);
						m_Requests.Add(key, request);
					}
					while ((obj = TryGetCacheValue(assetName, type)) == null && !request.isDone) {
						yield return null;
					}
					m_Requests.Remove(key);
					// check cache again!!!
					if (obj == null) {
						obj = TryGetCacheValue(assetName, type);
						if (obj == null) {
							obj = LoadObjectFromAsset(path, type, request.asset);
							// set cache !!!
							TrySetCacheValue(assetName, type, obj);
						}
					}
				}
				if (obj != null) {
					UnloadUnusedBundle(obj);
				}
				callback(obj);
			}

			public IEnumerator RefAsync(bool used)
			{
				Ref(used, true);
				while (!isDone) {
					yield return null;
				}
			}

			private Object LoadObjectFromAsset(string path, System.Type type, Object asset)
			{
				Object obj = null;
				if (type == typeof(GameObject)) {
					var prefab = asset as GameObject;
					if (prefab != null) {
						prefab.AddComponent<PrefabObject>().path = path;
						obj = prefab;
                    }
				} else if (path.EndsWith(".prefab")) {
					// Hack: Unity5.x不能直接获取脚本对象
					var prefab = asset as GameObject;
					if (prefab != null) {
						obj = prefab.GetComponent(type);
					}
				} else {
					obj = asset;
				}
				if (obj != null) {
					CollectObject(obj);
				}
				UnloadUnusedBundle(obj);
				return obj;
			}

			private void UnloadUnusedBundle(Object obj)
			{
				if ((m_Requests == null || m_Requests.Count == 0)
					&& m_Bundle.assets.Length == 1 && m_Bundle.dependents == 0
					&& m_AssetBundleCreateRequest == null && referenceCount == 1) { // 只有一个资源且不被任何依赖直接unload(false)
					if (obj == null || obj.GetType() != typeof(AudioClip) || ((AudioClip)obj).loadType != AudioClipLoadType.Streaming) { // Streaming类型的AudioClip不能直接unload
						UnloadAssetBundle(false);
					}
				}
			}

			public Object LoadAsset(string path, string assetName, System.Type type)
			{
				Ref(true, false);
				var t = assetName.EndsWith(".prefab") ? typeof(GameObject) : type;
				Object asset = assetBundle.LoadAsset(assetName, t);
				return LoadObjectFromAsset(path, type, asset);
			}

			public void Ref(bool used, bool async = false)
			{
				if (!used || !m_Used) {
					foreach (var index in m_Bundle.depends) {
#if SPARK_ASSET_DEBUG
						AssetBundleReference bundle;
						if (!m_ManifestBundles.TryGetValue(index, out bundle)) {
							throw new UnityException(string.Format("未找到AssetBundle[{0}]的依赖文件[index: {1}]", m_Bundle.name, index));
						}
#endif
						m_ManifestBundles[index].Ref(false, async);
					}
					++referenceCount;
					if (used) {
						m_Used = true;
						if (m_LoadedAssets == null) {
							m_LoadedAssets = new Queue<WeakReference>();
						}
					}
				}

				if (assetBundle == null) {
					if (async) {
						if (m_AssetBundleCreateRequest == null) {
							m_AssetBundleCreateRequest = AssetBundle.LoadFromFileAsync(GetAssetPath(m_Bundle.name));
#if SPARK_ASSET_DEBUG
							Debug.LogWarningFormat("Assets.LoadAssetBundleAsync: {0}", m_Bundle.name);
#endif
							return;
						} else if (m_AssetBundleCreateRequest.isDone) {
							assetBundle = m_AssetBundleCreateRequest.assetBundle;
							m_AssetBundleCreateRequest = null;
						}
					} else {
						if (m_AssetBundleCreateRequest != null) {
							if (m_AssetBundleCreateRequest.isDone) {
								assetBundle = m_AssetBundleCreateRequest.assetBundle;
							} else if (m_AssetBundleCreateRequest.assetBundle != null) {
								m_AssetBundleCreateRequest.assetBundle.Unload(false);
							}
							m_AssetBundleCreateRequest = null;
						}
						if (assetBundle == null) {
							assetBundle = AssetBundle.LoadFromFile(GetAssetPath(m_Bundle.name));
						}
					}
#if SPARK_ASSET_DEBUG
					if (assetBundle != null) {
						Debug.LogWarningFormat("Assets.LoadAssetBundle: {0}", m_Bundle.name);
					} else {
						throw new UnityException(string.Format("Can't Load AssetBundle File. [{0}: {1}]", m_Bundle.name, File.Exists(GetAssetPath(m_Bundle.name))));
					}
#endif
				}
			}

			public void Unref(bool used)
			{
				if (used) {
					if (!m_Used)
						return;
					if (m_Requests != null && m_Requests.Count > 0)
						return;

					// 判定原始资源是否销毁
					var objRefs = m_LoadedAssets;
					for (int i = 0, count = objRefs.Count; i < count; i++) {
						var objRef = objRefs.Dequeue();
						if (IsAlive(objRef)) {
							objRefs.Enqueue(objRef);
						} else {
#if SPARK_ASSET_DEBUG
							Debug.LogWarningFormat("Unload Asset {0}", m_Bundle.name);
#endif
							m_ObjRefPool.Release(objRef);
						}
					}
					if (objRefs.Count > 0)
						return;

					// 判定场景是否存在
					if (m_Bundle.scenes.Length > 0) {
						var count = SceneManager.sceneCount;
						for (int i = 0; i < count; i++) {
							var scene = SceneManager.GetSceneAt(i);
							if (System.Array.IndexOf(m_Bundle.scenes, scene.name) >= 0)
								return;
						}
					}
					m_Used = false;
				}

				if (referenceCount > 0) {
					if (--referenceCount == 0) {
						UnloadAssetBundle(true);
					}
					foreach (var path in m_Bundle.depends) {
						m_ManifestBundles[path].Unref(false);
					}
				}
#if SPARK_ASSET_DEBUG
				else {
					throw new UnityException(string.Format("The reference count of AssetBundle ({0}) must be greater than zero.", m_Bundle.name));
				}
#endif
			}
			public void Clear()
			{
				m_Used = false;
				referenceCount = 0;
				if (m_Requests != null) {
					m_Requests.Clear();
				}
				m_Shaders = null;
				if (m_AssetBundleCreateRequest != null) {
					if (!m_AssetBundleCreateRequest.isDone && m_AssetBundleCreateRequest.assetBundle != null) {
						m_AssetBundleCreateRequest.assetBundle.Unload(false);
					}
					m_AssetBundleCreateRequest = null;
				}
				UnloadAssetBundle(false); // false 是为了在二次更新时不删除之前已加载的文件
				if (m_LoadedAssets != null) {
					while (m_LoadedAssets.Count > 0) {
						m_ObjRefPool.Release(m_LoadedAssets.Dequeue());
					}
				}
			}
			private void UnloadAssetBundle(bool unloadAllLoadedObjects)
			{
				if (assetBundle != null) {
					assetBundle.Unload(unloadAllLoadedObjects);
					assetBundle = null;
#if SPARK_ASSET_DEBUG
					Debug.LogWarningFormat("Assets.UnloadAssetBundle({0}): {1}", unloadAllLoadedObjects, m_Bundle.name);
#endif
				}
			}
			public void CollectObject(Object obj)
			{
				var objRef = m_ObjRefPool.Get();
				objRef.Target = obj;
				m_LoadedAssets.Enqueue(objRef);
			}
			public void ReleaseObject(Object obj)
			{
                for (int i = 0, count = m_LoadedAssets.Count; i < count; i++) {
                    var objRef = m_LoadedAssets.Dequeue();
					if (objRef.Target == obj) {
#if SPARK_ASSET_DEBUG
						Debug.LogWarningFormat("Assets.ReleaseObject: {0}", obj);
#endif
                        m_ObjRefPool.Release(objRef);
                    } else {
                        m_LoadedAssets.Enqueue(objRef);
                    }
                }
            }
		}

		class AssetsBehaviour : MonoBehaviour
		{
#if UNITY_EDITOR
			[UnityEditor.CustomEditor(typeof(AssetsBehaviour), true)]
			class Inspector : UnityEditor.Editor
			{
				public override void OnInspectorGUI()
				{
					foreach (var entry in m_ManifestBundles) {
						var bundle = entry.Value;
						if (bundle.referenceCount > 0) {
							UnityEditor.EditorGUILayout.LabelField(bundle.name);
						}
					}
				}
			}
#endif
		}
	}
}

#if UNITY_EDITOR
namespace SparkEditor
{
	internal class EditorAssets : UnityEditor.AssetPostprocessor
	{
		private const string m_AssetFolderName = "SparkAssets";
		private static List<string> m_AssetFolders = new List<string>();

#if !SPARK_AUTO_BUILD
		[UnityEditor.InitializeOnLoadMethod]
		private static void DetectAssetFolders()
		{
			m_AssetFolders.Clear();
			foreach (var path in Directory.GetDirectories("Assets", m_AssetFolderName, SearchOption.AllDirectories)) {
				m_AssetFolders.Add(path.Replace("\\", "/"));
			}
		}

		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
		{
			if (IsAssetFolderChanged(importedAssets) || IsAssetFolderChanged(deletedAssets) || IsAssetFolderChanged(movedAssets) || IsAssetFolderChanged(movedFromPath)) {
				DetectAssetFolders();
			}
		}

#endif

		private static bool TryGetAssetFolderPath(string path, out string assetPath)
		{
			if (path.EndsWith("/" + m_AssetFolderName) && Directory.Exists(path)) {
				assetPath = path;
				return true;
			}

			int index = path.LastIndexOf("/" + m_AssetFolderName + "/");
			if (index > 0) {
				assetPath = path.Substring(0, index);
				return true;
			}

			assetPath = null;
			return false;
		}

		private static bool IsAssetFolderChanged(string[] assets)
		{
			string assetPath;
			foreach (var p in assets) {
				if (TryGetAssetFolderPath(p, out assetPath)) {
					if (!m_AssetFolders.Exists((path) => path == assetPath)) {
						return true;
					}
				}
			}
			return false;
		}

		public static UnityEngine.Object LoadAsset(string path, System.Type type)
		{
			UnityEngine.Object obj = null;
			if (path.StartsWith("Assets/")) {
				obj = LoadAssetAtPath(path, type);
				if (obj != null)
					return obj;
			}

			foreach (var folder in m_AssetFolders) {
				var assetPath = folder + "/" + path;
				if (File.Exists(assetPath)) {
					obj = LoadAssetAtPath(assetPath, type);
					if (obj != null)
						return obj;
				}
			}

			return null;
		}

		public static UnityEngine.Object LoadAssetAtPath(string path, System.Type type)
		{
#if UNITY_5 || UNITY_5_3_OR_NEWER
			return UnityEditor.AssetDatabase.LoadAssetAtPath(path, type);
#else
            return UnityEngine.Resources.LoadAssetAtPath(path, type);
#endif
		}
	}
}
#endif

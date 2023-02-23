using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Spark
{
	[XLua.LuaCallCSharp]
	public class GameObjectPool : Disposable
	{
		private GameObject m_ObjectContainer;
		private readonly Dictionary<string, Stack<GameObject>> m_AssetObjects;

		public GameObjectPool(string name) : this(name, null, true)
		{
		}

		public GameObjectPool(string name, bool dontDestroyOnLoad) : this(name, null, dontDestroyOnLoad)
		{
		}

		public GameObjectPool(string name, Transform parent, bool dontDestroyOnLoad)
		{
			m_AssetObjects = new Dictionary<string, Stack<GameObject>>();
			m_ObjectContainer = new GameObject("#GOP#" + name);
			m_ObjectContainer.SetActive(false);
			if (parent != null) {
				m_ObjectContainer.transform.SetParent(parent, false);
			}
			if (dontDestroyOnLoad) {
				GameObject.DontDestroyOnLoad(m_ObjectContainer);
			}
		}

		public GameObject BorrowObject(string assetPath)
		{
			return BorrowObject(assetPath, null);
		}

		public GameObject BorrowObject(string assetPath, Action<GameObject> onReset)
		{
			GameObject go = GetObjectFromPool(assetPath);
			if (go == null) {
				go = GameObject.Instantiate<GameObject>(Assets.LoadAsset<GameObject>(assetPath), m_ObjectContainer.transform);
				go.AddComponent<PoolObject>().value = assetPath;
			}
			if (onReset != null)
				onReset(go);

			return go;
		}

		private GameObject GetObjectFromPool(string assetPath)
		{
			Stack<GameObject> objects;
			if (!m_AssetObjects.TryGetValue(assetPath, out objects)) {
				objects = new Stack<GameObject>();
				m_AssetObjects[assetPath] = objects;
			}
			return objects.Count > 0 ? objects.Pop() : null;
		}

		public void BorrowObjectAsync(string assetPath, Action<GameObject> onComplete)
		{
			BorrowObjectAsync(assetPath, null, onComplete);
		}

		public void BorrowObjectAsync(string assetPath, Action<GameObject> onReset, Action<GameObject> onComplete)
		{
			GameObject go = GetObjectFromPool(assetPath);
			if (go == null) {
				Assets.LoadAssetAsync<GameObject>(assetPath, (asset) => {
					if (m_ObjectContainer != null) {
						go = GameObject.Instantiate(asset, m_ObjectContainer.transform);
						go.AddComponent<PoolObject>().value = assetPath;
						if (onReset != null)
							onReset(go);
						if (onComplete != null)
							onComplete(go);
					}
				});
			} else {
				if (onReset != null)
					onReset(go);
				if (onComplete != null)
					onComplete(go);
			}
		}

		public void ReturnObject(GameObject go)
		{
			var comp = go.GetComponent<PoolObject>();
			if (comp != null && comp.value != null) {
				Stack<GameObject> objects;
				if (m_AssetObjects.TryGetValue(comp.value, out objects)) {
					objects.Push(go);
					go.transform.SetParent(m_ObjectContainer.transform, false);
					return;
				}
			}
			GameObject.Destroy(go);
		}

		protected override void DisposeManagedObjects()
		{
			m_AssetObjects.Clear();
			GameObject.Destroy(m_ObjectContainer);
			m_ObjectContainer = null;
		}

		class PoolObject : MonoBehaviour
		{
			public string value;
		}
	}
}

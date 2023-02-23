using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Spark;

namespace SparkEditor
{
	static public class UIAtlasBuilder
	{
		static public void Build<T>(Sprite[] sprites, string savePath, Action<T> onPostBuild = null)
			where T : UIAtlas
		{
			GameObject go = null;
			GameObject oldPrefab = AssetDatabase.LoadAssetAtPath(savePath, typeof(GameObject)) as GameObject;
			if (oldPrefab != null) {
				go = PrefabUtility.InstantiatePrefab(oldPrefab) as GameObject;
			} else {
				go = new GameObject();
				go.name = Path.GetFileNameWithoutExtension(savePath);
			}
			Build<T>(go, sprites, savePath, onPostBuild);
		}

		static public void Build<T>(GameObject go, Sprite[] sprites, string savePath, Action<T> onPostBuild = null)
			where T : UIAtlas
		{
			savePath = Path.ChangeExtension(savePath, "prefab");

			var atlas = go.GetComponent<UIAtlas>();
			if (atlas != null) {
				if (atlas.GetType() != typeof(T)) {
					Component.DestroyImmediate(atlas);
				}
			}

			T t = go.GetComponent<T>();
			if (t == null) {
				t = go.AddComponent<T>();
			}

			Array.Sort(sprites, (a, b) => {
				int c = a.name.CompareTo(b.name);
				if (c == 0) {
					c = a.GetInstanceID().CompareTo(b.GetInstanceID());
				}
				return c;
			});
			t.SetSprites(sprites);

			if (onPostBuild != null)
				onPostBuild(t);

			SparkEditorHelper.CreateOrReplacePrefab(go, savePath);

			GameObject.DestroyImmediate(go);
		}

		//static public T Append<T>(GameObject go, params Sprite[] sprites) where T : UIAtlas
		//{
		//	T atlas = go.GetComponent<T>();
		//	if (atlas == null) {
		//		atlas = go.AddComponent<T>();
		//	}

		//	if (sprites != null && sprites.Length > 0) {
		//		var info = typeof(UIAtlas).GetField("m_SpriteDict", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		//		if (info != null) {
		//			var values = info.GetValue(atlas) as Dictionary<string, Sprite>;
		//			for (int i = 0; i < sprites.Length; i++) {
		//				values.Add(sprites[i].name, sprites[i]);
		//			}
		//		}
		//	}

		//	return atlas;
		//}
	}
}

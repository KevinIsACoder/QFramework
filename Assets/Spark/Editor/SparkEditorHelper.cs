using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace SparkEditor {
	static public class SparkEditorHelper {
		// relative to project folder.
		// like 'Assets/xxx.png'.
		//static public string GetRelativeAssetsPath(string path)
		//{
		//	return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
		//}

		static public string temporaryCachePath {
			get {
				return Application.dataPath + "/../SparkTemp";
			}
		}

		static public string GetSparkAssetIndexCachePath(BuildTarget target, string version) {
			return string.Format("{0}/ABBuilderManifestCache_{1}_{2}", SparkEditorHelper.temporaryCachePath, target, version);
		}

		static public string GetSparkAssetIndexPath() {
			return "Assets/" + SparkHelper.SparkAssetIndexName + ".asset";
		}

		static public string GetAssetRelativePath(string path) {
			path = Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
			if (path.IndexOf("SparkAssets") >= 0 || path.IndexOf("Resources") >= 0) {
				var a = path.Split('/');
				for (int i = a.Length - 2; i >= 0; i--) {
					if (a[i] == "SparkAssets" || a[i] == "Resources") {
						return string.Join("/", a, i + 1, a.Length - i - 1);
					}
				}
			}
			return path;
		}

		static public void CreateOrReplacePrefab(GameObject go, string targetPath) {
			GameObject prefab = AssetDatabase.LoadAssetAtPath(targetPath, typeof(GameObject)) as GameObject;
			if (prefab != null) {
				PrefabUtility.SaveAsPrefabAssetAndConnect(go, targetPath, InteractionMode.AutomatedAction);
				//PrefabUtility.ReplacePrefab(go, prefab, options);
			} else {
				Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
				//PrefabUtility.CreatePrefab(targetPath, go, options);
				PrefabUtility.SaveAsPrefabAssetAndConnect(go, targetPath, InteractionMode.AutomatedAction);
			}
		}

		public static T[] GetAssetsAtPath<T>(string path, bool recursive) where T : UnityEngine.Object {
			List<T> list = new List<T>();
			string[] files = Directory.GetFiles(path, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
			foreach (string file in files) {
				T t = AssetDatabase.LoadAssetAtPath<T>(file);
				if (t != null) {
					list.Add(t);
				}
			}
			return list.ToArray();
		}

		public static void Process(string cmd, string arguments) {
			// compress
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
			startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			startInfo.FileName = cmd;
			startInfo.Arguments = arguments;
			process.StartInfo = startInfo;
			process.Start();
		}

		//static private string GetRelativeAssetsPath(string path)
		//{
		//	bool found = false;
		//	string fileName = "";
		//	List<string> results = new List<string>();
		//	string[] paths = Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Split(Path.DirectorySeparatorChar);
		//	for (int i = paths.Length - 1; i >= 0; i--) {
		//		string name = paths[i];
		//		if (name == "Resources" || name == "SparkAssets") {
		//			found = true;
		//			break;
		//		} else if (fileName == "" && name.IndexOf(".") > 0) {
		//			fileName = name;
		//		}
		//		results.Insert(0, name);
		//	}
		//	return found ? string.Join("/", results.ToArray()) : fileName;
		//}
	}
}
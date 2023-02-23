using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class FindReferencesEditorWindow : EditorWindow {

	/// <summary>
	/// 查找引用
	/// </summary>
	[MenuItem("Tools/Find References")]
	public static void FindReferences() {
		FindReferencesEditorWindow window = (FindReferencesEditorWindow)EditorWindow.GetWindow(typeof(FindReferencesEditorWindow), false, "Find References", true);
		window.Show();
	}
		
	private static Object findObj;
	private List<Object> result = new List<Object>();

	private Vector2 scrollPos = new Vector2();
	private bool checkPrefab = true;
	private bool checkScene = true;
	private bool checkMaterial = true;
	private void OnGUI() {
		EditorGUILayout.BeginVertical();

		EditorGUILayout.BeginHorizontal();
		findObj = EditorGUILayout.ObjectField(findObj, typeof(Object), true);
		if (GUILayout.Button("Find", GUILayout.Width(100))) {
			result.Clear();
			if (findObj == null) {
				return;
			}
			string assetPath = AssetDatabase.GetAssetPath(findObj);
			string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
			
			string filter = "";
			if (checkPrefab) {
				filter += "t:Prefab ";
			}
			if (checkScene) {
				filter += "t:Scene ";
			}
			if (checkMaterial) {
				filter += "t:Material ";
			}
			filter = filter.Trim();
			if (!string.IsNullOrEmpty(filter)) {
				string[] guids = AssetDatabase.FindAssets(filter, new[] { "Assets" });
				int len = guids.Length;
				for (int i = 0; i < len; i++) {
					string filePath = AssetDatabase.GUIDToAssetPath(guids[i]);
					bool cancel = EditorUtility.DisplayCancelableProgressBar("Finding ...", filePath, i * 1.0f / len);
					if (cancel) {
						break;
					}
					// 检查是否包含guid
					try {
						// 某些文件读取会抛出异常
						string content = File.ReadAllText(filePath);
						if(content.Contains(assetGuid)) {
							Object fileObject = AssetDatabase.LoadAssetAtPath(filePath, typeof(Object));
							result.Add(fileObject);
						}
					} catch (System.Exception e) {
						Debug.LogWarning(filePath + "\n" + e.ToString());
					}
				}
				EditorUtility.ClearProgressBar();
			}
		}
		EditorGUILayout.EndHorizontal();
		checkPrefab = EditorGUILayout.Toggle("Check Prefab : ", checkPrefab);
		checkScene = EditorGUILayout.Toggle("Check Scene : ", checkScene);
		checkMaterial = EditorGUILayout.Toggle("Check Material : ", checkMaterial);
		EditorGUILayout.LabelField("Result Count = " + result.Count);

		EditorGUILayout.Space();

		// 显示结果
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
		for (int i = 0; i < result.Count; i++) {
			EditorGUILayout.ObjectField(result[i], typeof(Object), true);
		}
		EditorGUILayout.EndScrollView();

		EditorGUILayout.EndVertical();
	}
}

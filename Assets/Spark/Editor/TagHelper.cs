using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace SparkEditor
{
	public static class TagHelper
	{
		public static void AddTags(params string[] names)
		{
			UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
			if ((asset != null) && (asset.Length > 0)) {
				SerializedObject so = new SerializedObject(asset[0]);
				SerializedProperty tags = so.FindProperty("tags");

				bool update = false;
				foreach (var tag in names) {
					bool found = false;
					for (int i = 0; i < tags.arraySize; ++i) {
						if (tags.GetArrayElementAtIndex(i).stringValue == tag) {
							found = true;
							break;
						}
					}
					if (!found) {
						update = true;
						int index = Math.Max(0, tags.arraySize - 1);
						tags.InsertArrayElementAtIndex(index);
						tags.GetArrayElementAtIndex(index).stringValue = tag;
					}
				}

				if (update) {
					so.ApplyModifiedProperties();
					so.Update();
				}
			}
		}

		public static void RemoveTags(params string[] names)
		{
			UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
			if ((asset != null) && (asset.Length > 0)) {
				SerializedObject so = new SerializedObject(asset[0]);
				SerializedProperty tags = so.FindProperty("tags");

				bool update = false;
				foreach (var tag in names) {
					for (int i = tags.arraySize - 1; i >= 0; --i) {
						if (tags.GetArrayElementAtIndex(i).stringValue == tag) {
							update = true;
							tags.DeleteArrayElementAtIndex(i);
							break;
						}
					}
				}

				if (update) {
					so.ApplyModifiedProperties();
					so.Update();
				}
			}
		}
	}
}

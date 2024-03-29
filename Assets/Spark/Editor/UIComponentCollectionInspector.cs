//----------------------------------------------------
// Spark: A Framework For Unity
// Copyright © 2014 - 2015 Jay Hu (Q:156809986)
//----------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SparkEditor
{
	[CustomEditor(typeof(Spark.UIComponentCollection), true)]
	public class UIComponentCollectionInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			SparkEditorTools.SetLabelWidth(120f);
			GUILayout.Space(3f);

			EditorGUI.BeginDisabledGroup(true);

			SerializedProperty components = serializedObject.FindProperty("components");
			EditorGUILayout.LabelField("count:" + components.arraySize);
			for (int i = 0; i < components.arraySize; i++) {
				GUILayout.Space(2f);
				SerializedProperty item = components.GetArrayElementAtIndex(i);
				EditorGUILayout.ObjectField(item.objectReferenceValue, typeof(Component), true);
			}

			EditorGUI.EndDisabledGroup();

			serializedObject.ApplyModifiedProperties();
		}
	}
}

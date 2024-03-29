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
	[CustomEditor(typeof(Spark.UIImage), true)]
	public class UIImageInspector : UnityEditor.UI.ImageEditor
	{
		public override void OnInspectorGUI()
		{
			SparkEditorTools.SetLabelWidth(120f);
			GUILayout.Space(3f);
			serializedObject.Update();

			EditorGUILayout.BeginHorizontal();
			SerializedProperty sp = SparkEditorTools.DrawProperty("UI Atlas", serializedObject, "m_Atlas");
			Spark.UIAtlas atlas = sp.objectReferenceValue as Spark.UIAtlas;
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			sp = serializedObject.FindProperty("m_SpriteName");
			EditorGUILayout.LabelField("Sprite Name", GUILayout.Width(116f));
			if (GUILayout.Button(string.IsNullOrEmpty(sp.stringValue) ? "Select" : sp.stringValue, "DropDown")) {
				UIAtlasSpritesWindow.Show(atlas, serializedObject, sp);
			}
			EditorGUILayout.EndHorizontal();

			SparkEditorTools.DrawProperty("Force Native Size", serializedObject, "m_ForceNativeSize");

			serializedObject.ApplyModifiedProperties();

			base.OnInspectorGUI();
		}
	}
}

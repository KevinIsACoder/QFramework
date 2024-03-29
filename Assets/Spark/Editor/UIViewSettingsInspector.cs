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
	[CustomEditor(typeof(Spark.UIViewSettings), true)]
	public class UIViewSettingsInspector : ComponentEditor
	{
		protected override bool ShouldDrawProperties()
		{
			return !Application.isPlaying;
		}

		protected override void DrawCustomProperties()
		{
			SerializedProperty serializedProperty = serializedObject.FindProperty("m_Style");
            var sp = DrawRelativeProperty("UI Mode", serializedProperty, "mode");
            if (sp.enumValueIndex ==1 || sp.enumValueIndex == 2)
            {
                EditorGUI.indentLevel++;
                sp = DrawRelativeProperty("Override Color", serializedProperty, "overrideColor");
                if (sp.boolValue)
                {
                    SparkEditorTools.BeginIndent();
                    DrawRelativeProperty("Mask Color", serializedProperty, "maskColor");
                    SparkEditorTools.EndIndent();
                }
                EditorGUI.indentLevel--;
            }

            DrawRelativeProperty("Is Topmost", serializedProperty, "topmost");
            
            sp = SparkEditorTools.DrawProperty("Override Mode", serializedObject, "overrideMode");
			if (sp.boolValue) {
				SparkEditorTools.BeginIndent();
				sp = SparkEditorTools.DrawProperty("Invisible Mode", serializedObject, "invisibleMode");
				if (sp.enumValueIndex == 1) {
					SparkEditorTools.BeginIndent();
					sp = serializedObject.FindProperty("invisibleLayer");
					sp.intValue = EditorGUILayout.LayerField("Layer", sp.intValue);
					SparkEditorTools.EndIndent();
				}
				SparkEditorTools.EndIndent();
			}
		}

		SerializedProperty DrawRelativeProperty(string label, SerializedProperty serializedProperty, string propertyName)
		{
			SerializedProperty property = serializedProperty.FindPropertyRelative(propertyName);
			if (property != null) {
				EditorGUI.BeginChangeCheck();
				SparkEditorTools.DrawProperty(label, property);
				if (EditorGUI.EndChangeCheck())
					serializedObject.ApplyModifiedProperties();
			}
			return property;
		}
	}
}

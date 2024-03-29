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
	[CustomEditor(typeof(Spark.UIViewRoot), true)]
	public class UIViewRootInspector : ComponentEditor
	{
		protected override bool ShouldDrawProperties()
		{
			return !Application.isPlaying;
		}
		protected override void DrawCustomProperties()
        {
            SerializedProperty sp;
            var root = target as Spark.UIViewRoot;
            if (root.isRootView)
            {
                SparkEditorTools.DrawProperty("Camera", serializedObject, "m_Camera");
                SparkEditorTools.DrawProperty("Max Depth", serializedObject, "m_MaxDepth");
                SparkEditorTools.DrawProperty("Design Resolution", serializedObject, "m_DesignResolution");

                sp = serializedObject.FindProperty("m_MaskMode");
                SparkEditorTools.DrawProperty("Mask Mode", sp);
                EditorGUI.indentLevel++;
                if (sp.enumValueIndex == 0)
                {
                    SparkEditorTools.DrawProperty("Blur Camera", serializedObject, "m_BlurCamera");
                }
                else if (sp.enumValueIndex == 1)
                {
                    SparkEditorTools.DrawProperty("Default Mask Color", serializedObject, "m_DefaultMaskColor");
                }
                EditorGUI.indentLevel--;
            }

            sp = serializedObject.FindProperty("m_InvisibleMode");
			SparkEditorTools.DrawProperty("Invisible Mode", sp);
			if (sp.enumValueIndex == 1) {
				EditorGUI.indentLevel++;
				sp = serializedObject.FindProperty("m_InvisibleLayer");
				sp.intValue = EditorGUILayout.LayerField("Layer", sp.intValue);
				EditorGUI.indentLevel--;
			}
		}
	}
}

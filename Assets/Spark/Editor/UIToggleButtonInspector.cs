using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SparkEditor
{
	[CustomEditor(typeof(Spark.UIToggleButton), true)]
	public class UIToggleButtonInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			SparkEditorTools.SetLabelWidth(120f);
			GUILayout.Space(3f);
			serializedObject.Update();

			SparkEditorTools.DrawProperty("Interactable", serializedObject, "m_Interactable");
			GUILayout.Space(3f);
			SparkEditorTools.DrawProperty("Is On", serializedObject, "m_IsOn");
			SparkEditorTools.DrawProperty("On State", serializedObject, "m_OnState");
			SparkEditorTools.DrawProperty("Off State", serializedObject, "m_OffState");
			SparkEditorTools.DrawProperty("Group", serializedObject, "m_Group");
			GUILayout.Space(5f);
			SparkEditorTools.DrawProperty(serializedObject, "onValueChanged");

			serializedObject.ApplyModifiedProperties();
		}
	}
}

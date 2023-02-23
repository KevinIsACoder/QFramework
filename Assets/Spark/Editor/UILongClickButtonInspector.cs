using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace SparkEditor
{
	[CustomEditor(typeof(Spark.UILongClickButton), true)]
	public class UILongClickButtonInspector : SelectableEditor
	{
        SerializedProperty m_OnClickProperty;
        SerializedProperty m_OnLongClickProperty;
        SerializedProperty m_LongClickDelayProperty;
        SerializedProperty m_LongClickIntervalProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_OnClickProperty = serializedObject.FindProperty("m_OnClick");
            m_OnLongClickProperty = serializedObject.FindProperty("m_OnLongClick");
            m_LongClickDelayProperty = serializedObject.FindProperty("m_LongClickDelay");
            m_LongClickIntervalProperty = serializedObject.FindProperty("m_LongClickInterval");
        }

        public override void OnInspectorGUI()
		{
            base.OnInspectorGUI();

            serializedObject.Update();
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_LongClickDelayProperty);
            EditorGUILayout.PropertyField(m_LongClickIntervalProperty);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_OnClickProperty);
            EditorGUILayout.PropertyField(m_OnLongClickProperty);
            serializedObject.ApplyModifiedProperties();
		}
	}
}

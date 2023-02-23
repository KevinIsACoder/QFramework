#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using Spark;
using UnityEngine.UI;
using UnityEditorInternal;

[CustomEditor(typeof(Spark.UIComponentGroup))]
public class UIComponentGroupEditor : Editor
{
    protected SerializedProperty m_Script;
    protected SerializedProperty m_Components;
    private ReorderableList m_ComponentList;

    //private UIComponentGroup m_Group;

    protected virtual void OnEnable()
    {
        //m_Group = target as UIComponentGroup;
        m_Script = serializedObject.FindProperty("m_Script");
        m_Components = serializedObject.FindProperty("m_Components");
        m_ComponentList = new ReorderableList(serializedObject, m_Components, false, true, true, true);
        m_ComponentList.elementHeight = 60f;
        m_ComponentList.drawHeaderCallback = DrawHeader;
        m_ComponentList.drawElementCallback = DrawListItems;
    }

    void DrawHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, "Components (" + m_ComponentList.count + ")");
    }

    void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty element = m_ComponentList.serializedProperty.GetArrayElementAtIndex(index);
        SerializedProperty spName = element.FindPropertyRelative("name");
        SerializedProperty spObject = element.FindPropertyRelative("gameObject");
        SerializedProperty spComp = element.FindPropertyRelative("component");
        var objRect = new Rect(rect.x, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight);
        var nameRect = new Rect(rect.x, rect.y + 22, rect.width, EditorGUIUtility.singleLineHeight);
        var compRect = new Rect(rect.x, rect.y + 42, rect.width, EditorGUIUtility.singleLineHeight);

        EditorGUI.ObjectField(objRect, spObject, typeof(GameObject), new GUIContent("GameObject"));

        EditorGUI.indentLevel++;

        if (spObject.objectReferenceValue != null)
        {
            if (string.IsNullOrEmpty(spName.stringValue))
            {
                spName.stringValue = spObject.objectReferenceValue.name;
            }
            EditorGUI.PropertyField(nameRect, spName, new GUIContent("Property Name"));

            int currIndex = 0;
            Component currComp = spComp.objectReferenceValue as Component;

            List<Component> comStrs = new List<Component>();
            Component[] comps = ((GameObject)spObject.objectReferenceValue).GetComponents<Component>();

            for (int icom = 0; icom < comps.Length; icom++)
            {
                Component cpmp = comps[icom];
                comStrs.Add(cpmp);
                if (cpmp == currComp)
                {
                    currIndex = icom;
                }
            }

            if (currComp == null)
            {
                currIndex = 0;
            }

            string[] comArray = comStrs.ConvertAll<string>((v) => v.GetType().Name).ToArray();
            currIndex = EditorGUI.Popup(compRect, "Component", currIndex, comArray);
            spComp.objectReferenceValue = comps[currIndex];
        }
        else
        {
            spComp.objectReferenceValue = null;
            EditorGUI.BeginDisabledGroup(true);
            spName.stringValue = "";
            EditorGUI.PropertyField(nameRect, spName, new GUIContent("Property Name"));
            EditorGUI.Popup(compRect, "Component", 0, new string[] { });
            EditorGUI.EndDisabledGroup();
        }

        EditorGUI.indentLevel--;
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(m_Script);
        EditorGUILayout.Space();

        serializedObject.Update();

        m_ComponentList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }
}
#endif

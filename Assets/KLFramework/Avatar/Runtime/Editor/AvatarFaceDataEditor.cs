#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using KLFramework;
using System;
using System.IO;
using System.Text;
[CustomEditor(typeof(AvatarFaceEditorData))]
public class AvatarFaceDataEditor : Editor
{
    private SerializedProperty m_EditorDataPath;
    private SerializedProperty m_saveFaceDataPath; //保存捏脸的数据路径
    private SerializedProperty m_blendData;
    private SerializedProperty m_blendShapDatas;
    private SerializedProperty m_boneData;
    private SerializedProperty m_boneItemDatas;
    private SerializedProperty m_skinMesh;
    private ReorderableList m_RootList;
    private ReorderableList m_blendList;

    private SerializedProperty m_transformArray;

    private AvatarFaceEditorData m_target;
    public void OnEnable()
    {
        m_target = serializedObject.targetObject as AvatarFaceEditorData;
        m_EditorDataPath = serializedObject.FindProperty("editorDataPath");
        m_saveFaceDataPath = serializedObject.FindProperty("saveFaceDataPath");
        m_blendData = serializedObject.FindProperty("blendData");
        m_blendShapDatas = m_blendData.FindPropertyRelative("blendShapData");
        m_boneData = serializedObject.FindProperty("boneData");
        m_boneItemDatas = m_boneData.FindPropertyRelative("boneItemDatas");

        m_skinMesh = serializedObject.FindProperty("blendSkinMesh");

        m_RootList = new ReorderableList(serializedObject, m_boneItemDatas, false, true, true, true);
        m_RootList.elementHeight = 180f;
        m_RootList.drawHeaderCallback = DrawHeader;
        m_RootList.drawElementCallback = DrawListItems;
        m_RootList.onAddCallback = OnRootAddCallBack;

        m_blendList = new ReorderableList(serializedObject, m_blendShapDatas, false, true, true, true);
        m_blendList.elementHeight = 60f;
        m_blendList.drawHeaderCallback = DrawBlendItemHeader;
        m_blendList.drawElementCallback = DrawBlendElement;

        m_transformArray = serializedObject.FindProperty("bones");
    }

    void DrawHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, "Roots (" + m_RootList.count + ")");
    }

    void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty element = m_RootList.serializedProperty.GetArrayElementAtIndex(index);
        SerializedProperty blendWeight = element.FindPropertyRelative("blendWeight");
        SerializedProperty position = element.FindPropertyRelative("position");
        SerializedProperty rotation = element.FindPropertyRelative("rotation");
        SerializedProperty scale = element.FindPropertyRelative("scale");

        var bonesRect = new Rect(rect.x, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight);
        var posRect = new Rect(rect.x, rect.y + 32, rect.width, EditorGUIUtility.singleLineHeight);
        var rotationRect = new Rect(rect.x, rect.y + 72, rect.width, EditorGUIUtility.singleLineHeight);
        var scaleRect = new Rect(rect.x, rect.y + 112, rect.width, EditorGUIUtility.singleLineHeight);
        var blendWeightRect = new Rect(rect.x, rect.y + 152, rect.width, EditorGUIUtility.singleLineHeight);
        
        var trans = m_transformArray.GetArrayElementAtIndex(index);
        EditorGUI.ObjectField(bonesRect, trans, new GUIContent("Transform"));
        EditorGUI.PropertyField(posRect, position, new GUIContent("Position"));
        EditorGUI.PropertyField(rotationRect, rotation, new GUIContent("Rotaion"));
        EditorGUI.PropertyField(scaleRect, scale, new GUIContent("Scale"));
        EditorGUI.Slider(blendWeightRect, blendWeight, 0f, 100f);
    }

    void DrawBlendItemHeader(Rect rt)
    {
        EditorGUI.LabelField(rt, "BlendShaps (" + m_blendList.count + ")");
    }

    void DrawBlendElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty element = m_blendList.serializedProperty.GetArrayElementAtIndex(index);
        SerializedProperty blendweight = element.FindPropertyRelative("blendWeight");
        SerializedProperty blendName = element.FindPropertyRelative("blendName");

        var weightRect = new Rect(rect.x, rect.y + 32, rect.width, EditorGUIUtility.singleLineHeight);
        var nameRect = new Rect(rect.x, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight);

        EditorGUI.PropertyField(nameRect, blendName);
        EditorGUI.Slider(weightRect, blendweight, 0, 100f);
    }

    void OnRootAddCallBack(ReorderableList reorderableList)
    {
        if(reorderableList.serializedProperty != null)
        {
            reorderableList.serializedProperty.arraySize++;
            var index = reorderableList.serializedProperty.arraySize - 1;
            reorderableList.index = index;
            SerializedProperty element = m_RootList.serializedProperty.GetArrayElementAtIndex(index);
            var blendWeight = element.FindPropertyRelative("blendWeight");
            blendWeight.floatValue = 0;
            var position = element.FindPropertyRelative("position");
            position.vector3Value = Vector3.zero;
            var rotation = element.FindPropertyRelative("rotation");
            rotation.vector3Value = Vector3.zero;
            var scale = element.FindPropertyRelative("scale");
            scale.vector3Value = Vector3.zero;

            var trans = m_transformArray.GetArrayElementAtIndex(index);
            trans.objectReferenceValue = null;
            serializedObject.ApplyModifiedProperties();
            // if(serializedObject.ApplyModifiedProperties())
            // {
            //     m_target.UpdateBoneData(m_target.boneData);
            //     m_target.UpdateBlendData(m_target.blendData);
            // }
        }
        else
        {
            ReorderableList.defaultBehaviours.DoAddButton(reorderableList);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        m_EditorDataPath.stringValue = EditorGUILayout.TextField("EditorDataPath", m_EditorDataPath.stringValue);
        EditorGUILayout.Space();
        m_saveFaceDataPath.stringValue = EditorGUILayout.TextField("AvatarFaceDataPath", m_saveFaceDataPath.stringValue);
        EditorGUILayout.Space();
        m_RootList.DoLayoutList();

        EditorGUILayout.Space();
        EditorGUILayout.ObjectField(m_skinMesh, typeof(SkinnedMeshRenderer), new GUIContent("SkinMeshRenderer"));
        EditorGUILayout.Space();
        m_blendList.DoLayoutList();

        if (GUILayout.Button("Save AvatarEditorData"))
        {
            if (m_target != null)
            {
                m_target.UpdateBoneData(null);
                m_target.UpdateBlendData(null);
                var jsonBoneData = JsonUtility.ToJson(m_target.boneData);
                var jsonBlendData = JsonUtility.ToJson(m_target.blendData);
                var jsonData = string.Format("{0}|{1}", jsonBoneData, jsonBlendData);
                
                var dir = Path.GetDirectoryName(m_EditorDataPath.stringValue);
                var fileName = Path.GetFileName(m_EditorDataPath.stringValue);
                var path = EditorUtility.SaveFilePanel("保存编辑器数据", dir, fileName, "txt");
                File.WriteAllText(path, jsonData);
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Load AvatarEditorData"))
        {
            serializedObject.Update();
            var dir = Path.GetDirectoryName(m_EditorDataPath.stringValue);
            string path = EditorUtility.OpenFilePanel("选择编辑器数据", dir, "txt");
            var file = File.ReadAllText(path);
            m_target.LoadFaceData(file);
            serializedObject.ApplyModifiedProperties();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Save AvatarFaceData"))
        {
            m_target.SaveFaceData();
        }


        if (serializedObject.ApplyModifiedProperties())
        {
            m_target.UpdateBoneData(m_target.boneData);
            m_target.UpdateBlendData(m_target.blendData);
        }
    }

    private float fixed4(float value)
    {
        return (float)(Math.Floor(value * 10000) / 10000);
    }

    public string GetTransformPath(Transform trans)
    {
        StringBuilder sb = new StringBuilder();
        while (trans.parent != null)
        {
            sb.Insert(0, trans.name);
            sb.Insert(0, "/");
            trans = trans.parent;
        }
        sb.Insert(0, trans.name);
        var path = sb.ToString();
        path = path.Substring(path.IndexOf("/") + 1);
        return path;
    }
}

#endif

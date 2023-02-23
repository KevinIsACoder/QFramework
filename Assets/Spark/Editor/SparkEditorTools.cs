//----------------------------------------------------
// Spark: A Framework For Unity
// Copyright © 2014 - 2015 Jay Hu (Q:156809986)
//----------------------------------------------------

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Text;
using System;

/// <summary>
/// Tools for the editor
/// </summary>

public static class SparkEditorTools
{
	static public string assetsPath
	{
		get
		{
			return Application.dataPath + "/SparkAssets";
		}
	}

	public static float labelMaxWidth
	{
		get
		{
			return ((EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth) + 5f);
		}
	}

	public static float labelMinWidth
	{
		get
		{
			return ((EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth) + 5f);
		}
	}

	static public void Assert(string text = "")
	{
		throw new Exception(text);
	}
	static public void Assert(bool value, string text = "")
	{
		if (!value) {
			throw new Exception(text);
		}
	}

	static public string GetRelativeAssetsPath(string path)
	{
		bool found = false;
		string fileName = "";
		List<string> results = new List<string>();
		string[] paths = Path.GetFullPath(path).Split(Path.DirectorySeparatorChar);
		for (int i = paths.Length - 1; i >= 0; i--) {
			string name = paths[i];
			if (name == "Resources" || name == "SparkAssets") {
				found = true;
				break;
			} else if (fileName == "" && name.IndexOf(".") > 0) {
				fileName = name;
			}
			results.Insert(0, name);
		}
		return found ? string.Join("/", results.ToArray()) : fileName;
	}

	static private bool minimalisticLook = false;

	static public bool DrawPrefixButton(string text)
	{
		return GUILayout.Button(text, "DropDown", GUILayout.Width(76f));
	}

	static public bool DrawPrefixButton(string text, params GUILayoutOption[] options)
	{
		return GUILayout.Button(text, "DropDown", options);
	}

	static public int DrawPrefixList(int index, string[] list, params GUILayoutOption[] options)
	{
		return EditorGUILayout.Popup(index, list, "DropDown", options);
	}

	static public int DrawPrefixList(string text, int index, string[] list, params GUILayoutOption[] options)
	{
		return EditorGUILayout.Popup(text, index, list, "DropDown", options);
	}

	/// <summary>
	/// Draw a distinctly different looking header label
	/// </summary>

	static public bool DrawMinimalisticHeader(string text)
	{
		return DrawHeader(text, text, false, true);
	}

	/// <summary>
	/// Draw a distinctly different looking header label
	/// </summary>

	static public bool DrawHeader(string text)
	{
		return DrawHeader(text, text, false, SparkEditorTools.minimalisticLook);
	}

	/// <summary>
	/// Draw a distinctly different looking header label
	/// </summary>

	static public bool DrawHeader(string text, string key)
	{
		return DrawHeader(text, key, false, SparkEditorTools.minimalisticLook);
	}

	/// <summary>
	/// Draw a distinctly different looking header label
	/// </summary>

	static public bool DrawHeader(string text, bool detailed)
	{
		return DrawHeader(text, text, detailed, !detailed);
	}

	/// <summary>
	/// Draw a distinctly different looking header label
	/// </summary>

	static public bool DrawHeader(string text, string key, bool forceOn, bool minimalistic)
	{
		bool state = EditorPrefs.GetBool(key, true);

		if (!minimalistic)
			GUILayout.Space(3f);
		if (!forceOn && !state)
			GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
		GUILayout.BeginHorizontal();
		GUI.changed = false;

		if (minimalistic) {
			if (state)
				text = "\u25BC" + (char)0x200a + text;
			else
				text = "\u25BA" + (char)0x200a + text;

			GUILayout.BeginHorizontal();
			GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
			if (!GUILayout.Toggle(true, text, "PreToolbar2", GUILayout.MinWidth(20f)))
				state = !state;
			GUI.contentColor = Color.white;
			GUILayout.EndHorizontal();
		} else {
			text = "<b><size=11>" + text + "</size></b>";
			if (state)
				text = "\u25BC " + text;
			else
				text = "\u25BA " + text;
			if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f)))
				state = !state;
		}

		if (GUI.changed)
			EditorPrefs.SetBool(key, state);

		if (!minimalistic)
			GUILayout.Space(2f);
		GUILayout.EndHorizontal();
		GUI.backgroundColor = Color.white;
		if (!forceOn && !state)
			GUILayout.Space(3f);
		return state;
	}

	/// <summary>
	/// Begin drawing the content area.
	/// </summary>

	static public void BeginContents()
	{
		BeginContents(SparkEditorTools.minimalisticLook);
	}

	static bool mEndHorizontal = false;

	/// <summary>
	/// Begin drawing the content area.
	/// </summary>

	static public void BeginContents(bool minimalistic)
	{
		if (!minimalistic) {
			mEndHorizontal = true;
			GUILayout.BeginHorizontal();
#if UNITY_5_6_OR_NEWER
			EditorGUILayout.BeginHorizontal("TextArea", GUILayout.MinHeight(10f));
#else
			EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
#endif
		}
		else {
			mEndHorizontal = false;
			EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
			GUILayout.Space(10f);
		}
		GUILayout.BeginVertical();
		GUILayout.Space(2f);
	}

	/// <summary>
	/// End drawing the content area.
	/// </summary>

	static public void EndContents()
	{
		GUILayout.Space(3f);
		GUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();

		if (mEndHorizontal) {
			GUILayout.Space(3f);
			GUILayout.EndHorizontal();
		}

		GUILayout.Space(3f);
	}

	static public void BeginIndent()
	{
		EditorGUI.indentLevel++;
	}
	static public void EndIndent()
	{
		EditorGUI.indentLevel--;
	}

	static public void SortingLayerField(GUIContent label, SerializedProperty layerID, GUIStyle style)
	{
		typeof(EditorGUILayout).InvokeMember("SortingLayerField", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, new object[] { label, layerID, style });
	}

	static public void SortingLayerField(GUIContent label, SerializedProperty layerID, GUIStyle style, GUIStyle labelStyle)
	{
		typeof(EditorGUILayout).InvokeMember("SortingLayerField", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, new object[] { label, layerID, style, labelStyle });
	}

	/// <summary>
	/// Helper function that draws a serialized property.
	/// </summary>

	static public SerializedProperty DrawProperty(SerializedObject serializedObject, string property, params GUILayoutOption[] options)
	{
		return DrawProperty(null, serializedObject, property, false, options);
	}

	/// <summary>
	/// Helper function that draws a serialized property.
	/// </summary>

	static public SerializedProperty DrawProperty(string label, SerializedObject serializedObject, string property, params GUILayoutOption[] options)
	{
		return DrawProperty(label, serializedObject, property, false, options);
	}

	/// <summary>
	/// Helper function that draws a serialized property.
	/// </summary>

	static public SerializedProperty DrawPaddedProperty(SerializedObject serializedObject, string property, params GUILayoutOption[] options)
	{
		return DrawProperty(null, serializedObject, property, true, options);
	}

	/// <summary>
	/// Helper function that draws a serialized property.
	/// </summary>

	static public SerializedProperty DrawPaddedProperty(string label, SerializedObject serializedObject, string property, params GUILayoutOption[] options)
	{
		return DrawProperty(label, serializedObject, property, true, options);
	}

	/// <summary>
	/// Helper function that draws a serialized property.
	/// </summary>

	static public SerializedProperty DrawProperty(string label, SerializedObject serializedObject, string property, bool padding, params GUILayoutOption[] options)
	{
		SerializedProperty sp = serializedObject.FindProperty(property);

		if (sp != null) {
			if (SparkEditorTools.minimalisticLook)
				padding = false;

			if (padding)
				EditorGUILayout.BeginHorizontal();

			if (label != null)
				EditorGUILayout.PropertyField(sp, new GUIContent(label), options);
			else
				EditorGUILayout.PropertyField(sp, options);

			if (padding) {
				SparkEditorTools.DrawPadding();
				EditorGUILayout.EndHorizontal();
			}
		}
		return sp;
	}

	/// <summary>
	/// Helper function that draws a serialized property.
	/// </summary>

	static public void DrawProperty(string label, SerializedProperty sp, params GUILayoutOption[] options)
	{
		DrawProperty(label, sp, false, options);
	}

	/// <summary>
	/// Helper function that draws a serialized property.
	/// </summary>

	static public void DrawProperty(string label, SerializedProperty sp, bool padding, params GUILayoutOption[] options)
	{
		if (sp != null) {
			if (padding)
				EditorGUILayout.BeginHorizontal();

			if (label != null)
				EditorGUILayout.PropertyField(sp, new GUIContent(label), options);
			else
				EditorGUILayout.PropertyField(sp, options);

			if (padding) {
				SparkEditorTools.DrawPadding();
				EditorGUILayout.EndHorizontal();
			}
		}
	}

	/// <summary>
	/// Helper function that draws a compact Vector4.
	/// </summary>

	static public void DrawBorderProperty(string name, SerializedObject serializedObject, string field)
	{
		if (serializedObject.FindProperty(field) != null) {
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(name, GUILayout.Width(75f));

				SparkEditorTools.SetLabelWidth(50f);
				GUILayout.BeginVertical();
				SparkEditorTools.DrawProperty("Left", serializedObject, field + ".x", GUILayout.MinWidth(80f));
				SparkEditorTools.DrawProperty("Bottom", serializedObject, field + ".y", GUILayout.MinWidth(80f));
				GUILayout.EndVertical();

				GUILayout.BeginVertical();
				SparkEditorTools.DrawProperty("Right", serializedObject, field + ".z", GUILayout.MinWidth(80f));
				SparkEditorTools.DrawProperty("Top", serializedObject, field + ".w", GUILayout.MinWidth(80f));
				GUILayout.EndVertical();

				SparkEditorTools.SetLabelWidth(80f);
			}
			GUILayout.EndHorizontal();
		}
	}

	/// <summary>
	/// Helper function that draws a compact Rect.
	/// </summary>

	static public void DrawRectProperty(string name, SerializedObject serializedObject, string field)
	{
		DrawRectProperty(name, serializedObject, field, 56f, 18f);
	}

	/// <summary>
	/// Helper function that draws a compact Rect.
	/// </summary>

	static public void DrawRectProperty(string name, SerializedObject serializedObject, string field, float labelWidth, float spacing)
	{
		if (serializedObject.FindProperty(field) != null) {
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label(name, GUILayout.Width(labelWidth));

				SparkEditorTools.SetLabelWidth(20f);
				GUILayout.BeginVertical();
				SparkEditorTools.DrawProperty("X", serializedObject, field + ".x", GUILayout.MinWidth(50f));
				SparkEditorTools.DrawProperty("Y", serializedObject, field + ".y", GUILayout.MinWidth(50f));
				GUILayout.EndVertical();

				SparkEditorTools.SetLabelWidth(50f);
				GUILayout.BeginVertical();
				SparkEditorTools.DrawProperty("Width", serializedObject, field + ".width", GUILayout.MinWidth(80f));
				SparkEditorTools.DrawProperty("Height", serializedObject, field + ".height", GUILayout.MinWidth(80f));
				GUILayout.EndVertical();

				SparkEditorTools.SetLabelWidth(80f);
				if (spacing != 0f)
					GUILayout.Space(spacing);
			}
			GUILayout.EndHorizontal();
		}
	}

	/// <summary>
	/// Unity 4.3 changed the way LookLikeControls works.
	/// </summary>

	static public void SetLabelWidth(float width)
	{
		EditorGUIUtility.labelWidth = width;
	}

	/// <summary>
	/// Create an undo point for the specified objects.
	/// </summary>

	//static public void RegisterUndo(string name, params Object[] objects)
	//{
	//	if (objects != null && objects.Length > 0) {
	//		UnityEditor.Undo.RecordObjects(objects, name);

	//		foreach (Object obj in objects) {
	//			if (obj == null)
	//				continue;
	//			EditorUtility.SetDirty(obj);
	//		}
	//	}
	//}

	/// <summary>
	/// Draw 18 pixel padding on the right-hand side. Used to align fields.
	/// </summary>

	static public void DrawPadding()
	{
		if (!SparkEditorTools.minimalisticLook)
			GUILayout.Space(18f);
	}

	static public void DrawSeparator()
	{
		GUILayout.Space(12f);

		if (Event.current.type == EventType.Repaint) {
			Texture2D tex = EditorGUIUtility.whiteTexture;
			Rect rect = GUILayoutUtility.GetLastRect();
			GUI.color = new Color(0f, 0f, 0f, 0.25f);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 4f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 1f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 9f, Screen.width, 1f), tex);
			GUI.color = Color.white;
		}
	}
	static public void DrawOutline(Rect rect, Color color)
	{
		if (Event.current.type == EventType.Repaint) {
			Texture2D tex = EditorGUIUtility.whiteTexture;
			GUI.color = color;
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, 1f, rect.height), tex);
			GUI.DrawTexture(new Rect(rect.xMax, rect.yMin, 1f, rect.height), tex);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), tex);
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMax, rect.width, 1f), tex);
			GUI.color = Color.white;
		}
	}
}

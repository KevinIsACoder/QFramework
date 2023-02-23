using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.Sprites;
using UnityEngine;

namespace SparkEditor
{
	[CustomEditor(typeof(Spark.UIAtlas), true)]
	public class UIAtlasInspector : ComponentEditor
	{
		protected override void DrawCustomProperties()
		{
			Spark.UIAtlas atlas = target as Spark.UIAtlas;
			var sprites = serializedObject.FindProperty("m_Sprites");

			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Sprite Count: " + atlas.spriteCount);

			//EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Sprites")) {
				UIAtlasSpritesWindow.Show(atlas);
			}
			//if (GUILayout.Button("Texture")) {
			//	string spriteName = atlas.name;
			//	EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOn;
			//	Packer.RebuildAtlasCacheIfNeeded(EditorUserBuildSettings.activeBuildTarget, true);
			//	EditorApplication.ExecuteMenuItem("Window/Sprite Packer");
			//	var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.Sprites.PackerWindow");
			//	var window = EditorWindow.GetWindow(type);
			//	FieldInfo infoNames = type.GetField("m_AtlasNames", BindingFlags.NonPublic | BindingFlags.Instance);
			//	string[] infoNamesArray = (string[])infoNames.GetValue(window);

			//	if (infoNamesArray != null) {
			//		for (int i = 0; i < infoNamesArray.Length; i++) {
			//			string infoName = infoNamesArray[i];
			//			int index = infoName.IndexOf("(Group");
			//			if (index > 0) {
			//				infoName = infoName.Substring(0, index).Trim();
			//			}
			//			if (infoName == spriteName) {
			//				FieldInfo info = type.GetField("m_SelectedAtlas", BindingFlags.NonPublic | BindingFlags.Instance);
			//				info.SetValue(window, i);
			//				break;
			//			}
			//		}
			//	}
			//}
			//EditorGUILayout.EndHorizontal();
		}
	}
}

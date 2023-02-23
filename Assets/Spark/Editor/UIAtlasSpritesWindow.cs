using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Spark;

namespace SparkEditor
{
	public class UIAtlasSpritesWindow : ScriptableWizard
	{
		static public UIAtlasSpritesWindow Show(UIAtlas atlas, System.Action<Sprite> onSpriteSelect = null)
		{
			UIAtlasSpritesWindow window = ScriptableWizard.DisplayWizard<UIAtlasSpritesWindow>("Select a Sprite");
			window.m_Atlas = atlas;
			window.m_OnSpriteSelect = onSpriteSelect;
			return window;
		}

		static public void Show(UIAtlas atlas, SerializedObject so, SerializedProperty sp)
		{
			var window = Show(atlas, (sprite) => {
				if (so != null && sp != null) {
					so.Update();
					sp.stringValue = sprite.name;
					so.ApplyModifiedProperties();
				}
			});
			if (sp != null) {
				window.m_SelectedSprite = sp.stringValue;
			}
		}

		private UIAtlas m_Atlas = null;
		private string m_SelectedSprite = "";
		private Action<Sprite> m_OnSpriteSelect = null;

		private float m_ClickTime = 0f;
		private string m_SearchText = "";
		private Vector2 m_ScrollPosition = Vector2.zero;

		protected override bool DrawWizardGUI()
		{
			if (m_Atlas == null) {
				GUILayout.Label("No Atlas selected.", "LODLevelNotifyText");
			} else {
				GUILayout.Label(m_Atlas.name + " Sprites (" + m_Atlas.spriteCount + ")", "LODLevelNotifyText");
				SparkEditorTools.DrawSeparator();
				GUILayout.BeginHorizontal();
				GUILayout.Space(84f);
				m_SearchText = EditorGUILayout.TextField("", m_SearchText, "SearchTextField");
				if (GUILayout.Button("", "SearchCancelButton", GUILayout.Width(18f))) {
					m_SearchText = "";
					GUIUtility.keyboardControl = 0;
				}
				GUILayout.Space(84f);
				GUILayout.EndHorizontal();

				float size = 80f;
				float padded = size + 10f;
				int columns = Mathf.FloorToInt(Screen.width / padded);
				if (columns < 1)
					columns = 1;

				int offset = 0;
				Rect rect = new Rect(10f, 0, size, size);

				GUILayout.Space(10f);
				m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
				int rows = 1;

				List<Sprite> sprites = GetSprites(m_Atlas, m_SearchText);

				bool close = false;
				while (offset < sprites.Count) {
					GUILayout.BeginHorizontal();
					{
						int col = 0;
						rect.x = 10f;

						for (; offset < sprites.Count; ++offset) {
							var sprite = sprites[offset];

							// Button comes first
							if (GUI.Button(rect, "")) {
								if (Event.current.button == 0) {
									float delta = Time.realtimeSinceStartup - m_ClickTime;
									m_ClickTime = Time.realtimeSinceStartup;

									if (m_SelectedSprite != sprite.name) {
										m_SelectedSprite = sprite.name;
										if (m_OnSpriteSelect != null)
											m_OnSpriteSelect(sprite);
									} else if (delta < 0.5f) {
										close = true;
									}
								}
							}

							// On top of the button we have a checkboard grid
							//NGUIEditorTools.DrawTiledTexture(rect, NGUIEditorTools.backdropTexture);
							Texture tex = sprite.texture;
							Rect textureRect = sprite.textureRect;
							Rect uv = new Rect(textureRect.x / tex.width, textureRect.y / tex.height, textureRect.width / tex.width, textureRect.height / tex.height);

							// Calculate the texture's scale that's needed to display the sprite in the clipped area
							float scaleX = rect.width / uv.width;
							float scaleY = rect.height / uv.height;

							// Stretch the sprite so that it will appear proper
							float aspect = (scaleY / scaleX) / ((float)tex.height / tex.width);
							Rect clipRect = rect;

							if (aspect != 1f) {
								if (aspect < 1f) {
									// The sprite is taller than it is wider
									float padding = size * (1f - aspect) * 0.5f;
									clipRect.xMin += padding;
									clipRect.xMax -= padding;
								} else {
									// The sprite is wider than it is taller
									float padding = size * (1f - 1f / aspect) * 0.5f;
									clipRect.yMin += padding;
									clipRect.yMax -= padding;
								}
							}

							GUI.DrawTextureWithTexCoords(clipRect, tex, uv);

							// Draw the selection
							if (m_SelectedSprite == sprite.name) {
								SparkEditorTools.DrawOutline(rect, new Color(0.4f, 1f, 0f, 1f));
							}

							GUI.backgroundColor = new Color(1f, 1f, 1f, 0.5f);
							GUI.contentColor = new Color(1f, 1f, 1f, 0.7f);
							GUI.Label(new Rect(rect.x, rect.y + rect.height, rect.width, 32f), sprite.name, "ProgressBarBack");
							GUI.contentColor = Color.white;
							GUI.backgroundColor = Color.white;

							if (++col >= columns) {
								++offset;
								break;
							}
							rect.x += padded;
						}
					}
					GUILayout.EndHorizontal();
					GUILayout.Space(padded);
					rect.y += padded + 26;
					++rows;
				}
				GUILayout.Space(rows * 26);
				GUILayout.EndScrollView();

				if (close)
					Close();
			}

			return false;
		}

		static private List<Sprite> GetSprites(UIAtlas atlas, string match)
		{
			List<Sprite> sprites = atlas.GetSprites();

			if (string.IsNullOrEmpty(match)) {
				return sprites;
			}

			List<Sprite> list = new List<Sprite>();

			// First try to find an exact match
			foreach (var sprite in sprites) {
				if (string.Equals(match, sprite.name, StringComparison.OrdinalIgnoreCase)) {
					list.Add(sprite);
					return list;
				}
			}

			// No exact match found? Split up the search into space-separated components.
			string[] keywords = match.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < keywords.Length; ++i)
				keywords[i] = keywords[i].ToLower();

			// Try to find all sprites where all keywords are present
			foreach (var sprite in sprites) {
				string tl = sprite.name.ToLower();
				int matches = 0;

				for (int b = 0; b < keywords.Length; ++b) {
					if (tl.Contains(keywords[b]))
						++matches;
				}
				if (matches == keywords.Length)
					list.Add(sprite);
			}

			return list;
		}
	}
}

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace Spark
{
	[XLua.LuaCallCSharp]
	public class UIAtlas : MonoBehaviour, ISerializationCallbackReceiver
	{
		public UIAtlas replacement;

		[SerializeField]
		private List<Sprite> m_Sprites = new List<Sprite>();
		[SerializeField]
		private List<string> m_SpriteNames = new List<string>();

		private Dictionary<string, Sprite> m_SpriteDict = new Dictionary<string, Sprite>();
		private Dictionary<string, string> m_SpritePathDict = new Dictionary<string, string>();

		public int spriteCount
		{
			get
			{
				return m_Sprites.Count;
			}
		}

		public Sprite GetSprite(string name)
		{
			if (replacement != null) {
				return replacement.GetSprite(name);
			}
			return GetSpriteByName(name);
		}

		private Sprite GetSpriteByName(string name)
		{
			Sprite sprite = null;
			m_SpriteDict.TryGetValue(name, out sprite);
			return sprite;
		}

#if UNITY_EDITOR
		[XLua.BlackList]
		public List<Sprite> GetSprites()
		{
			if (replacement != null)
				return replacement.GetSprites();

			return m_Sprites;
		}

		[XLua.BlackList]
		public void SetSprites(Sprite[] sprites)
		{
			m_Sprites.Clear();
			m_SpriteNames.Clear();

			if (sprites != null) {
				foreach (var sprite in sprites)
				{
					m_Sprites.Add(sprite);
					m_SpriteNames.Add(sprite.name);
				}
			}
		}
#endif

		[XLua.BlackList]
		public void OnBeforeSerialize()
		{
		}

		[XLua.BlackList]
		public void OnAfterDeserialize()
		{
			m_SpriteDict.Clear();

			if (m_Sprites.Count != m_SpriteNames.Count)
				throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

			for (int i = m_Sprites.Count - 1; i >= 0; i--) {
				m_SpriteDict[m_SpriteNames[i]] = m_Sprites[i];
			}
		}
	}
}

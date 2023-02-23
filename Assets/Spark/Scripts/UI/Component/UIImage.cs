using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Spark
{
	[AddComponentMenu("Spark/UIImage", 10)]
	[XLua.LuaCallCSharp]
	public class UIImage : Image, IResettable
	{
		[SerializeField]
		internal protected UIAtlas m_Atlas;

		[SerializeField]
		internal protected string m_SpriteName;

		[SerializeField]
		private bool m_ForceNativeSize;

		public UIAtlas atlas
		{
			get
			{
				return m_Atlas;
			}
			set
			{
				if (m_Atlas == value)
					return;
				m_Atlas = value;
				UpdateSprite();
			}
		}

		public string spriteName
		{
			get
			{
				return m_SpriteName;
			}
			set
			{
				if (m_SpriteName == value)
					return;
				m_SpriteName = value;
				UpdateSprite();
			}
		}

		internal virtual protected void UpdateSprite()
		{
			if (m_Atlas == null || string.IsNullOrEmpty(m_SpriteName)) {
				sprite = null;
			} else {
				sprite = m_Atlas.GetSprite(m_SpriteName);
				if (m_ForceNativeSize && sprite != null) {
					SetNativeSize();
				}
			}
		}

		void IResettable.Reset()
		{
#if UNITY_EDITOR
			m_SpriteName = string.Empty;
			sprite = null;
#endif
		}

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			if (!Application.isPlaying) {
				UpdateSprite();
				base.OnValidate();
			}
		}
#endif
	}
}

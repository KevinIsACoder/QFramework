using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[XLua.LuaCallCSharp]
public class UIGameContentView : UIBehaviour
{
	private RectTransform m_Rect;
	//private DrivenRectTransformTracker m_Tracker;

	private RectTransform rectTransform
	{
		get
		{
			if (m_Rect == null)
			{
				m_Rect = GetComponent<RectTransform>();
			}
			return m_Rect;
		}
	}

//#if UNITY_EDITOR
//	protected override void OnValidate()
//	{
//		SetDirty();
//	}
//	protected override void Reset()
//	{
//		SetDirty();
//	}
//#endif

//	private void SetDirty()
//	{
//		m_Tracker.Clear();
//		m_Tracker.Add(this, rectTransform, DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.SizeDeltaX);

//		var rect = rectTransform;
//		rect.anchorMin = Vector2.zero;
//		rect.anchorMax = Vector2.one;
//		rect.offsetMin = Vector2.zero;
//		rect.offsetMax = Vector2.zero;
//		rect.anchoredPosition = Vector2.zero;
//	}
}

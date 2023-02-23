using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[XLua.LuaCallCSharp]
public class UITyperText : Text
{
	private int m_Index = 0;
	private int m_Total = 0;
	private int m_EndIndex = 0;
	
	static private UIVertex s_TempVert = new UIVertex();

	public Action onTypingComplete;

	public override string text
	{
		get
		{
			return base.text;
		}

		set
		{
			SetDirty();
			base.text = value;
			m_Total = value != null ? value.Length : 0;
		}
	}

	public bool typing
	{
		get;
		private set;
	}

	public bool ending
	{
		get
		{
			return m_Index >= m_Total;
		}
	}

	public void BeginTyping(float duration)
	{
		if (typing)
			return;
		typing = true;
		m_EndIndex = m_Text.IndexOf('\b', m_Index) + 1;
		if (m_EndIndex == 0) {
			m_EndIndex = m_Total;
		}
		DOTween.To(() => m_Index, (v) => {
			m_Index = v;
			SetVerticesDirty();
		}, m_EndIndex, duration).SetTarget(this).OnComplete(EndTyping);
	}

	public void EndTyping()
	{
		if (typing) {
			DOTween.Kill(this);
			if (m_Index < m_EndIndex) {
				m_Index = m_EndIndex;
				SetVerticesDirty();
			}
			typing = false;
		}
		if (onTypingComplete != null) {
			onTypingComplete.Invoke();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (DOTween.IsTweening(this)) {
			typing = true;
			DOTween.Play(this);
		}
	}
	protected override void OnDisable()
	{
		if (typing) {
			typing = false;
			DOTween.Pause(this);
		}
		base.OnDisable();
	}
	protected override void OnDestroy()
	{
		DOTween.Kill(this);
		base.OnDestroy();
	}

	private void SetDirty()
	{
		m_Index = 0;
		m_Total = 0;
		m_EndIndex = 0;
		DOTween.Kill(this);
	}

	protected override void OnPopulateMesh(VertexHelper toFill)
	{
		base.OnPopulateMesh(toFill);
#if UNITY_EDITOR
		if (!Application.isPlaying)
			return;
#endif
		if (font != null) {
			m_Total = Mathf.FloorToInt(toFill.currentVertCount / 4);
			if (m_Index < m_Total) {
				for (int i = m_Index; i < m_Total; i++) {
					for (int j = 0; j < 4; j++) {
						var index = i * 4 + j;
						toFill.PopulateUIVertex(ref s_TempVert, index);
						s_TempVert.color = Color.clear;
						toFill.SetUIVertex(s_TempVert, index);
					}
				}
			}
		}
	}

#if UNITY_EDITOR
	protected override void OnValidate()
	{
		SetDirty();
		base.OnValidate();
	}
#endif
}

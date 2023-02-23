using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core;

namespace Spark
{
	public class UIViewSettings : UnityEngine.EventSystems.UIBehaviour
	{
		[SerializeField]
		private UIStyle m_Style;

		[SerializeField]
		internal bool overrideMode = false;

		[SerializeField]
		internal UIViewRoot.InvisibleMode invisibleMode;

		[SerializeField]
		internal int invisibleLayer;

		private UIStyle m_RuntimeStyle;

		public UIStyle style
		{
			get
			{
				return m_RuntimeStyle;
			}
			internal set
			{
				m_Style = value;
			}
		}

		protected override void Awake()
		{
			m_RuntimeStyle = m_Style;
		}

		private Sequence m_ActiveAnimation;
		private UIViewOpenAnimation[] m_OpenAnimations;
		private UIViewCloseAnimation[] m_CloseAnimations;

		protected override void OnDestroy()
		{
			if (m_ActiveAnimation != null) {
				m_ActiveAnimation.Kill(false);
				m_ActiveAnimation = null;
			}
		}

		internal void CompleteActiveAnimations()
		{
			if (m_ActiveAnimation != null) {
				m_ActiveAnimation.Kill(true);
				m_ActiveAnimation = null;
			}
		}

		internal void PlayOpenAnimations(Action callback)
		{
			PlayAnimations<UIViewOpenAnimation>(ref m_OpenAnimations, callback);
		}
		internal void PlayCloseAnimations(Action callback)
		{
			PlayAnimations<UIViewCloseAnimation>(ref m_CloseAnimations, callback);
		}

		private void PlayAnimations<T>(ref T[] animations, Action callback) where T : DOTweenAnimation
		{
			if (animations == null) {
				animations = GetComponentsInChildren<T>(true);
			}
			CompleteActiveAnimations();
			if (animations != null && animations.Length > 0) {
				m_ActiveAnimation = DOTween.Sequence();
				foreach (var ani in animations) {
					if (ani.gameObject.activeInHierarchy) {
						if (ani.tween == null) {
							ani.CreateTween();
							if (ani.tween == null)
								continue;
						}
						ani.tween.Rewind();
						if (ani.duration == 0 && ani.delay == 0) {
							ani.tween.SetAutoKill(true).Complete();
						} else {
							ani.tween.SetAutoKill(true).Play();
							m_ActiveAnimation.Insert(0, ani.tween);
						}
					}
				}
				if (callback != null) {
					m_ActiveAnimation.OnComplete(callback.Invoke);
				}
				m_ActiveAnimation.SetAutoKill(true).OnKill(() => m_ActiveAnimation = null).Play();
			} else {
				if (callback != null) {
					callback();
				}
			}
		}
	}
}

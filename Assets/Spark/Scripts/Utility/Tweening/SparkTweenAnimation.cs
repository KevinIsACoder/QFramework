using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using UnityEngine;

namespace Spark
{
	[XLua.LuaCallCSharp]
	[AddComponentMenu("Spark/SparkTween Animation")]
	public class SparkTweenAnimation : DOTweenAnimation, IResettable
	{
		private Tweening.Tweener m_SparkTween;

		public Tweening.Tweener GetTween()
		{
			if (m_SparkTween == null) {
				if (tween == null) {
					CreateTween();
				}
				m_SparkTween = Tweening.Tweener.Get<Tweening.Tweener>(tween);
				m_SparkTween.OnKill(() => m_SparkTween = null);
			}
			return m_SparkTween;
		}

		void OnDestroy()
		{
			m_SparkTween = null;
		}

		void IResettable.Reset()
		{
#if UNITY_EDITOR
			autoKill = false;
			autoPlay = false;
#endif
		}
	}
}

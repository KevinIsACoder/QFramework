using System;
using System.Linq;
using DG.Tweening;
using System.Collections.Generic;

namespace Spark.Tweening
{
	[XLua.LuaCallCSharp]
	public enum UpdateType
	{
		Normal = 0,
		Late = 1,
		Fixed = 2
	}

	[XLua.LuaCallCSharp]
	public enum LoopType
	{
		Restart = 0,
		Yoyo = 1,
		Incremental = 2
	}

	[XLua.LuaCallCSharp]
	public class Tween
	{
		static Dictionary<Type, Stack<Tween>> m_TweenPool = new Dictionary<Type, Stack<Tween>>();

		internal static T Get<T>(DG.Tweening.Tween tween) where T : Tween, new()
		{
			var type = typeof(T);
			Stack<Tween> tweens;
			if (!m_TweenPool.TryGetValue(type, out tweens)) {
				tweens = new Stack<Tween>();
				m_TweenPool.Add(type, tweens);
			}
			return (T)(tweens.Count > 0 ? tweens.Pop() : new T()).Init(tween);
		}

		private static void Release(Tween t)
		{
			Stack<Tween> tweens;
			if (m_TweenPool.TryGetValue(t.GetType(), out tweens)) {
				tweens.Push(t);
			}
		}

		internal protected DG.Tweening.Tween m_DOTween;
		private Action m_OnKill;

		private Tween Init(DG.Tweening.Tween tween)
		{
			m_DOTween = tween.OnKill(OnKill);
			return this;
		}

		private void OnKill()
		{
			if (m_OnKill != null) {
				m_OnKill();
				m_OnKill = null;
			}
			m_DOTween = null;
			Release(this);
		}

		public void Complete()
		{
			m_DOTween.Complete();
		}

		public void Flip()
		{
			m_DOTween.Flip();
		}

		public void Kill()
		{
			Kill(false);
		}

		public void Kill(bool complete)
		{
			m_DOTween.Kill(complete);
		}

		public Tween Play()
		{
			m_DOTween.Play();
			return this;
		}

		public Tween Pause()
		{
			m_DOTween.Pause();
			return this;
		}

		public Tween PlayBackwards()
		{
			m_DOTween.PlayBackwards();
			return this;
		}

		public Tween PlayForward()
		{
			m_DOTween.PlayForward();
			return this;
		}

		public Tween Rewind()
		{
			return Rewind(true);
		}
		public Tween Rewind(bool includeDelay)
		{
			m_DOTween.Rewind(includeDelay);
			return this;
		}

		public Tween Restart()
		{
			return Restart(true, -1);
		}
		public Tween Restart(bool includeDelay)
		{
			return Restart(includeDelay, -1);
		}
		public Tween Restart(bool includeDelay, float changeDelayTo)
		{
			m_DOTween.Restart(includeDelay, changeDelayTo);
			return this;
		}

		public Tween SmoothRewind()
		{
			m_DOTween.SmoothRewind();
			return this;
		}

		public Tween TogglePause()
		{
			m_DOTween.TogglePause();
			return this;
		}

		public bool IsActive()
		{
			return m_DOTween.IsActive();
		}

		public bool IsBackwards()
		{
			return m_DOTween.IsBackwards();
		}

		public bool IsComplete()
		{
			return m_DOTween.IsComplete();
		}

		public bool IsPlaying()
		{
			return m_DOTween.IsPlaying();
		}

		public bool IsInitialized()
		{
			return m_DOTween.IsInitialized();
		}

		public Tween OnComplete(Action onComplete)
		{
			m_DOTween.OnComplete(onComplete.Invoke);
			return this;
		}
		public Tween OnKill(Action onKill)
		{
			m_OnKill = onKill;
			return this;
		}
		public Tween OnPause(Action onPause)
		{
			m_DOTween.OnPause(onPause.Invoke);
			return this;
		}
		public Tween OnPlay(Action onPlay)
		{
			m_DOTween.OnPlay(onPlay.Invoke);
			return this;
		}
		public Tween OnRewind(Action onRewind)
		{
			m_DOTween.OnRewind(onRewind.Invoke);
			return this;
		}
		public Tween OnStart(Action onStart)
		{
			m_DOTween.OnStart(onStart.Invoke);
			return this;
		}
		public Tween OnStepComplete(Action onStepComplete)
		{
			m_DOTween.OnStepComplete(onStepComplete.Invoke);
			return this;
		}
		public Tween OnUpdate(Action onUpdate)
		{
			m_DOTween.OnUpdate(onUpdate.Invoke);
			return this;
		}
		public Tween OnWaypointChange(Action<int> onWaypointChange)
		{
			m_DOTween.OnWaypointChange(onWaypointChange.Invoke);
			return this;
		}

		public Tween SetAutoKill()
		{
			m_DOTween.SetAutoKill();
			return this;
		}
		public Tween SetAutoKill(bool autoKillOnCompletion)
		{
			m_DOTween.SetAutoKill(autoKillOnCompletion);
			return this;
		}
		public Tween SetDelay(float delay)
		{
			m_DOTween.SetDelay(delay);
			return this;
		}
		public Tween SetEase(Ease ease)
		{
			m_DOTween.SetEase((DG.Tweening.Ease)ease);
			return this;
		}
		public Tween SetEase(Ease ease, float overshoot)
		{
			m_DOTween.SetEase((DG.Tweening.Ease)ease, overshoot);
			return this;
		}
		public Tween SetEase(Ease ease, float amplitude, float period)
		{
			m_DOTween.SetEase((DG.Tweening.Ease)ease, amplitude, period);
			return this;
		}
		public Tween SetId(object id)
		{
			m_DOTween.SetId(id);
			return this;
		}
		public Tween SetLoops(int loops)
		{
			m_DOTween.SetLoops(loops);
			return this;
		}
		public Tween SetLoops(int loops, LoopType loopType)
		{
			m_DOTween.SetLoops(loops, (DG.Tweening.LoopType)loopType);
			return this;
		}
		public Tween SetRecyclable()
		{
			m_DOTween.SetRecyclable();
			return this;
		}
		public Tween SetRecyclable(bool recyclable)
		{
			m_DOTween.SetRecyclable(recyclable);
			return this;
		}
		public Tween SetRelative()
		{
			m_DOTween.SetRelative();
			return this;
		}
		public Tween SetRelative(bool isRelative)
		{
			m_DOTween.SetRelative(isRelative);
			return this;
		}
		public Tween SetSpeedBased()
		{
			m_DOTween.SetSpeedBased();
			return this;
		}
		public Tween SetSpeedBased(bool isSpeedBased)
		{
			m_DOTween.SetSpeedBased(isSpeedBased);
			return this;
		}
		public Tween SetTarget(object target)
		{
			m_DOTween.SetTarget(target);
			return this;
		}
		public Tween SetUpdate(bool isIndependentUpdate)
		{
			m_DOTween.SetUpdate(isIndependentUpdate);
			return this;
		}
		public Tween SetUpdate(UpdateType updateType)
		{
			m_DOTween.SetUpdate((DG.Tweening.UpdateType)updateType);
			return this;
		}
		public Tween SetUpdate(UpdateType updateType, bool isIndependentUpdate)
		{
			m_DOTween.SetUpdate((DG.Tweening.UpdateType)updateType, isIndependentUpdate);
			return this;
		}

		public Tween SetTimeScale(float timeScale)
		{
			m_DOTween.timeScale = timeScale;
			return this;
		}
	}

	[XLua.LuaCallCSharp]
	public class Tweener : Tween
	{
		public Tweener From()
		{
			((DG.Tweening.Tweener)m_DOTween).From();
			return this;
		}
		public Tweener From(bool isRelative)
		{
			((DG.Tweening.Tweener)m_DOTween).From(isRelative);
			return this;
		}
	}

	[XLua.LuaCallCSharp]
	public class Sequence : Tween
	{
		public Sequence Append(Tween t)
		{
			((DG.Tweening.Sequence)m_DOTween).Append(t.m_DOTween);
			return this;
		}

		public Sequence AppendCallback(Action callback)
		{
			((DG.Tweening.Sequence)m_DOTween).AppendCallback(callback.Invoke);
			return this;
		}

		public Sequence AppendInterval(float interval)
		{
			((DG.Tweening.Sequence)m_DOTween).AppendInterval(interval);
			return this;
		}

		public Sequence Insert(float atPosition, Tween t)
		{
			((DG.Tweening.Sequence)m_DOTween).Insert(atPosition, t.m_DOTween);
			return this;
		}

		public Sequence InsertCallback(float atPosition, Action callback)
		{
			((DG.Tweening.Sequence)m_DOTween).InsertCallback(atPosition, callback.Invoke);
			return this;
		}

		public Sequence Join(Tween t)
		{
			((DG.Tweening.Sequence)m_DOTween).Join(t.m_DOTween);
			return this;
		}

		public Sequence Prepend(Tween t)
		{
			((DG.Tweening.Sequence)m_DOTween).Prepend(t.m_DOTween);
			return this;
		}

		public Sequence PrependCallback(Action callback)
		{
			((DG.Tweening.Sequence)m_DOTween).PrependCallback(callback.Invoke);
			return this;
		}

		public Sequence PrependInterval(float interval)
		{
			((DG.Tweening.Sequence)m_DOTween).PrependInterval(interval);
			return this;
		}
	}
}

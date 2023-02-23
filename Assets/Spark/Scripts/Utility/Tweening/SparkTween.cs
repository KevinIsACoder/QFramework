using DG.Tweening;
using SparkSequence = Spark.Tweening.Sequence;

[XLua.LuaCallCSharp]
public static partial class SparkTween
{
	public static SparkSequence Sequence()
	{
		return SparkSequence.Get<SparkSequence>(DOTween.Sequence());
	}

	public static void Clear()
	{
		DOTween.Clear();
	}
	public static void Clear(bool destroy)
	{
		DOTween.Clear(destroy);
	}
	public static int Complete(object targetOrId)
	{
		return DOTween.Complete(targetOrId);
	}
	public static int Complete(object targetOrId, bool withCallbacks)
	{
		return DOTween.Complete(targetOrId, withCallbacks);
	}
	public static int CompleteAll()
	{
		return DOTween.CompleteAll();
	}
	public static int CompleteAll(bool withCallbacks)
	{
		return DOTween.CompleteAll(withCallbacks);
	}
	public static int Flip(object targetOrId)
	{
		return DOTween.Flip(targetOrId);
	}
	public static int FlipAll()
	{
		return DOTween.FlipAll();
	}
	public static int Goto(object targetOrId, float to)
	{
		return DOTween.Goto(targetOrId, to);
	}
	public static int Goto(object targetOrId, float to, bool andPlay)
	{
		return DOTween.Goto(targetOrId, to, andPlay);
	}
	public static int GotoAll(float to)
	{
		return DOTween.GotoAll(to);
	}
	public static int GotoAll(float to, bool andPlay)
	{
		return DOTween.GotoAll(to, andPlay);
	}
	public static bool IsTweening(object targetOrId)
	{
		return DOTween.IsTweening(targetOrId);
	}
	public static int Kill(object targetOrId)
	{
		return DOTween.Kill(targetOrId);
	}
	public static int Kill(object targetOrId, bool complete)
	{
		return DOTween.Kill(targetOrId, complete);
	}
	public static int KillAll()
	{
		return DOTween.KillAll();
	}
	public static int KillAll(bool complete)
	{
		return DOTween.KillAll(complete);
	}
	public static int KillAll(bool complete, params object[] idsOrTargetsToExclude)
	{
		return DOTween.KillAll(complete, idsOrTargetsToExclude);
	}
	public static int Pause(object targetOrId)
	{
		return DOTween.Pause(targetOrId);
	}
	public static int PauseAll()
	{
		return DOTween.PauseAll();
	}
	public static int Play(object targetOrId)
	{
		return DOTween.Play(targetOrId);
	}
	public static int Play(object target, object id)
	{
		return DOTween.Play(target, id);
	}
	public static int PlayAll()
	{
		return DOTween.PlayAll();
	}
	public static int PlayBackwards(object targetOrId)
	{
		return DOTween.PlayBackwards(targetOrId);
	}
	public static int PlayBackwards(object target, object id)
	{
		return DOTween.PlayBackwards(target, id);
	}
	public static int PlayBackwardsAll()
	{
		return DOTween.PlayBackwardsAll();
	}
	public static int PlayForward(object targetOrId)
	{
		return DOTween.PlayForward(targetOrId);
	}
	public static int PlayForward(object target, object id)
	{
		return DOTween.PlayForward(target, id);
	}
	public static int PlayForwardAll()
	{
		return DOTween.PlayForwardAll();
	}
	public static int Restart(object targetOrId)
	{
		return DOTween.Restart(targetOrId);
	}
	public static int Restart(object targetOrId, bool includeDelay)
	{
		return DOTween.Restart(targetOrId, includeDelay);
	}
	public static int Restart(object target, object id)
	{
		return DOTween.Restart(target, id);
	}
	public static int Restart(object target, object id, bool includeDelay)
	{
		return DOTween.Restart(target, id, includeDelay);
	}
	public static int RestartAll()
	{
		return DOTween.RestartAll();
	}
	public static int RestartAll(bool includeDelay)
	{
		return DOTween.RestartAll(includeDelay);
	}
	public static int Rewind(object targetOrId)
	{
		return DOTween.Rewind(targetOrId);
	}
	public static int Rewind(object targetOrId, bool includeDelay)
	{
		return DOTween.Rewind(targetOrId, includeDelay);
	}
	public static int RewindAll()
	{
		return DOTween.RewindAll();
	}
	public static int RewindAll(bool includeDelay)
	{
		return DOTween.RewindAll(includeDelay);
	}
	public static int SmoothRewind(object targetOrId)
	{
		return DOTween.SmoothRewind(targetOrId);
	}
	public static int SmoothRewindAll()
	{
		return DOTween.SmoothRewindAll();
	}
	public static int TogglePause(object targetOrId)
	{
		return DOTween.TogglePause(targetOrId);
	}
	public static int TogglePauseAll()
	{
		return DOTween.TogglePauseAll();
	}
	public static int TotalPlayingTweens()
	{
		return DOTween.TotalPlayingTweens();
	}
}
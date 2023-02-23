using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using SparkTweener = Spark.Tweening.Tweener;

namespace Spark.Tweening
{
	[XLua.LuaCallCSharp]
	public enum RotateMode
	{
		Fast = 0,
		FastBeyond360 = 1,
		WorldAxisAdd = 2,
		LocalAxisAdd = 3
	}

	[XLua.LuaCallCSharp]
	public enum PathType
	{
		Linear = 0,
		CatmullRom = 1
	}

	[XLua.LuaCallCSharp]
	public enum PathMode
	{
		Ignore = 0,
		Full3D = 1,
		TopDown2D = 2,
		Sidescroller2D = 3
	}
}

public static partial class SparkTween
{
	#region Generate
	public static SparkTweener ToInt(Action<int> setter, int startValue, int endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(DOTween.To(() => startValue, setter.Invoke, endValue, duration));
	}
	public static SparkTweener ToFloat(Action<float> setter, float startValue, float endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(DOTween.To(setter.Invoke, startValue, endValue, duration));
	}
	#endregion

	#region Transform
	public static SparkTweener DOMove(Transform target, float x, float y, float z, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOMove(new Vector3(x, y, z), duration));
	}
	public static SparkTweener DOMove(Transform target, float x, float y, float z, float duration, bool snapping)
	{
		return SparkTweener.Get<SparkTweener>(target.DOMove(new Vector3(x, y, z), duration, snapping));
	}
	public static SparkTweener DOMoveX(Transform target, float endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOMoveX(endValue, duration));
	}
	public static SparkTweener DOMoveX(Transform target, float endValue, float duration, bool snapping)
	{
		return SparkTweener.Get<SparkTweener>(target.DOMoveX(endValue, duration, snapping));
	}
	public static SparkTweener DOMoveY(Transform target, float endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOMoveY(endValue, duration));
	}
	public static SparkTweener DOMoveY(Transform target, float endValue, float duration, bool snapping)
	{
		return SparkTweener.Get<SparkTweener>(target.DOMoveY(endValue, duration, snapping));
	}
	public static SparkTweener DOMoveZ(Transform target, float endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOMoveZ(endValue, duration));
	}
	public static SparkTweener DOMoveZ(Transform target, float endValue, float duration, bool snapping)
	{
		return SparkTweener.Get<SparkTweener>(target.DOMoveZ(endValue, duration, snapping));
	}

	public static SparkTweener DOLocalMove(Transform target, float x, float y, float z, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOLocalMove(new Vector3(x, y, z), duration));
	}
	public static SparkTweener DOLocalMove(Transform target, float x, float y, float z, float duration, bool snapping)
	{
		return SparkTweener.Get<SparkTweener>(target.DOLocalMove(new Vector3(x, y, z), duration, snapping));
	}
	public static SparkTweener DOLocalMoveX(Transform target, float endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOLocalMoveX(endValue, duration));
	}
	public static SparkTweener DOLocalMoveX(Transform target, float endValue, float duration, bool snapping)
	{
		return SparkTweener.Get<SparkTweener>(target.DOLocalMoveX(endValue, duration, snapping));
	}
	public static SparkTweener DOLocalMoveY(Transform target, float endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOLocalMoveY(endValue, duration));
	}
	public static SparkTweener DOLocalMoveY(Transform target, float endValue, float duration, bool snapping)
	{
		return SparkTweener.Get<SparkTweener>(target.DOLocalMoveY(endValue, duration, snapping));
	}
	public static SparkTweener DOLocalMoveZ(Transform target, float endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOLocalMoveZ(endValue, duration));
	}
	public static SparkTweener DOLocalMoveZ(Transform target, float endValue, float duration, bool snapping)
	{
		return SparkTweener.Get<SparkTweener>(target.DOLocalMoveZ(endValue, duration, snapping));
	}

	public static SparkTweener DOScale(Transform target, float x, float y, float z, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOScale(new Vector3(x, y, z), duration));
	}
	public static SparkTweener DOScale(Transform target, float endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOScale(endValue, duration));
	}
	public static SparkTweener DOScaleX(Transform target, float endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOScaleX(endValue, duration));
	}
	public static SparkTweener DOScaleY(Transform target, float endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOScaleY(endValue, duration));
	}
	public static SparkTweener DOScaleZ(Transform target, float endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOScaleZ(endValue, duration));
	}
	public static SparkTweener DORotate(Transform target, float x, float y, float z, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DORotate(new Vector3(x, y, z), duration));
	}
	public static SparkTweener DORotate(Transform target, float x, float y, float z, float duration, Spark.Tweening.RotateMode mode)
	{
		return SparkTweener.Get<SparkTweener>(target.DORotate(new Vector3(x, y, z), duration, (DG.Tweening.RotateMode)mode));
	}
	public static SparkTweener DOLocalRotate(this Transform target, float x, float y, float z, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOLocalRotate(new Vector3(x, y, z), duration));
	}
	public static SparkTweener DOLocalRotate(this Transform target, float x, float y, float z, float duration, Spark.Tweening.RotateMode mode)
	{
		return SparkTweener.Get<SparkTweener>(target.DOLocalRotate(new Vector3(x, y, z), duration, (DG.Tweening.RotateMode)mode));
	}

	// ShakePosition
	public static SparkTweener DOShakePosition(Transform target, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakePosition(duration));
	}
	public static SparkTweener DOShakePosition(Transform target, float duration, float strength)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakePosition(duration, strength));
	}
	public static SparkTweener DOShakePosition(Transform target, float duration, float strength, int vibrato)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakePosition(duration, strength, vibrato));
	}
	public static SparkTweener DOShakePosition(Transform target, float duration, float strength, int vibrato, float randomness)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakePosition(duration, strength, vibrato, randomness));
	}
	public static SparkTweener DOShakePosition(Transform target, float duration, float strength, int vibrato, float randomness, bool fadeOut)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakePosition(duration, strength, vibrato, randomness, false, fadeOut));
	}
	public static SparkTweener DOShakePosition(Transform target, float duration, float strengthX, float strengthY, float strengthZ)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakePosition(duration, new Vector3(strengthX, strengthY, strengthZ)));
	}
	public static SparkTweener DOShakePosition(Transform target, float duration, float strengthX, float strengthY, float strengthZ, int vibrato)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakePosition(duration, new Vector3(strengthX, strengthY, strengthZ), vibrato));
	}
	public static SparkTweener DOShakePosition(Transform target, float duration, float strengthX, float strengthY, float strengthZ, int vibrato, float randomness)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakePosition(duration, new Vector3(strengthX, strengthY, strengthZ), vibrato, randomness));
	}
	public static SparkTweener DOShakePosition(Transform target, float duration, float strengthX, float strengthY, float strengthZ, int vibrato, float randomness, bool fadeOut)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakePosition(duration, new Vector3(strengthX, strengthY, strengthZ), vibrato, randomness, false, fadeOut));
	}

	// ShakeRotation
	public static SparkTweener DOShakeRotation(Transform target, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeRotation(duration));
	}
	public static SparkTweener DOShakeRotation(Transform target, float duration, float strength)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeRotation(duration, strength));
	}
	public static SparkTweener DOShakeRotation(Transform target, float duration, float strength, int vibrato)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeRotation(duration, strength, vibrato));
	}
	public static SparkTweener DOShakeRotation(Transform target, float duration, float strength, int vibrato, float randomness)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeRotation(duration, strength, vibrato, randomness));
	}
	public static SparkTweener DOShakeRotation(Transform target, float duration, float strength, int vibrato, float randomness, bool fadeOut)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeRotation(duration, strength, vibrato, randomness, fadeOut));
	}
	public static SparkTweener DOShakeRotation(Transform target, float duration, float strengthX, float strengthY, float strengthZ)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeRotation(duration, new Vector3(strengthX, strengthY, strengthZ)));
	}
	public static SparkTweener DOShakeRotation(Transform target, float duration, float strengthX, float strengthY, float strengthZ, int vibrato)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeRotation(duration, new Vector3(strengthX, strengthY, strengthZ), vibrato));
	}
	public static SparkTweener DOShakeRotation(Transform target, float duration, float strengthX, float strengthY, float strengthZ, int vibrato, float randomness)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeRotation(duration, new Vector3(strengthX, strengthY, strengthZ), vibrato, randomness));
	}
	public static SparkTweener DOShakeRotation(Transform target, float duration, float strengthX, float strengthY, float strengthZ, int vibrato, float randomness, bool fadeOut)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeRotation(duration, new Vector3(strengthX, strengthY, strengthZ), vibrato, randomness, fadeOut));
	}

	// ShakeScale
	public static SparkTweener DOShakeScale(Transform target, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeScale(duration));
	}
	public static SparkTweener DOShakeScale(Transform target, float duration, float strength)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeScale(duration, strength));
	}
	public static SparkTweener DOShakeScale(Transform target, float duration, float strength, int vibrato)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeScale(duration, strength, vibrato));
	}
	public static SparkTweener DOShakeScale(Transform target, float duration, float strength, int vibrato, float randomness)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeScale(duration, strength, vibrato, randomness));
	}
	public static SparkTweener DOShakeScale(Transform target, float duration, float strength, int vibrato, float randomness, bool fadeOut)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeScale(duration, strength, vibrato, randomness, fadeOut));
	}
	public static SparkTweener DOShakeScale(Transform target, float duration, float strengthX, float strengthY, float strengthZ)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeScale(duration, new Vector3(strengthX, strengthY, strengthZ)));
	}
	public static SparkTweener DOShakeScale(Transform target, float duration, float strengthX, float strengthY, float strengthZ, int vibrato)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeScale(duration, new Vector3(strengthX, strengthY, strengthZ), vibrato));
	}
	public static SparkTweener DOShakeScale(Transform target, float duration, float strengthX, float strengthY, float strengthZ, int vibrato, float randomness)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeScale(duration, new Vector3(strengthX, strengthY, strengthZ), vibrato, randomness));
	}
	public static SparkTweener DOShakeScale(Transform target, float duration, float strengthX, float strengthY, float strengthZ, int vibrato, float randomness, bool fadeOut)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeScale(duration, new Vector3(strengthX, strengthY, strengthZ), vibrato, randomness, fadeOut));
	}

	public static SparkTweener DOPath(Transform target, Vector3[] path, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOPath(path, duration));
	}
	public static SparkTweener DOPath(Transform target, Vector3[] path, float duration, Spark.Tweening.PathType pathType)
	{
		return SparkTweener.Get<SparkTweener>(target.DOPath(path, duration, (DG.Tweening.PathType)pathType));
	}
	public static SparkTweener DOPath(Transform target, Vector3[] path, float duration, Spark.Tweening.PathType pathType, Spark.Tweening.PathMode pathMode)
	{
		return SparkTweener.Get<SparkTweener>(target.DOPath(path, duration, (DG.Tweening.PathType)pathType, (DG.Tweening.PathMode)pathMode));
	}
	public static SparkTweener DOPath(Transform target, Vector3[] path, float duration, Spark.Tweening.PathType pathType, Spark.Tweening.PathMode pathMode, int resolution)
	{
		return SparkTweener.Get<SparkTweener>(target.DOPath(path, duration, (DG.Tweening.PathType)pathType, (DG.Tweening.PathMode)pathMode, resolution));
	}

	public static SparkTweener DOLocalPath(Transform target, Vector3[] path, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOLocalPath(path, duration));
	}
	public static SparkTweener DOLocalPath(Transform target, Vector3[] path, float duration, Spark.Tweening.PathType pathType)
	{
		return SparkTweener.Get<SparkTweener>(target.DOLocalPath(path, duration, (DG.Tweening.PathType)pathType));
	}
	public static SparkTweener DOLocalPath(Transform target, Vector3[] path, float duration, Spark.Tweening.PathType pathType, Spark.Tweening.PathMode pathMode)
	{
		return SparkTweener.Get<SparkTweener>(target.DOLocalPath(path, duration, (DG.Tweening.PathType)pathType, (DG.Tweening.PathMode)pathMode));
	}
	public static SparkTweener DOLocalPath(Transform target, Vector3[] path, float duration, Spark.Tweening.PathType pathType, Spark.Tweening.PathMode pathMode, int resolution)
	{
		return SparkTweener.Get<SparkTweener>(target.DOLocalPath(path, duration, (DG.Tweening.PathType)pathType, (DG.Tweening.PathMode)pathMode, resolution));
	}
	#endregion

	#region RectTransform
	public static SparkTweener DOAnchorMax(RectTransform target, float x, float y, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOAnchorMax(new Vector2(x, y), duration));
	}
	public static SparkTweener DOAnchorMax(RectTransform target, float x, float y, float duration, bool snapping)
	{
		return SparkTweener.Get<SparkTweener>(target.DOAnchorMax(new Vector2(x, y), duration, snapping));
	}
	public static SparkTweener DOAnchorMin(RectTransform target, float x, float y, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOAnchorMin(new Vector2(x, y), duration));
	}
	public static SparkTweener DOAnchorMin(RectTransform target, float x, float y, float duration, bool snapping)
	{
		return SparkTweener.Get<SparkTweener>(target.DOAnchorMin(new Vector2(x, y), duration, snapping));
	}

	public static SparkTweener DOAnchorPos(RectTransform target, float x, float y, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOAnchorPos(new Vector2(x, y), duration));
	}
	public static SparkTweener DOAnchorPos(RectTransform target, float x, float y, float duration, bool snapping)
	{
		return SparkTweener.Get<SparkTweener>(target.DOAnchorPos(new Vector2(x, y), duration, snapping));
	}
	public static SparkTweener DOAnchorPosX(RectTransform target, float endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOAnchorPosX(endValue, duration));
	}
	public static SparkTweener DOAnchorPosX(RectTransform target, float endValue, float duration, bool snapping)
	{
		return SparkTweener.Get<SparkTweener>(target.DOAnchorPosX(endValue, duration, snapping));
	}
	public static SparkTweener DOAnchorPosY(RectTransform target, float endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOAnchorPosY(endValue, duration));
	}
	public static SparkTweener DOAnchorPosY(RectTransform target, float endValue, float duration, bool snapping)
	{
		return SparkTweener.Get<SparkTweener>(target.DOAnchorPosY(endValue, duration, snapping));
	}

	// DOJumpAnchorPos
	public static SparkTweener DOJumpAnchorPos(RectTransform target, float endX, float endY, float jumpPower, int numJumps, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOJumpAnchorPos(new Vector2(endX, endY), jumpPower, numJumps, duration));
	}
	public static SparkTweener DOJumpAnchorPos(RectTransform target, float endX, float endY, float jumpPower, int numJumps, float duration, bool snapping)
	{
		return SparkTweener.Get<SparkTweener>(target.DOJumpAnchorPos(new Vector2(endX, endY), jumpPower, numJumps, duration, snapping));
	}

	// DOPunchAnchorPos
	public static SparkTweener DOPunchAnchorPos(RectTransform target, float punchX, float punchY, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOPunchAnchorPos(new Vector2(punchX, punchY), duration));
	}
	public static SparkTweener DOPunchAnchorPos(RectTransform target, float punchX, float punchY, float duration, int vibrato)
	{
		return SparkTweener.Get<SparkTweener>(target.DOPunchAnchorPos(new Vector2(punchX, punchY), duration, vibrato));
	}
	public static SparkTweener DOPunchAnchorPos(RectTransform target, float punchX, float punchY, float duration, int vibrato, float elasticity)
	{
		return SparkTweener.Get<SparkTweener>(target.DOPunchAnchorPos(new Vector2(punchX, punchY), duration, vibrato, elasticity));
	}
	public static SparkTweener DOPunchAnchorPos(RectTransform target, float punchX, float punchY, float duration, int vibrato, float elasticity, bool snapping)
	{
		return SparkTweener.Get<SparkTweener>(target.DOPunchAnchorPos(new Vector2(punchX, punchY), duration, vibrato, elasticity, snapping));
	}

	// DOShakeAnchorPos
	public static SparkTweener DOShakeAnchorPos(RectTransform target, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeAnchorPos(duration));
	}
	public static SparkTweener DOShakeAnchorPos(RectTransform target, float duration, float strength)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeAnchorPos(duration, strength));
	}
	public static SparkTweener DOShakeAnchorPos(RectTransform target, float duration, float strength, int vibrato)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeAnchorPos(duration, strength, vibrato));
	}
	public static SparkTweener DOShakeAnchorPos(RectTransform target, float duration, float strength, int vibrato, float randomness)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeAnchorPos(duration, strength, vibrato, randomness));
	}
	public static SparkTweener DOShakeAnchorPos(RectTransform target, float duration, float strength, int vibrato, float randomness, bool snapping)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeAnchorPos(duration, strength, vibrato, randomness, snapping));
	}
	public static SparkTweener DOShakeAnchorPos(RectTransform target, float duration, float strength, int vibrato, float randomness, bool snapping, bool fadeOut)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeAnchorPos(duration, strength, vibrato, randomness, snapping, fadeOut));
	}

	// DOShakeAnchorPos
	public static SparkTweener DOShakeAnchorPos(RectTransform target, float duration, float strengthX, float strengthY)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeAnchorPos(duration, new Vector2(strengthX, strengthY)));
	}
	public static SparkTweener DOShakeAnchorPos(RectTransform target, float duration, float strengthX, float strengthY, int vibrato)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeAnchorPos(duration, new Vector2(strengthX, strengthY), vibrato));
	}
	public static SparkTweener DOShakeAnchorPos(RectTransform target, float duration, float strengthX, float strengthY, int vibrato, float randomness)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeAnchorPos(duration, new Vector2(strengthX, strengthY), vibrato, randomness));
	}
	public static SparkTweener DOShakeAnchorPos(RectTransform target, float duration, float strengthX, float strengthY, int vibrato, float randomness, bool snapping)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeAnchorPos(duration, new Vector2(strengthX, strengthY), vibrato, randomness, snapping));
	}
	public static SparkTweener DOShakeAnchorPos(RectTransform target, float duration, float strengthX, float strengthY, int vibrato, float randomness, bool snapping, bool fadeOut)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeAnchorPos(duration, new Vector2(strengthX, strengthY), vibrato, randomness, snapping, fadeOut));
	}

	public static SparkTweener DOPivot(RectTransform target, float x, float y, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOPivot(new Vector2(x, y), duration));
	}
	public static SparkTweener DOPivotX(RectTransform target, float endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOPivotX(endValue, duration));
	}
	public static SparkTweener DOPivotY(RectTransform target, float endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOPivotY(endValue, duration));
	}

	public static SparkTweener DOSizeDelta(RectTransform target, float x, float y, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOSizeDelta(new Vector2(x, y), duration));
	}
	public static SparkTweener DOSizeDelta(RectTransform target, float x, float y, float duration, bool snapping)
	{
		return SparkTweener.Get<SparkTweener>(target.DOSizeDelta(new Vector2(x, y), duration, snapping));
	}
	#endregion

	#region AudioSource
	public static SparkTweener DOFade(AudioSource target, float endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOFade(endValue, duration));
	}
	public static SparkTweener DOPitch(AudioSource target, float endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOPitch(endValue, duration));
	}
	#endregion

	#region Camera
	public static SparkTweener DOAspect(Camera target, float endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOAspect(endValue, duration));
	}
	public static SparkTweener DOColor(Camera target, Color endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOColor(endValue, duration));
	}
	public static SparkTweener DOColor(Camera target, float r, float g, float b, float a, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOColor(new Color(r, g, b, a), duration));
	}
	public static SparkTweener DONearClipPlane(Camera target, float endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DONearClipPlane(endValue, duration));
	}
	public static SparkTweener DOFarClipPlane(Camera target, float endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOFarClipPlane(endValue, duration));
	}
	public static SparkTweener DOFieldOfView(Camera target, float endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOFieldOfView(endValue, duration));
	}
	public static SparkTweener DOOrthoSize(Camera target, float endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOOrthoSize(endValue, duration));
	}
	// ShakePosition
	public static SparkTweener DOShakePosition(Camera target, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakePosition(duration));
	}
	public static SparkTweener DOShakePosition(Camera target, float duration, float strength)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakePosition(duration, strength));
	}
	public static SparkTweener DOShakePosition(Camera target, float duration, float strength, int vibrato)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakePosition(duration, strength, vibrato));
	}
	public static SparkTweener DOShakePosition(Camera target, float duration, float strength, int vibrato, float randomness)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakePosition(duration, strength, vibrato, randomness));
	}
	public static SparkTweener DOShakePosition(Camera target, float duration, float strength, int vibrato, float randomness, bool fadeOut)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakePosition(duration, strength, vibrato, randomness, fadeOut));
	}
	public static SparkTweener DOShakePosition(Camera target, float duration, float strengthX, float strengthY, float strengthZ)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakePosition(duration, new Vector3(strengthX, strengthY, strengthZ)));
	}
	public static SparkTweener DOShakePosition(Camera target, float duration, float strengthX, float strengthY, float strengthZ, int vibrato)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakePosition(duration, new Vector3(strengthX, strengthY, strengthZ), vibrato));
	}
	public static SparkTweener DOShakePosition(Camera target, float duration, float strengthX, float strengthY, float strengthZ, int vibrato, float randomness)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakePosition(duration, new Vector3(strengthX, strengthY, strengthZ), vibrato, randomness));
	}
	public static SparkTweener DOShakePosition(Camera target, float duration, float strengthX, float strengthY, float strengthZ, int vibrato, float randomness, bool fadeOut)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakePosition(duration, new Vector3(strengthX, strengthY, strengthZ), vibrato, randomness, fadeOut));
	}

	// ShakeRotation
	public static SparkTweener DOShakeRotation(Camera target, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeRotation(duration));
	}
	public static SparkTweener DOShakeRotation(Camera target, float duration, float strength)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeRotation(duration, strength));
	}
	public static SparkTweener DOShakeRotation(Camera target, float duration, float strength, int vibrato)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeRotation(duration, strength, vibrato));
	}
	public static SparkTweener DOShakeRotation(Camera target, float duration, float strength, int vibrato, float randomness)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeRotation(duration, strength, vibrato, randomness));
	}
	public static SparkTweener DOShakeRotation(Camera target, float duration, float strength, int vibrato, float randomness, bool fadeOut)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeRotation(duration, strength, vibrato, randomness, fadeOut));
	}
	public static SparkTweener DOShakeRotation(Camera target, float duration, float strengthX, float strengthY, float strengthZ)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeRotation(duration, new Vector3(strengthX, strengthY, strengthZ)));
	}
	public static SparkTweener DOShakeRotation(Camera target, float duration, float strengthX, float strengthY, float strengthZ, int vibrato)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeRotation(duration, new Vector3(strengthX, strengthY, strengthZ), vibrato));
	}
	public static SparkTweener DOShakeRotation(Camera target, float duration, float strengthX, float strengthY, float strengthZ, int vibrato, float randomness)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeRotation(duration, new Vector3(strengthX, strengthY, strengthZ), vibrato, randomness));
	}
	public static SparkTweener DOShakeRotation(Camera target, float duration, float strengthX, float strengthY, float strengthZ, int vibrato, float randomness, bool fadeOut)
	{
		return SparkTweener.Get<SparkTweener>(target.DOShakeRotation(duration, new Vector3(strengthX, strengthY, strengthZ), vibrato, randomness, fadeOut));
	}
	#endregion

	#region TailRenderer
	public static SparkTweener DOTime(TrailRenderer target, float endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOTime(endValue, duration));
	}
    #endregion

    #region CanvasGroup
    public static SparkTweener DOFade(CanvasGroup target, float endValue, float duration) 
    {
        return SparkTweener.Get<SparkTweener>(target.DOFade(endValue, duration));
    }
    #endregion
    #region Text
	public static SparkTweener DOText(Text target, string endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOText(endValue, duration));
	}
	public static SparkTweener DOColor(Text target, Color endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOColor(endValue, duration));
	}
	public static SparkTweener DOFade(Text target, float endValue, float duration)
	{
		return SparkTweener.Get<SparkTweener>(target.DOFade(endValue, duration));
	}
    #endregion
    #region Image
    public static SparkTweener DOFillAmount(Image target, float endValue, float duration)
    {
        return SparkTweener.Get<SparkTweener>(target.DOFillAmount(endValue, duration));
    }
    public static SparkTweener DOFade(Image target, float endValue, float duration)
    {
        return SparkTweener.Get<SparkTweener>(target.DOFade(endValue, duration));
    }
    #endregion
}

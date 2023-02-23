using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using UnityEngine;

namespace Spark
{
	[AddComponentMenu("Spark/View Open Animation")]
	public class UIViewOpenAnimation : DOTweenAnimation, IResettable
	{
		void IResettable.Reset()
		{
#if UNITY_EDITOR
			autoKill = false;
			autoPlay = false;
#endif
		}
	}
}
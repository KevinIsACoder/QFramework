using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using UnityEngine;

namespace Spark
{
	[AddComponentMenu("Spark/View Close Animation")]
	public class UIViewCloseAnimation : DOTweenAnimation, IResettable
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

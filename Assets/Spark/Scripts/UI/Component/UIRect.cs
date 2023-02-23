using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Spark
{
	[XLua.LuaCallCSharp]
	[RequireComponent(typeof(CanvasRenderer))]
	[AddComponentMenu("Spark/Rect", 40)]
	public class UIRect : MaskableGraphic
	{
		protected UIRect()
		{
			useLegacyMeshGeneration = false;
            raycastTarget = true;
        }

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();
		}
	}
}

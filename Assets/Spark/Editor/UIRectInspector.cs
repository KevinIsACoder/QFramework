using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace SparkEditor
{
	[CustomEditor(typeof(Spark.UIRect), true)]
	public class UIRectInspector : ComponentEditor
	{
		protected override void DrawCustomProperties()
		{
		}
	}
}

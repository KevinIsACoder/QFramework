using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace SparkEditor
{
	public class ComponentEditor : Editor
	{
		/// <summary>
		/// Draw the inspector properties.
		/// </summary>

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUI.BeginDisabledGroup(!ShouldDrawProperties());
			DrawCustomProperties();
			EditorGUI.EndDisabledGroup();

			serializedObject.ApplyModifiedProperties();
		}

		protected virtual bool ShouldDrawProperties()
		{
			return true;
		}
		protected virtual void DrawCustomProperties()
		{
		}

	}
}

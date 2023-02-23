using System;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace Spark
{
	[XLua.LuaCallCSharp]
	sealed public class UIComponentCollection : MonoBehaviour
	{
		[SerializeField]
		internal List<Component> components = new List<Component>();

		[BlackList]
		public T Get<T>(int index) where T : Component
		{
			return (T)components[index];
		}

		public Component Get(int index)
        {
			if (index >= components.Count) return null;
			return components[index];
        }
	}
}

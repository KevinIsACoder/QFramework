using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spark
{
	/// <summary>
	/// 界面的参数数据类
	/// </summary>
	[XLua.LuaCallCSharp]
	public class UIContext
	{
		static private readonly ObjectPool<UIContext> m_ContextPool;

		static UIContext()
		{
			m_ContextPool = new ObjectPool<UIContext>(() => new UIContext(), null, (context) => {
#if USE_LUA
				context.m_LuaValues = null;
#else
				context.m_ValueDict.Clear();
#endif
			});
		}

		public static UIContext Get()
		{
			return m_ContextPool.Get();
		}

#if USE_LUA
		public static UIContext Get(XLua.LuaTable values)
		{
			UIContext context = Get();
			context.m_LuaValues = values;
			return context;
		}
#else
		[XLua.BlackList]
		public static UIContext Get(Dictionary<string, object> values)
		{
			UIContext context = Get();
			if (values != null) {
				var map = context.m_ValueDict;
				foreach (var kv in values) {
					map[kv.Key] = kv.Value;
				}
			}
			return context;
		}
#endif

		internal static void Release(UIContext context)
		{
			if (context != null) {
				m_ContextPool.Release(context);
			}
		}

#if USE_LUA
		private XLua.LuaTable m_LuaValues;
#else
		private Dictionary<string, object> m_ValueDict;
#endif

		private UIContext()
		{
#if !USE_LUA
			m_ValueDict = new Dictionary<string, object>();
#endif
		}

		public T Get<T>(string name)
		{
#if USE_LUA
			if (m_LuaValues != null)
            {
				return m_LuaValues.Get<T>(name);
			}
#else
			object value;
			if (m_ValueDict.TryGetValue(name, out value)) {
				return (T)value;
			}
#endif
			return default(T);
		}

		public bool TryGet<T>(string name, out T value)
		{
#if USE_LUA
			if (m_LuaValues != null && m_LuaValues.ContainsKey(name))
            {
				value = m_LuaValues.Get<T>(name);
				return true;
            }
#else
			object ret;
			if (m_ValueDict.TryGetValue(name, out ret)) {
				value = (T)ret;
				return true;
			}
#endif
			value = default(T);
			return false;
		}

		public object Get(string name)
		{
			return Get<object>(name);
		}

		public bool TryGet(string name, out object value)
		{
			return TryGet<object>(name, out value);
		}

		public UIContext Set(string name, object value)
		{
#if USE_LUA
			if (m_LuaValues != null)
			{
				m_LuaValues.Set<string, object>(name, value);
			}
#else
			m_ValueDict[name] = value;
#endif
			return this;
		}

		public UIContext Remove(string name)
		{
#if USE_LUA
			if (m_LuaValues != null)
            {
				m_LuaValues.Set<string, object>(name, null);
            }
#else
			if (m_ValueDict.ContainsKey(name))
				m_ValueDict.Remove(name);
#endif
			return this;
		}

		public bool Contains(string name)
		{
#if USE_LUA
			return m_LuaValues != null && m_LuaValues.ContainsKey(name);
#else
			return m_ValueDict.ContainsKey(name);
#endif
		}
	}
}

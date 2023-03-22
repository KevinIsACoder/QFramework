/*
 * @Author: zhendong liang
 * @Date: 2022-08-24 15:56:43
 * @LastEditors: Do not edit
 * @LastEditTime: 2022-08-24 18:00:32
 * @Description: 对象池管理类
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QFramWork
{
	public class ObjectPool<T> : Disposable
	{
		private readonly Stack<T> m_Stack;
		private readonly HashSet<T> m_HashSet;
		private readonly Func<T> m_ActionOnCreate;
		private readonly Action<T> m_ActionOnGet;
		private readonly Action<T> m_ActionOnRelease;

		public ObjectPool(Func<T> actionOnCreate) : this(actionOnCreate, null, null)
		{
		}

		public ObjectPool(Func<T> actionOnCreate, Action<T> actionOnGet, Action<T> actionOnRelease)
		{
			m_Stack = new Stack<T>();
			m_HashSet = new HashSet<T>();
			m_ActionOnGet = actionOnGet;
			m_ActionOnCreate = actionOnCreate;
			m_ActionOnRelease = actionOnRelease;
		}

		public T Get()
		{
			T element;
			if (m_Stack.Count == 0) {
				element = m_ActionOnCreate();
			} else {
				element = m_Stack.Pop();
				m_HashSet.Remove(element);
			}

			if (m_ActionOnGet != null)
				m_ActionOnGet(element);

			return element;
		}

		public void Release(T element)
		{
			if (m_HashSet.Add(element)) {
				if (m_ActionOnRelease != null)
					m_ActionOnRelease(element);

				m_Stack.Push(element);
			}
		}

		protected override void DisposeManagedObjects()
		{
			m_Stack.Clear();
			m_HashSet.Clear();
		}
	}

	public class ClassPool<T> : ObjectPool<T> where T : class, new()
	{
		public ClassPool() : this(null, null)
		{
		}

		public ClassPool(Action<T> actionOnGet, Action<T> actionOnRelease) : base(() => new T(), actionOnGet, actionOnRelease)
		{
		}
	}
}


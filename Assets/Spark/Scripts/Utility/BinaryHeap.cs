using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spark
{
	public class BinaryHeap<T>
	{
		private int m_Count;
		private T[] m_Elements;
		private Comparison<T> m_Comparison;

		public BinaryHeap(Comparison<T> comparison)
		{
			m_Count = 0;
			// element 0 is noop
			m_Elements = new T[1];
			m_Comparison = comparison;
		}

		public int count
		{
			get
			{
				return m_Count;
			}
		}

		public void Insert(T element)
		{
			// +1 as element 0 is noop
			if (m_Elements.Length == m_Count + 1) {
				Array.Resize(ref m_Elements, m_Elements.Length * 2);
			}
			m_Elements[++m_Count] = element;
			HeapUp(m_Count);
		}

		public void Remove(T element)
		{
			for (int i = 1; i <= m_Count; i++) {
				if (m_Elements[i].Equals(element)) {
					RemoveAt(i);
					return;
				}
			}
		}

		private void RemoveAt(int index)
		{
			m_Elements[index] = m_Elements[m_Count];
			m_Elements[m_Count] = default(T);
			if (--m_Count > 0 && index <= m_Count) {
				if (index > 1) {
					var ret = Compare(m_Elements[index], m_Elements[index / 2]);
					if (ret < 0) {
						HeapUp(index);
					} else if (ret > 0) {
						HeapDown(index);
					}
				} else {
					HeapDown(index);
				}
			}
		}

		public T Pop()
		{
			var element = Peek();
			RemoveAt(1);
			return element;
		}

		public T Peek()
		{
			if (m_Count == 0) {
				throw new InvalidOperationException("InvalidOperationException: Operation is not valid due to the current state of the object");
			}
			return m_Elements[1];
		}

		private int Compare(T a, T b)
		{
			if (m_Comparison != null) {
				return m_Comparison(a, b);
			}
			return 0;
		}

		private void HeapUp(int index)
		{
			int hole = index;
			T element = m_Elements[hole];
			while (hole > 1 && Compare(element, m_Elements[hole / 2]) < 0) {
				int next = hole / 2;
				m_Elements[hole] = m_Elements[next];
				hole = next;
			}
			m_Elements[hole] = element;
		}

		private void HeapDown(int index)
		{
			T element = m_Elements[index];
			int hole = index;

			while ((hole * 2) <= m_Count) {
				int child = hole * 2;
				if (child != m_Count && Compare(m_Elements[child + 1], m_Elements[child]) < 0) {
					child++;
				}
				if (Compare(m_Elements[child], element) >= 0) {
					break;
				}
				m_Elements[hole] = m_Elements[child];
				hole = child;
			}
			m_Elements[hole] = element;
		}

		public void Clear()
		{
			m_Count = 0;
			m_Elements = new T[m_Elements.Length];  // for gc
		}

		public bool Contains(T element)
		{
			for (int i = 1; i <= m_Count; i++) {
				if (m_Elements[i].Equals(element)) {
					return true;
				}
			}
			return false;
		}
	}
}

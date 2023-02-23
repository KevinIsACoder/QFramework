using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spark 
{
	[DisallowMultipleComponent]
	[XLua.LuaCallCSharp]
	public class UITableViewRecycledContainer : UIRecycledContainer 
	{
		private List<UITableViewCell> m_RecycledCells = new List<UITableViewCell>();

		public UITableViewCell DequeueCell(string ident)
		{
			for (int i = 0; i < m_RecycledCells.Count; i++) {
				UITableViewCell cell = m_RecycledCells[i];
				if (ident == cell.identifier) {
					m_RecycledCells.RemoveAt(i);
					base.DequeueCell(cell.gameObject);
					return cell;
				}
			}
			return null;
		}

		public void RecycleCell(UITableViewCell cell)
		{
			m_RecycledCells.Add(cell);
			cell.active = false;

			base.RecycleCell(cell.gameObject);
		}
	}
}

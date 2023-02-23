using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Spark 
{
	[XLua.LuaCallCSharp]
	public class UIRecycledContainer : MonoBehaviour 
	{
        protected enum InvisibleMode
		{
			SetActive,
			SetLayer
		}

        [SerializeField]
		protected InvisibleMode m_InvisibleMode = InvisibleMode.SetActive;
        [SerializeField]
		private int m_InvisibleLayer = 0;
		[SerializeField]
		private int m_VisibleLayer = 0;

        void Awake()
        {
            if (m_InvisibleMode == InvisibleMode.SetLayer) {
                gameObject.layer = m_InvisibleLayer;
            }
        }

        protected void DequeueCell(GameObject cell)
		{
            SetVisible(cell, true);
		}

		protected void RecycleCell(GameObject cell)
		{
			UITableView[] tableViews = cell.GetComponentsInChildren<UITableView>(true);
			foreach (UITableView tv in tableViews) {
				tv.Clear(true);
			}
			cell.transform.SetParent(transform, false);
			SetVisible(cell, false);
		}

        private void SetVisible(GameObject cell, bool visible)
		{
			if (m_InvisibleMode == InvisibleMode.SetActive) {
				cell.SetActive(visible);
			} else if (m_InvisibleMode == InvisibleMode.SetLayer) {
				SparkHelper.SetLayer(cell, visible ? m_VisibleLayer : m_InvisibleLayer);
				var raycaster = cell.GetComponent<GraphicRaycaster>();
				if (raycaster != null) {
					raycaster.enabled = visible;
                }
			}
		}
	}
}

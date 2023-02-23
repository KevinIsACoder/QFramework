using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Spark
{
	[XLua.LuaCallCSharp]
	public class UITableViewCell : UITableViewCellTrigger
	{
		internal enum State
		{
			Normal,
			Dimmed,
			Selected,
			Disabled,
		}

		public enum Mode
		{
			Fixed, CalcOnce, CalcEvery
		}

		[System.NonSerialized]
		internal UITableView tableView;

		[SerializeField]
		public string identifier;

		[SerializeField]
		internal Mode mode;

		[NonSerialized]
		internal int dataIndex;

		[NonSerialized]
		internal int cellIndex;

		[NonSerialized]
		internal bool active;

		[SerializeField]
		private GameObject m_NormalState = null;

		[SerializeField]
		private GameObject m_DimmedState = null;

		[SerializeField]
		private GameObject m_SelectedState = null;

		[SerializeField]
		private GameObject m_DisabledState = null;

		private State m_State = State.Normal;
		private GameObject[] m_StateObjects = null;

		internal State state
		{
			get
			{
				return m_State;
			}
			set
			{
				if (m_State == value)
					return;
				SetStateActive(m_State, false);
				m_State = value;
				SetStateActive(m_State, true);
			}
		}

		void Awake()
		{
			m_StateObjects = new GameObject[4] { m_NormalState, m_DimmedState, m_SelectedState, m_DisabledState };
		}

		void Start()
		{
			tableViewCell = this;

			foreach (Selectable comp in GetComponentsInChildren<Selectable>(true)) {
				var trigger = comp.gameObject.GetComponent<UITableViewCellTrigger>();
				if (trigger == null) {
					trigger = comp.gameObject.AddComponent<UITableViewCellTrigger>();
				}
				trigger.tableViewCell = this;
			}

			for (int i = 0; i < 4; i++) {
				GameObject go = m_StateObjects[i];
				if (go == null)
					continue;
				go.SetActive(false);
			}
			SetStateActive(m_State, true);
		}

		private void SetStateActive(State state, bool active)
		{
			GameObject go = m_StateObjects[(int)state];
			if (go != null) {
				go.SetActive(active);
			}
		}

		[XLua.BlackList]
		public override void OnPointerClick(PointerEventData eventData)
		{
			tableView.HandleClick(tableViewCell, gameObject, true);
		}
	}

	[XLua.LuaCallCSharp]
	public class UITableViewCellTrigger : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
	{
		[System.NonSerialized]
		internal UITableViewCell tableViewCell;

		[XLua.BlackList]
		public virtual void OnPointerClick(PointerEventData eventData)
		{
			tableViewCell.tableView.HandleClick(tableViewCell, gameObject, false);
		}

		[XLua.BlackList]
		public void OnPointerDown(PointerEventData eventData)
		{
			tableViewCell.tableView.HandlePress(true, tableViewCell, gameObject);
		}

		[XLua.BlackList]
		public void OnPointerUp(PointerEventData eventData)
		{
			tableViewCell.tableView.HandlePress(false, tableViewCell, gameObject);
		}
	}
}

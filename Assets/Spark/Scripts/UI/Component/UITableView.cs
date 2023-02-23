using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

namespace Spark
{
	[XLua.LuaCallCSharp]
	public class UITableView : UIScroller, IControl, IPointerClickHandler
	{
		struct Data
		{
			public bool dirty;
			public Rect rect;
			public string ident;
			public object userData;
			public UITableViewCell.State state;
		}

		#region events
		public delegate void OnCellEvent(UITableView table, UITableViewCell cell, object data);
		public delegate void OnCellTouchEvent(UITableView table, UITableViewCell cell, GameObject target, object data);
		public delegate void OnDataEvent(UITableView tableView, object data);
		public delegate void OnSelectionChanged(UITableView table);
		public delegate void OnSnapCompleted(UITableView table, int lineIndex);
		// public delegate void OnCellTextLink(UITableView table, UITableViewCell cell, UIText text, string value);

		public OnCellEvent onCellInit;
		public OnCellEvent onCellShown;
		public OnCellEvent onCellStateChanged;
		public OnCellTouchEvent onCellClick;
		public OnCellTouchEvent onCellPress;
		public OnCellTouchEvent onCellDelayPress;
		public OnCellTouchEvent onCellLongPress;
		public OnDataEvent onSelected;
		public OnDataEvent onDeselected;
		public OnSelectionChanged onSelectionChanged;
		public OnSnapCompleted onSnapCompleted;
		// public OnCellTextLink onCellTextLink;
		
		#endregion

		#region serialized fields
		[SerializeField]
		private List<UITableViewCell> m_CellList = new List<UITableViewCell>();

		[SerializeField]
		private TextAnchor m_Alignment = TextAnchor.UpperLeft;

		[SerializeField]
		private TextAnchor m_ChildAlignment = TextAnchor.UpperLeft;

		[SerializeField]
		private RectOffset m_Padding = new RectOffset();

		[SerializeField]
		private Vector2 m_Spacing = new Vector2();

		[SerializeField]
		private int m_MaxPerLine = 1;

		[SerializeField]
		private bool m_Cancelable = false;

		public bool cancelable
		{
			get
			{
				return m_Cancelable;
			}
			set
			{
				m_Cancelable = value;
			}
		}

		[SerializeField]
		private int m_MaxSelection = 1;

		public int maxSelection
		{
			get
			{
				return m_MaxSelection;
			}
			set
			{
				if (value < 0 || m_MaxSelection == value)
					return;
				m_MaxSelection = value;
			}
		}

		// 自动吸附
		[SerializeField]
		private bool m_Snapping = false;

		[SerializeField]
		private bool m_SnapCellCenter = false; // Only used when m_Snapping is enabled

		[SerializeField]
		private float m_SnapVelocityThreshold = 5f; // Only used when m_Snapping is enabled

		// 循环滚动
		[SerializeField]
		private bool m_Loop = false;

		[SerializeField]
		private int m_DelayFrames = 0;

		#endregion

		#region private and protected fields
		private Dictionary<string, UITableViewCell> m_CellPrefabs = new Dictionary<string, UITableViewCell>();
		private Dictionary<string, Bounds> m_CellPrefabBounds = new Dictionary<string, Bounds>();

		private Vector2 m_SnapOffset = Vector2.zero;

		// 偏移，用于对齐
		private Vector2 m_LayoutOffset = Vector2.zero;

		[SerializeField, HideInInspector]
		private RectTransform m_Container = null;
		[SerializeField]
		private UITableViewRecycledContainer m_RecycledContainer = null;
		private UITableViewRecycledContainer m_InternalRecycledContainer = null;

		private List<Data> m_Datas = new List<Data>();
		private List<UITableViewCell> m_ActiveCells = new List<UITableViewCell>();

		private int m_ActiveCellsStartIndex = -1;
		private int m_ActiveCellsEndIndex = -1;

		private float m_ScrollPosition = 0;
		public float scrollPosition
		{
			get
			{
				return m_ScrollPosition;
			}
			set
			{
				m_ScrollPosition = Mathf.Clamp(value, 0, m_StoredMaxScrollPosition);
				if (m_Direction == Direction.Vertical) {
					normalizedPosition = 1 - (m_ScrollPosition / m_StoredMaxScrollPosition);
				} else {
					normalizedPosition = (m_ScrollPosition / m_StoredMaxScrollPosition);
				}
			}
		}

		public float scrollRectSize
		{
			get
			{
				return m_RectTransform.rect.size[(int)m_Direction];
			}
		}
		#endregion

		#region properties
		public int dataCount
		{
			get
			{
				return m_Datas.Count;
			}
		}

		private List<int> m_SelectedIndices = new List<int>();

		public int selectedCount
		{
			get
			{
				return m_SelectedIndices.Count;
			}
		}

		public object selectedItem
		{
			get
			{
				return m_SelectedIndices.Count > 0 ? m_Datas[m_SelectedIndices[0]].userData : null;
			}
		}

		public List<object> selectedItems
		{
			get
			{
				return m_SelectedIndices.ConvertAll<object>((i) => {
					return m_Datas[i].userData;
				});
			}
		}

		public int selectedIndex
		{
			get
			{
				return m_SelectedIndices.Count > 0 ? m_SelectedIndices[0] : -1;
			}
		}

		public List<int> selectedIndices
		{
			get
			{
				return new List<int>(m_SelectedIndices);
			}
		}

		public UITableViewRecycledContainer recycledContainer
		{
			get
			{
				if (m_RecycledContainer == null) {
					GameObject go = new GameObject("RecycledContainer", typeof(RectTransform), typeof(UITableViewRecycledContainer));
					m_InternalRecycledContainer = go.GetComponent<UITableViewRecycledContainer>();
					m_RecycledContainer = m_InternalRecycledContainer;
					RectTransform rt = go.GetComponent<RectTransform>();
					rt.SetParent(m_RectTransform, false);
					rt.anchorMin = Vector2.zero;
					rt.anchorMax = Vector2.one;
					rt.pivot = new Vector2(0f, 1f);
					rt.offsetMax = rt.offsetMin = Vector3.zero;
					rt.localScale = Vector3.one;
					rt.gameObject.SetActive(false);
				}
				return m_RecycledContainer;
			}
			set
			{
				m_RecycledContainer = value;
				if (value == null) {
					m_RecycledContainer = m_InternalRecycledContainer;
				}
			}
		}
		#endregion

		#region public functions
		private bool m_DataDirty;
		public void AddData(object data)
		{
			AddData(data, m_CellList[0].identifier);
		}

		public void AddData(object data, string identifier)
		{
			m_Datas.Add(new Data() { userData = data, ident = identifier, dirty = true });
			m_DataDirty = true;
		}
		//public void AddDataList(object[] data, string identifier)
		//{
		//	m_Datas.Add(new Data() { userData = data, ident = identifier, dirty = true });
		//	m_DataDirty = true;
		//}
		public void AddDataAt(int index, object data)
		{
			AddDataAt(index, data, m_CellList[0].identifier);
		}
		public void AddDataAt(int index, object data, string identifier)
		{
			if (index < 0 || index > m_Datas.Count)
				return;

			m_Datas.Insert(index, new Data() { userData = data, ident = identifier, dirty = true });
			m_DataDirty = true;

			if (index < m_Datas.Count - 1) {
				for (int i = m_SelectedIndices.Count - 1; i >= 0; i--) {
					if (m_SelectedIndices[i] >= index) {
						m_SelectedIndices[i]++;
					}
				}
				m_ActiveCells.ForEach((cell) => {
					if (cell.dataIndex >= index) {
						cell.dataIndex++;
					}
				});
			}
		}

		public void SetData(object oldData, object newData)
		{
			SetData(oldData, newData, m_CellList[0].identifier);
		}
		public void SetData(object oldData, object newData, string identifier)
		{
			SetDataAt(m_Datas.FindIndex((v) => object.Equals(v.userData, oldData)), newData, identifier);
		}
		public void SetDataAt(int index, object newData)
		{
			SetDataAt(index, newData, m_CellList[0].identifier);
		}
		public void SetDataAt(int index, object newData, string identifier)
		{
			if (index < 0 || index >= m_Datas.Count)
				return;

			var data = m_Datas[index];
			data.userData = newData;
			data.dirty = true;
			data.ident = identifier;
			m_Datas[index] = data;

			m_DataDirty = true;
		}

		public void RemoveData(object data)
		{
			RemoveDataAt(m_Datas.FindIndex((v) => object.Equals(v.userData, data)));
		}
		public void RemoveDataAt(int dataIndex)
		{
			if (dataIndex < 0 || dataIndex >= m_Datas.Count)
				return;

			m_Datas.RemoveAt(dataIndex);
			m_DataDirty = true;

			for (int i = m_SelectedIndices.Count - 1; i >= 0; i--) {
				int selectedIndex = m_SelectedIndices[i];
				if (selectedIndex == dataIndex) {
					m_SelectedIndices.RemoveAt(i);
				} else if (selectedIndex > dataIndex) {
					m_SelectedIndices[i] = selectedIndex - 1;
				}
			}
			for (int i = m_ActiveCells.Count - 1; i >= 0; i--) {
				var cell = m_ActiveCells[i];
				if (cell.dataIndex == dataIndex) {
					RecycleCell(cell);
				} else if (cell.dataIndex > dataIndex) {
					cell.dataIndex--;
				}
			}
		}

		public object GetData(UITableViewCell cell)
		{
			return GetDataAt(cell.dataIndex);
		}
		public object GetDataAt(int dataIndex)
		{
			if (dataIndex < 0 || dataIndex >= m_Datas.Count)
				return null;
			return m_Datas[dataIndex].userData;
		}

		public void SetDisable(object data, bool disabled)
		{
			SetDisable(data, disabled, true);
		}
		public void SetDisable(object data, bool disabled, bool fireEvent)
		{
			SetDisableAt(GetDataIndex(data), disabled, fireEvent);
		}
		public void SetDisableAt(int dataIndex, bool disabled)
		{
			SetDisableAt(dataIndex, disabled, true);
		}
		public void SetDisableAt(int dataIndex, bool disabled, bool fireEvent)
		{
			if (dataIndex >= 0 && dataIndex < m_Datas.Count) {
				var data = m_Datas[dataIndex];
				var state = disabled ? UITableViewCell.State.Disabled : UITableViewCell.State.Normal;
				if (data.state == UITableViewCell.State.Selected || data.state == state)
					return;

				SetState(dataIndex, state, fireEvent);
			}
		}

		public void SetSelect(object data, bool selected)
		{
			SetSelect(data, selected, true);
		}
		public void SetSelect(object data, bool selected, bool fireEvent)
		{
			SetSelectAt(GetDataIndex(data), selected, fireEvent);
		}

		public void SetSelectAt(int dataIndex, bool selected)
		{
			SetSelectAt(dataIndex, selected, true);
		}
		public void SetSelectAt(int dataIndex, bool selected, bool fireEvent)
		{
			if (dataIndex >= 0 && dataIndex < m_Datas.Count) {
				if (selected) {
					if (m_SelectedIndices.Contains(dataIndex))
						return;
				} else {
					if (!m_SelectedIndices.Contains(dataIndex))
						return;
				}

				UpdateSelection(dataIndex, fireEvent, true);
			}
		}

		public int GetDataIndex(UITableViewCell cell)
		{
			return cell != null ? cell.dataIndex : -1;
		}
		public int GetDataIndex(object data)
		{
			UITableViewCell cell = GetCell(data);
			if (cell != null)
				return cell.dataIndex;

			return m_Datas.FindIndex((v) => {
				return object.Equals(v.userData, data);
			});
		}

		public UITableViewCell GetCell(object data)
		{
			return m_ActiveCells.Find((cell) => object.Equals(m_Datas[cell.dataIndex].userData, data));
		}
		public UITableViewCell GetCellAt(int dataIndex)
		{
			return m_ActiveCells.Find((cell) => cell.dataIndex == dataIndex);
		}

		public void Clear(bool keepPosition)
		{
			DOTween.Kill(this);
			m_Datas.Clear();
			m_SelectedIndices.Clear();
			while (m_ActiveCells.Count > 0)
				RecycleCell(m_ActiveCells[0]);
			m_DataDirty = true;
			if (!keepPosition) {
				m_ScrollPosition = 0;
			}
		}

		public void Refresh()
		{
			if (m_DataDirty)
				return;

			if (onCellInit != null) {
				using (var it = m_ActiveCells.GetEnumerator()) {
					while (it.MoveNext()) {
						onCellInit(this, it.Current, m_Datas[it.Current.dataIndex].userData);
					}
				}
			}
		}

		private int m_JumpToIndex = -1;
		private float m_JumpToPosition = -1;
		private float m_JumpToInterval = 0f;

		public void JumpTo(int dataIndex)
		{
			JumpTo(dataIndex, 0f);
		}
		public void JumpTo(int dataIndex, float interval)
		{
			m_JumpToPosition = -1;
			m_JumpToIndex = dataIndex;
			m_JumpToInterval = interval;
		}

		public void JumpToPosition(float position)
		{
			JumpToPosition(position, 0);
		}
		public void JumpToPosition(float position, float interval)
		{
			m_JumpToIndex = -1;
			m_JumpToPosition = position;
			m_JumpToInterval = interval;
		}

		public bool IsSelected(int dataIndex)
		{
			return CheckDataState(dataIndex, UITableViewCell.State.Selected);
		}

		public bool IsDisabled(int dataIndex)
		{
			return CheckDataState(dataIndex, UITableViewCell.State.Disabled);
		}

		public bool IsDimmed(int dataIndex)
		{
			if (dataIndex >= 0 && dataIndex < m_Datas.Count) {
				UITableViewCell.State state = m_Datas[dataIndex].state;
				if (state == UITableViewCell.State.Normal) {
					if (m_MaxSelection > 1 && m_SelectedIndices.Count >= m_MaxSelection) {
						return true;
					}
				}
			}
			return false;
		}

		//public void SelectAll()
		//{
		//}
		#endregion

		#region selection and state
		private bool CheckDataState(int dataIndex, UITableViewCell.State state)
		{
			if (dataIndex >= 0 && dataIndex < m_Datas.Count) {
				return m_Datas[dataIndex].state == state;
			}
			return false;
		}
		private void UpdateActiveCellStates()
		{
			using (var it = m_ActiveCells.GetEnumerator()) {
				while (it.MoveNext()) {
					SetState(it.Current, m_Datas[it.Current.dataIndex].state, true);
				}
			}
		}
		private void UpdateSelection(/*UITableViewCell cell, */int dataIndex, bool fireEvent, bool force)
		{
			int added = -1, removed = -1;
			int selectedCount = m_SelectedIndices.Count;
			if (m_SelectedIndices.Contains(dataIndex)) {
				if (force || m_Cancelable || m_MaxSelection != 1) {
					m_SelectedIndices.Remove(removed = dataIndex);
				}
			} else if (!IsDisabled(dataIndex)) {
				if (m_MaxSelection == 1) {
					if (selectedCount > 0) {
						removed = m_SelectedIndices[0];
						m_SelectedIndices.Clear();
					}
					m_SelectedIndices.Add(added = dataIndex);
				} else if (m_MaxSelection == 0 || selectedCount < m_MaxSelection) {
					m_SelectedIndices.Add(added = dataIndex);
				}
			}

			if (removed != -1 || added != -1) {
				if (removed != -1) {
					SetState(removed, UITableViewCell.State.Normal, true);
					if (fireEvent && onDeselected != null) {
						onDeselected(this, m_Datas[removed].userData);
					}
				}
				if (added != -1) {
					SetState(added, UITableViewCell.State.Selected, true);
					if (fireEvent && onSelected != null) {
						onSelected(this, m_Datas[added].userData);
					}
				}
				if (m_MaxSelection > 1) {
					int count = m_SelectedIndices.Count;
					if ((selectedCount >= m_MaxSelection && count < m_MaxSelection) || (selectedCount < m_MaxSelection && count >= m_MaxSelection)) {
						UpdateActiveCellStates();
					}
				}
				if (fireEvent) {
					if (onSelectionChanged != null) {
						onSelectionChanged(this);
					}
				}
			}
		}

		private void SetState(UITableViewCell cell, UITableViewCell.State state, bool fireEvent)
		{
			if (cell != null) {
				if (state == UITableViewCell.State.Normal) {
					if (m_MaxSelection != 0 && m_SelectedIndices.Count >= m_MaxSelection) {
						state = UITableViewCell.State.Dimmed;
					}
				}
				if (cell.state != state) {
					cell.state = state;
					if (fireEvent && onCellStateChanged != null) {
						onCellStateChanged(this, cell, m_Datas[cell.dataIndex].userData);
					}
				}
			}
		}
		private void SetState(int dataIndex, UITableViewCell.State state, bool fireEvent)
		{
			Data data = m_Datas[dataIndex];
			data.state = state;
			m_Datas[dataIndex] = data;

			SetState(GetCellAt(dataIndex), state, fireEvent);
		}
		#endregion

		#region event handlers
		public override void OnInitializePotentialDrag(PointerEventData eventData)
		{
			base.OnInitializePotentialDrag(eventData);
			DOTween.Kill(this);
			if (m_Parent) {
				ExecuteEvents.Execute(m_Parent, eventData, ExecuteEvents.initializePotentialDrag);
			}
		}

		public override void OnBeginDrag(PointerEventData eventData)
		{
			if (m_Parent) {
				m_BeginDragDirection = Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y) ? Direction.Horizontal : Direction.Vertical;
				if (m_BeginDragDirection != m_Direction) {
					ExecuteEvents.Execute(m_Parent, eventData, ExecuteEvents.beginDragHandler);
					return;
				}
			}
			base.OnBeginDrag(eventData);
		}

		public override void OnEndDrag(PointerEventData eventData)
		{
			if (m_Parent) {
				if (m_BeginDragDirection != m_Direction) {
					ExecuteEvents.Execute(m_Parent, eventData, ExecuteEvents.endDragHandler);
					return;
				}
			}
			base.OnEndDrag(eventData);

			TryStartSnap();
		}

		public override void OnDrag(PointerEventData eventData)
		{
			if (m_Parent) {
				if (m_BeginDragDirection != m_Direction) {
					ExecuteEvents.Execute(m_Parent, eventData, ExecuteEvents.dragHandler);
					return;
				}
			}
			base.OnDrag(eventData);
		}

		public override void OnScroll(PointerEventData eventData)
		{
			if (m_Parent) {
				if (m_BeginDragDirection != m_Direction) {
					ExecuteEvents.Execute(m_Parent, eventData, ExecuteEvents.scrollHandler);
					return;
				}
			}
			base.OnScroll(eventData);
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;
			TryStartSnap();
		}

		private int m_LongPressHandle;
		[SerializeField]
		private float m_LongPressDelay = 0.5f;
		[SerializeField]
		private float m_LongPressInterval = 0.2f;

		internal void HandlePress(bool pressed, UITableViewCell cell, GameObject target)
		{
			Scheduler.Remove(ref m_LongPressHandle);

			if (pressed) {
				object data = m_Datas[cell.dataIndex].userData;
				if (onCellPress != null) {
					onCellPress(this, cell, target, data);
				}
				if (onCellDelayPress != null || onCellLongPress != null) {
					m_LongPressHandle = Scheduler.Timeout(delegate () {
						if (onCellDelayPress != null) {
							onCellDelayPress(this, cell, target, data);
						}
						if (onCellLongPress != null) {
							onCellLongPress(this, cell, target, data);
							m_LongPressHandle = Scheduler.Interval(delegate () {
								if (onCellLongPress != null) {
									onCellLongPress(this, cell, target, data);
								} else {
									Scheduler.Remove(ref m_LongPressHandle);
								}
							}, m_LongPressInterval);
						}
					}, m_LongPressDelay);
				}
			}
		}
		internal void HandleClick(UITableViewCell cell, GameObject target, bool updateSelection)
		{
			TryStartSnap();
			if (onCellClick != null) {
				onCellClick(this, cell, target, m_Datas[cell.dataIndex].userData);
			}
			if (updateSelection && cell != null && cell.active) {
				if (!IsDisabled(cell.dataIndex)) {
					UpdateSelection(cell.dataIndex, true, false);
				}
			}
		}
		#endregion

		#region private and overrided methods
		private float m_StoredScrollRectSize;
		private float m_StoredMaxScrollPosition = 0f;
		private float m_AdjustSpacingSize = 0f;

		protected override void Awake()
		{
			base.Awake();

			GameObject go;
			if (m_Container == null) {
				go = new GameObject("Container", typeof(RectTransform));
				m_Container = go.GetComponent<RectTransform>();
				m_Container.SetParent(m_RectTransform, false);
				if (m_Direction == Direction.Vertical) {
					m_Container.anchorMin = new Vector2(0f, 1f);
					m_Container.anchorMax = Vector2.one;
				} else {
					m_Container.anchorMin = Vector2.zero;
					m_Container.anchorMax = new Vector2(0f, 1f);
				}
				m_Container.pivot = new Vector2(0f, 1f);
				m_Container.offsetMax = m_Container.offsetMin = Vector3.zero;
				m_Container.localScale = Vector3.one;
			}

			content = m_Container;
			m_StoredScrollRectSize = scrollRectSize;
		}

		protected override void Start()
		{
			base.Start();

			for (int i = 0; i < m_CellList.Count; i++) {
				var cell = m_CellList[i];
				if (cell == null)
					continue;
				m_CellPrefabs[cell.identifier] = cell;
				//cell.gameObject.SetActive(true);
				//cell.tableView = this;
				UIScroller child = cell.GetComponentInChildren<UIScroller>(true);
				if (child) {
					child.m_Parent = gameObject;
				}
				//RecycleCell(cell);
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			TryStartSnap();
		}
		protected override void OnDisable()
		{
			base.OnDisable();

			DOTween.Kill(this, true);
			Scheduler.Remove(ref m_LongPressHandle);
		}

		private void TryStartSnap()
		{
			if (DOTween.IsTweening(this))
				return;

			if (m_Snapping && !m_Dragging && m_ScrollPosition > 0 && m_ScrollPosition < m_StoredMaxScrollPosition) {
				if (Mathf.Abs(velocity) <= m_SnapVelocityThreshold) {
					velocity = 0f;
					float position = m_ScrollPosition - padding;
					if (m_SnapCellCenter) {
						position += (m_StoredScrollRectSize / 2 - m_SnapOffset[(int)m_Direction]);
					}
					int lineIndex = GetLineIndexAtPosition(position);
					Vector2 vec = axisRects[lineIndex];
					if (vec.x + vec.y * (m_SnapCellCenter ? 1f : 0.5f) >= position) {
						position = CalculateScrollPosition(lineIndex);
					} else {
						position = CalculateScrollPosition(++lineIndex);
					}
					DOTween.To((v) => scrollPosition = v, m_ScrollPosition, position, 0.2f).SetTarget(this);
				}
			}
		}
		private float CalculateScrollPosition(int lineIndex)
		{
			Vector2 v;
			if (m_Snapping && m_SnapCellCenter) {
				v = axisRects[lineIndex];
				return m_SnapOffset[(int)m_Direction] + padding - (m_StoredScrollRectSize - v.y) / 2f + v.x;
			}

			v = axisRects[lineIndex];
			return v.x;
		}

		private UITableViewCell DequeueOrCreateCell(int dataIndex)
		{
			Data data = m_Datas[dataIndex];
			UITableViewCell cell = recycledContainer.DequeueCell(data.ident);
			if (cell == null) {
				GameObject go = GameObject.Instantiate<GameObject>(m_CellPrefabs[data.ident].gameObject);
				go.SetActive(true);
				cell = go.GetComponent<UITableViewCell>();
			}
			cell.transform.SetParent(m_Container, false);
			cell.tableView = this;
			return cell;
		}

		private void AddCell(int cellIndex, int dataIndex)
		{
			Data data = m_Datas[dataIndex];
			UITableViewCell cell = DequeueOrCreateCell(dataIndex);
			m_ActiveCells.Add(cell);

			// set the cell's properties
			cell.cellIndex = cellIndex;
			cell.dataIndex = dataIndex;
			cell.active = true;
			SetState(cell, data.state, true);

			cell.transform.SetParent(m_Container, false);
			cell.transform.localScale = Vector3.one;
			cell.gameObject.SetActive(true);

			Vector2 col, row;
			if (m_Direction == Direction.Vertical) {
				row = m_RowRects[cellIndex / m_MaxPerLine];
				if (m_MaxPerLine > 1) {
					col = new Vector2((cellIndex % m_MaxPerLine) * m_AdjustSpacingSize, m_AdjustSpacingSize);
				} else {
					col = m_ColRects[cellIndex % m_MaxPerLine];
				}
			} else {
				row = m_RowRects[cellIndex % m_MaxPerLine];
				col = m_ColRects[cellIndex / m_MaxPerLine];
				//if (m_MaxPerLine > 1) {
				//	row = new Vector2((cellIndex % m_MaxPerLine) * m_AdjustSpacingSize, m_AdjustSpacingSize);
				//} else {
				//	row = m_ColRects[cellIndex % m_MaxPerLine];
				//}
			}

			Rect rect = data.rect;
			col.x += (col.y - rect.width) * ((int)m_ChildAlignment % 3) * 0.5f - rect.x + m_SnapOffset.x + m_LayoutOffset.x + m_Padding.left;
			row.x += (row.y - rect.height) * ((int)m_ChildAlignment / 3) * 0.5f + rect.y + m_SnapOffset.y + m_LayoutOffset.y + m_Padding.top;

			RectTransform rectTransform = (RectTransform)cell.transform;
			Vector2 position = rectTransform.localPosition;
			if (m_Direction == Direction.Vertical) {
				if (rectTransform.anchorMin.x == rectTransform.anchorMax.x) {
					position.x = col.x;
				}
				position.y = -row.x;
			} else {
				if (rectTransform.anchorMin.y == rectTransform.anchorMax.y) {
					position.y = -row.x;
				}
				position.x = col.x;
			}
			cell.transform.localPosition = position;

			if (onCellInit != null)
				onCellInit(this, cell, data.userData);
		}

		private void RecycleCell(UITableViewCell cell)
		{
			m_ActiveCells.Remove(cell);
			recycledContainer.RecycleCell(cell);
		}

		private int m_CellCount = 0;
		private int m_LineCount = 0;
		// x - 坐标， y - 尺寸
		private Vector2[] m_RowRects = new Vector2[0];
		private Vector2[] m_ColRects = new Vector2[0];
		private Vector2[] axisRects
		{
			get
			{
				return m_Direction == Direction.Horizontal ? m_ColRects : m_RowRects;
			}
		}

		private void ResizeArray(int newSize, bool reset)
		{
			m_CellCount = newSize;
			m_LineCount = Mathf.CeilToInt(newSize / (float)m_MaxPerLine);
			if (m_Direction == Direction.Vertical) {
				ResizeArray(ref m_ColRects, m_MaxPerLine, reset);
				ResizeArray(ref m_RowRects, m_LineCount, reset);
			} else {
				ResizeArray(ref m_ColRects, m_LineCount, reset);
				ResizeArray(ref m_RowRects, m_MaxPerLine, reset);
			}
		}
		private void ResizeArray<T>(ref T[] array, int newSize, bool reset)
		{
			if (array.Length < newSize) {
				var buffer = new T[newSize];
				if (!reset) {
					Array.Copy(array, buffer, array.Length);
				}
				array = buffer;
			}
			if (reset) {
				Array.Clear(array, 0, newSize);
			}
		}

		private void ExpandLineSize(int cellIndex, Rect rect)
		{
			int row, col;
			if (m_Direction == Direction.Vertical) {
				col = cellIndex % m_MaxPerLine;
				row = cellIndex / m_MaxPerLine;
			} else {
				row = cellIndex % m_MaxPerLine;
				col = cellIndex / m_MaxPerLine;
			}
			m_ColRects[col].y = Mathf.Max(m_ColRects[col].y, rect.width);
			m_RowRects[row].y = Mathf.Max(m_RowRects[row].y, rect.height);
		}

		private Vector2 CalculateCellOffsets()
		{
			float offsetX = 0f; // m_Padding.left;
			for (int i = 0, count = (m_Direction == Direction.Vertical ? m_MaxPerLine : m_LineCount); i < count; i++) {
				m_ColRects[i].x = offsetX;
				offsetX += m_ColRects[i].y + m_Spacing.x;
			}

			float offsetY = 0f; // m_Padding.top;
			for (int i = 0, count = (m_Direction == Direction.Vertical ? m_LineCount : m_MaxPerLine); i < count; i++) {
				m_RowRects[i].x = offsetY;
				offsetY += m_RowRects[i].y + m_Spacing.y;
			}
			return new Vector2(offsetX - (offsetX != 0 ? m_Spacing.x : 0), offsetY - (offsetY != 0 ? m_Spacing.y : 0));
		}

		private void CalculateCellSizes()
		{
			int dataCount = m_Datas.Count;
			ResizeArray(dataCount, true);

			Data data;
			Bounds bounds;
			for (int i = 0; i < dataCount; i++) {
				data = m_Datas[i];
				if (data.dirty || m_ScrollRectSizeChanged) {
					if (!m_CellPrefabBounds.TryGetValue(data.ident, out bounds)) {
						var cell = m_CellPrefabs[data.ident];
						if (cell.mode == UITableViewCell.Mode.Fixed) {
							bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(cell.transform);
							m_CellPrefabBounds[data.ident] = bounds;
						} else if (onCellInit != null) {
							cell = DequeueOrCreateCell(i);
							onCellInit(this, cell, data.userData);
							//foreach (var layout in GetComponentsInChildren<LayoutGroup>(false)) {
							//	LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
							//}
							bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(cell.transform);
							if (cell.mode == UITableViewCell.Mode.CalcOnce) {
								m_CellPrefabBounds[data.ident] = bounds;
							}
							RecycleCell(cell);
						} else {
							bounds = default(Bounds);
						}
					}
					data.rect = new Rect(new Vector2(bounds.min.x, bounds.max.y), bounds.size);
					data.dirty = false;
					m_Datas[i] = data;
				}
				ExpandLineSize(i, data.rect);
			}
			m_ScrollRectSizeChanged = false;

			if (m_Loop) {
				if (m_MaxPerLine > 1 && dataCount % m_MaxPerLine > 0) {
					for (int i = 2; i <= m_MaxPerLine; i++) {
						int len = dataCount * i;
						if (len % m_MaxPerLine == 0) {
							//size = len / m_MaxPerLine;
							ResizeArray(len, false);
							break;
						}
					}
					for (int i = dataCount, length = dataCount * m_MaxPerLine; i < length; i++) {
						ExpandLineSize(i, m_Datas[i % dataCount].rect);
					}
				}

				// copy two more cellsizes
				int rate = 1, size = m_LineCount;
				float offset = CalculateCellOffsets()[(int)m_Direction];
				if (offset < m_StoredScrollRectSize) {
					rate = Mathf.CeilToInt(m_StoredScrollRectSize / offset);
				}
				rate *= 3;
				ResizeArray(m_CellCount * rate, false);
				for (int i = 1; i < rate; i++) {
					Array.Copy(axisRects, 0, axisRects, size * i, size);
				}
			}
		}

		private float m_LoopPosition;
		private float m_LoopFirstJumpTrigger;
		private float m_LoopLastJumpTrigger;

		private void CalculateContainerSize()
		{
			Vector2 size = Vector2.zero;
			Vector2 offset = CalculateCellOffsets();
			size.x = m_Padding.left + offset.x + m_Padding.right;
			size.y = m_Padding.top + offset.y + m_Padding.bottom;

			m_SnapOffset = Vector2.zero;
			if (m_SnapCellCenter && !m_Loop) {
				if (m_Direction == Direction.Vertical) {
					m_SnapOffset.y = (m_StoredScrollRectSize - m_RowRects[0].y) * 0.5f - m_Padding.top;
					if (m_LineCount > 0) {
						size.y += m_SnapOffset.y + (m_StoredScrollRectSize - m_RowRects[m_LineCount - 1].y) * 0.5f - m_Padding.bottom;
					}
				} else {
					m_SnapOffset.x = (m_StoredScrollRectSize - m_ColRects[0].y) * 0.5f - m_Padding.left;
					if (m_LineCount > 0) {
						size.x += m_SnapOffset.x + (m_StoredScrollRectSize - m_ColRects[m_LineCount - 1].y) * 0.5f - m_Padding.right;
					}
				}
			}
			float contentSize = size[(int)m_Direction];
			Vector2 sizeDelta = m_Container.sizeDelta;
			sizeDelta[(int)m_Direction] = contentSize;
			m_Container.sizeDelta = sizeDelta;
			m_StoredMaxScrollPosition = contentSize - scrollRectSize;

			m_LayoutOffset = Vector2.zero;
			Rect rect = m_RectTransform.rect;
			if (m_Direction == Direction.Vertical) {
				if (!m_Loop && size.y < rect.height) {
					m_LayoutOffset.y = (rect.height - size.y) * ((int)m_Alignment / 3) * 0.5f;
				}
				m_LayoutOffset.x = (rect.width - size.x) * ((int)m_Alignment % 3) * 0.5f;
			} else {
				if (!m_Loop && size.x < rect.width) {
					m_LayoutOffset.x = (rect.width - size.x) * ((int)m_Alignment % 3) * 0.5f;
				}
				m_LayoutOffset.y = (rect.height - size.y) * ((int)m_Alignment / 3) * 0.5f;
			}

			if (m_Loop) {
				int firstIndex = m_LineCount / 3;
				m_LoopPosition = axisRects[firstIndex].x;
				m_LoopFirstJumpTrigger = m_LoopPosition - m_StoredScrollRectSize;
				m_LoopLastJumpTrigger = axisRects[firstIndex * 2].x;
				if (m_SnapCellCenter) {
					scrollPosition = m_LoopPosition - (m_StoredScrollRectSize - axisRects[firstIndex].y) / 2;
				} else {
					scrollPosition = m_LoopPosition;
				}
				//Debug.Log(m_LoopPosition + ", " + m_LoopFirstJumpTrigger + ", " + m_LoopLastJumpTrigger);
			}

			//CalculateActiveCells();
		}

		private void CalculateActiveCells()
		{
			int dataCount = m_Datas.Count;
			if (dataCount <= 0)
				return;

			int startIndex, endIndex;
			CalculateCurrentActiveCellRange(out startIndex, out endIndex);

			//Debug.Log(startIndex + ", " + endIndex + " | " + m_ActiveCellsStartIndex + ", " + m_ActiveCellsEndIndex);

			if (startIndex >= m_ActiveCellsStartIndex && endIndex <= m_ActiveCellsEndIndex)
				return;

			int i = 0;
			while (i < m_ActiveCells.Count) {
				UITableViewCell cell = m_ActiveCells[i];
				if (cell.cellIndex < startIndex || cell.cellIndex > endIndex) {
					RecycleCell(cell);
				} else {
					i++;
				}
			}
			for (i = startIndex; i <= endIndex; i++) {
				if (i >= m_ActiveCellsStartIndex && i <= m_ActiveCellsEndIndex)
					continue;
				AddCell(i, i % dataCount);
			}

			m_ActiveCellsStartIndex = startIndex;
			m_ActiveCellsEndIndex = endIndex;
		}

		private void CalculateCurrentActiveCellRange(out int startIndex, out int endIndex)
		{
			int count = m_Datas.Count;
			var startPosition = m_ScrollPosition - padding - m_SnapOffset[(int)m_Direction];
			var endPosition = startPosition + m_StoredScrollRectSize;

			//Debug.LogWarning(startPosition + ", " + endPosition);

			endIndex = m_LineCount - 1;
			startIndex = GetLineIndexAtPosition(startPosition);

			for (int i = startIndex + 1; i <= endIndex; i++) {
				if (axisRects[i].x >= endPosition) {
					endIndex = i - 1;
					break;
				}
			}

			startIndex = startIndex * m_MaxPerLine;
			endIndex = Mathf.Min((endIndex + 1) * m_MaxPerLine - 1, m_CellCount - 1);
		}
		private float padding
		{
			get
			{
				return m_Direction == Direction.Vertical ? m_Padding.top : m_Padding.left;
			}
		}
		private int GetLineIndexAtPosition(float position)
		{
			return GetLineIndexAtPosition(axisRects, position, 0, m_LineCount - 1);
		}
		private int GetLineIndexAtPosition(Vector2[] array, float position, int startIndex, int endIndex)
		{
			if (startIndex >= endIndex)
				return startIndex;

			var middleIndex = (startIndex + endIndex) / 2;

			if (array[middleIndex].x + array[middleIndex].y >= position)
				return GetLineIndexAtPosition(array, position, startIndex, middleIndex);
			else
				return GetLineIndexAtPosition(array, position, middleIndex + 1, endIndex);
		}

		protected override void OnPositionChanged(float val)
		{
			if (m_Direction == Direction.Vertical)
				m_ScrollPosition = (1f - val) * m_StoredMaxScrollPosition;
			else
				m_ScrollPosition = val * m_StoredMaxScrollPosition;

			if (m_Loop) {
				if (m_ScrollPosition < m_LoopFirstJumpTrigger) {
					float velocity = this.velocity;
					scrollPosition = m_LoopPosition + m_ScrollPosition;
					this.velocity = velocity;
				} else if (m_ScrollPosition > m_LoopLastJumpTrigger) {
					float velocity = this.velocity;
					scrollPosition = m_LoopPosition + (m_ScrollPosition - m_LoopLastJumpTrigger);
					this.velocity = velocity;
				}
			}

			TryStartSnap();
			CalculateActiveCells();

			if (onValueChanged != null)
				onValueChanged(val);
		}

		private bool m_ScrollRectSizeChanged = false;

		protected override void LateUpdate()
		{
			base.LateUpdate();

			if (m_StoredScrollRectSize != scrollRectSize) {
				m_StoredScrollRectSize = scrollRectSize;
				m_DataDirty = true;
				m_ScrollRectSizeChanged = true;
				m_CellPrefabBounds.Clear();
			}

			if (m_DelayFrames > 0) {
				--m_DelayFrames;
				return;
			}

			if (m_DataDirty) {
				m_DataDirty = false;

				if (m_MaxPerLine > 1) {
					m_AdjustSpacingSize = (m_RectTransform.rect.size.x - m_Padding.left - m_Padding.right) / m_MaxPerLine;
				}
				DOTween.Kill(this);
				Scheduler.Remove(ref m_LongPressHandle);

				float cachedPosition = m_ScrollPosition;
				while (m_ActiveCells.Count > 0)
					RecycleCell(m_ActiveCells[0]);
				m_ActiveCellsStartIndex = m_ActiveCellsEndIndex = -1;
				CalculateCellSizes();
				CalculateContainerSize();
				if (m_JumpToIndex < 0 && m_JumpToPosition < 0) {
					scrollPosition = cachedPosition;
					CalculateActiveCells();
				}
			}
			if (m_JumpToIndex >= 0 || m_JumpToPosition >= 0) {
				float position = 0f;
				if (m_JumpToIndex >= 0) {
					m_JumpToIndex = m_JumpToIndex / m_MaxPerLine;
					position = CalculateScrollPosition(m_JumpToIndex);
					m_JumpToIndex = -1;
				} else {
					position = Math.Min(m_JumpToPosition, m_StoredMaxScrollPosition);
					m_JumpToPosition = -1;
				}
				if (m_JumpToInterval > 0f) {
					DOTween.Kill(this, true);
					DOTween.To((v) => scrollPosition = v, m_ScrollPosition, position, m_JumpToInterval).SetTarget(this);
				} else {
					scrollPosition = position;
				}
				CalculateActiveCells();
			}
		}
		#endregion
	}
}

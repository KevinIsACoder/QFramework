using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Spark
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform))]
	[XLua.LuaCallCSharp]
	public class UIScroller : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler, ICanvasElement, ILayoutElement, ILayoutGroup
	{
		public enum MovementType
		{
			Elastic, // Restricted but flexible -- can go past the edges, but springs back in place
			Clamped, // Restricted movement where it's not possible to go past the edges
		}
		public enum Direction
		{
			Horizontal,
			Vertical
		}

		public delegate void OnValueChanged(float val);
		public OnValueChanged onValueChanged;

		[SerializeField]
		protected Direction m_Direction = Direction.Vertical;

		[SerializeField]
		protected MovementType m_MovementType = MovementType.Elastic;

		[SerializeField]
		protected float m_Elasticity = 0.1f; // Only used for MovementType.Elastic

		[SerializeField]
		protected bool m_Inertia = true;

		[SerializeField]
		protected float m_DecelerationRate = 0.135f; // Only used when inertia is enabled

		[SerializeField]
		protected float m_ScrollSensitivity = 20.0f;

		public RectTransform content
		{
			get; protected set;
		}

		protected RectTransform m_RectTransform
		{
			get; private set;
		}

		protected bool m_Dragging
		{
			get; private set;
		}
			
		private Vector2 m_Velocity;
		protected float velocity
		{
			get
			{
				return m_Velocity[(int)m_Direction];
			}
			set
			{
				m_Velocity[(int)m_Direction] = value;
			}
		}

		// The offset from handle position to mouse down position
		private Vector2 m_PointerStartLocalCursor = Vector2.zero;
		private Vector2 m_ContentStartPosition = Vector2.zero;

		private Bounds m_ContentBounds;
		private Bounds m_ViewBounds;

		private Vector2 m_PrevPosition = Vector2.zero;
		private Bounds m_PrevContentBounds;
		private Bounds m_PrevViewBounds;
		[NonSerialized]
		private bool m_HasRebuiltLayout = false;

		protected Direction m_BeginDragDirection = Direction.Horizontal;
		[SerializeField, HideInInspector]internal GameObject m_Parent = null;
		protected UIScroller()
		{
		}

		[XLua.BlackList]
		public virtual void Rebuild(CanvasUpdate executing)
		{
			if (executing == CanvasUpdate.PostLayout) {
				UpdateBounds();
				UpdatePrevData();

				m_HasRebuiltLayout = true;
			}
		}

		[XLua.BlackList]
		public virtual void LayoutComplete()
		{
		}

		[XLua.BlackList]
		public virtual void GraphicUpdateComplete()
		{
		}

		protected override void Awake()
		{
			base.Awake();

			m_RectTransform = (RectTransform)transform;
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
		}

		protected override void OnDisable()
		{
			CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

			m_HasRebuiltLayout = false;
			m_Velocity = Vector2.zero;
			LayoutRebuilder.MarkLayoutForRebuild(m_RectTransform);
			base.OnDisable();
		}

		[XLua.BlackList]
		public override bool IsActive()
		{
			return base.IsActive() && content != null;
		}

		private IEnumerator EnsureLayoutHasRebuilt()
		{
			yield return new WaitForEndOfFrame();
			if (!m_HasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
			{
				Canvas.ForceUpdateCanvases();
			}
		}

		//public virtual void StopMovement()
		//{
		//	velocity = Vector2.zero;
		//}

		[XLua.BlackList]
		public virtual void OnScroll(PointerEventData data)
		{
			if (!IsActive())
				return;

			StartCoroutine(EnsureLayoutHasRebuilt());
			UpdateBounds();

			Vector2 delta = data.scrollDelta;
			// Down is positive for scroll events, while in UI system up is positive.
			delta.y *= -1;
			if (m_Direction == Direction.Vertical) {
				if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
					delta.y = delta.x;
				delta.x = 0;
			} else {
				if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
					delta.x = delta.y;
				delta.y = 0;
			}

			Vector2 position = content.anchoredPosition;
			position += delta * m_ScrollSensitivity;
			if (m_MovementType == MovementType.Clamped)
				position += CalculateOffset(position - content.anchoredPosition);

			SetContentAnchoredPosition(position);
			UpdateBounds();
		}

		[XLua.BlackList]
		public virtual void OnInitializePotentialDrag(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			m_Velocity = Vector2.zero;
		}

		//[XLua.BlackList]
		public virtual void OnBeginDrag(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			if (!IsActive())
				return;

			UpdateBounds();

			m_PointerStartLocalCursor = Vector2.zero;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(m_RectTransform, eventData.position, eventData.pressEventCamera, out m_PointerStartLocalCursor);
			m_ContentStartPosition = content.anchoredPosition;
			m_Dragging = true;
		}

		//[XLua.BlackList]
		public virtual void OnEndDrag(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			m_Dragging = false;
		}

		//[XLua.BlackList]
		public virtual void OnDrag(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			if (!IsActive())
				return;

			Vector2 localCursor;
			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(m_RectTransform, eventData.position, eventData.pressEventCamera, out localCursor))
				return;

			UpdateBounds();

			var pointerDelta = localCursor - m_PointerStartLocalCursor;
			Vector2 position = m_ContentStartPosition + pointerDelta;

			// Offset to get content into place in the view.
			Vector2 offset = CalculateOffset(position - content.anchoredPosition);
			position += offset;
			if (m_MovementType == MovementType.Elastic) {
				if (offset.x != 0)
					position.x = position.x - RubberDelta(offset.x, m_ViewBounds.size.x);
				if (offset.y != 0)
					position.y = position.y - RubberDelta(offset.y, m_ViewBounds.size.y);
			}

			SetContentAnchoredPosition(position);
		}

		protected virtual void SetContentAnchoredPosition(Vector2 position)
		{
			if (m_Direction == Direction.Vertical)
				position.x = content.anchoredPosition.x;
			else
				position.y = content.anchoredPosition.y;

			if (position != content.anchoredPosition) {
				content.anchoredPosition = position;
				UpdateBounds();
			}
		}

		protected virtual void LateUpdate()
		{
			if (!content)
				return;

			StartCoroutine(EnsureLayoutHasRebuilt());
			UpdateBounds();
			float deltaTime = Time.unscaledDeltaTime;
			Vector2 offset = CalculateOffset(Vector2.zero);
			if (!m_Dragging && (offset != Vector2.zero || m_Velocity != Vector2.zero)) {
				Vector2 position = content.anchoredPosition;
				for (int axis = 0; axis < 2; axis++) {
					// Apply spring physics if movement is elastic and content has an offset from the view.
					if (m_MovementType == MovementType.Elastic && offset[axis] != 0) {
						float speed = m_Velocity[axis];
						position[axis] = Mathf.SmoothDamp(content.anchoredPosition[axis], content.anchoredPosition[axis] + offset[axis], ref speed, m_Elasticity, Mathf.Infinity, deltaTime);
						m_Velocity[axis] = speed;
					}
					// Else move content according to velocity with deceleration applied.
					else if (m_Inertia) {
						m_Velocity[axis] *= Mathf.Pow(m_DecelerationRate, deltaTime);
						if (Mathf.Abs(m_Velocity[axis]) < 1)
							m_Velocity[axis] = 0;
						position[axis] += m_Velocity[axis] * deltaTime;
					}
					// If we have neither elaticity or friction, there shouldn't be any velocity.
					else {
						m_Velocity[axis] = 0;
					}
				}

				if (m_Velocity != Vector2.zero) {
					if (m_MovementType == MovementType.Clamped) {
						offset = CalculateOffset(position - content.anchoredPosition);
						position += offset;
					}

					SetContentAnchoredPosition(position);
				}
			}

			if (m_Dragging && m_Inertia) {
				Vector3 newVelocity = (content.anchoredPosition - m_PrevPosition) / deltaTime;
				m_Velocity = Vector3.Lerp(m_Velocity, newVelocity, deltaTime * 10);
			}

			if (m_ViewBounds != m_PrevViewBounds || m_ContentBounds != m_PrevContentBounds || content.anchoredPosition != m_PrevPosition) {
				OnPositionChanged(normalizedPosition);
				UpdatePrevData();
			}
		}

		protected virtual void OnPositionChanged(float val)
		{
		}

		private void UpdatePrevData()
		{
			if (content == null)
				m_PrevPosition = Vector2.zero;
			else
				m_PrevPosition = content.anchoredPosition;
			m_PrevViewBounds = m_ViewBounds;
			m_PrevContentBounds = m_ContentBounds;
		}

		public virtual float normalizedPosition
		{
			get
			{
				return GetNormalizedPosition((int)m_Direction);
			}
			set
			{
				SetNormalizedPosition(value, (int)m_Direction);
			}
		}

		private float GetNormalizedPosition(int axis)
		{
			UpdateBounds();
			if (m_ContentBounds.size[axis] <= m_ViewBounds.size[axis])
				return (m_ViewBounds.min[axis] > m_ContentBounds.min[axis]) ? 1 : 0;
			return (m_ViewBounds.min[axis] - m_ContentBounds.min[axis]) / (m_ContentBounds.size[axis] - m_ViewBounds.size[axis]);
		}

		private void SetNormalizedPosition(float value, int axis)
		{
			StartCoroutine(EnsureLayoutHasRebuilt());
			UpdateBounds();
			// How much the content is larger than the view.
			float hiddenLength = m_ContentBounds.size[axis] - m_ViewBounds.size[axis];
			// Where the position of the lower left corner of the content bounds should be, in the space of the view.
			float contentBoundsMinPosition = m_ViewBounds.min[axis] - value * hiddenLength;
			// The new content localPosition, in the space of the view.
			float newLocalPosition = content.localPosition[axis] + contentBoundsMinPosition - m_ContentBounds.min[axis];

			Vector3 localPosition = content.localPosition;
			if (Mathf.Abs(localPosition[axis] - newLocalPosition) > 0.01f) {
				localPosition[axis] = newLocalPosition;
				content.localPosition = localPosition;
				m_Velocity[axis] = 0;
				UpdateBounds();
			}
		}

		private static float RubberDelta(float overStretching, float viewSize)
		{
			return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
		}

		protected override void OnRectTransformDimensionsChange()
		{
			SetDirty();
		}

		[XLua.BlackList]
		public virtual void CalculateLayoutInputHorizontal()
		{
		}
		[XLua.BlackList]
		public virtual void CalculateLayoutInputVertical()
		{
		}

		[XLua.BlackList]
		public virtual float minWidth
		{
			get
			{
				return -1;
			}
		}
		[XLua.BlackList]
		public virtual float preferredWidth
		{
			get
			{
				return -1;
			}
		}
		[XLua.BlackList]
		public virtual float flexibleWidth
		{
			get
			{
				return -1;
			}
		}
		[XLua.BlackList]
		public virtual float minHeight
		{
			get
			{
				return -1;
			}
		}
		[XLua.BlackList]
		public virtual float preferredHeight
		{
			get
			{
				return -1;
			}
		}
		[XLua.BlackList]
		public virtual float flexibleHeight
		{
			get
			{
				return -1;
			}
		}
		[XLua.BlackList]
		public virtual int layoutPriority
		{
			get
			{
				return -1;
			}
		}
		[XLua.BlackList]
		public virtual void SetLayoutHorizontal()
		{
		}
		[XLua.BlackList]
		public virtual void SetLayoutVertical()
		{
			//m_ViewBounds = new Bounds(m_RectTransform.rect.center, m_RectTransform.rect.size);
			//m_ContentBounds = GetBounds();
		}

		private void UpdateBounds()
		{
			m_ViewBounds = new Bounds(m_RectTransform.rect.center, m_RectTransform.rect.size);
			m_ContentBounds = GetBounds();

			if (content == null)
				return;

			// Make sure content bounds are at least as large as view by adding padding if not.
			// One might think at first that if the content is smaller than the view, scrolling should be allowed.
			// However, that's not how scroll views normally work.
			// Scrolling is *only* possible when content is *larger* than view.
			// We use the pivot of the content rect to decide in which directions the content bounds should be expanded.
			// E.g. if pivot is at top, bounds are expanded downwards.
			// This also works nicely when ContentSizeFitter is used on the content.
			Vector3 contentSize = m_ContentBounds.size;
			Vector3 contentPos = m_ContentBounds.center;
			Vector3 excess = m_ViewBounds.size - contentSize;
			if (excess.x > 0) {
				contentPos.x -= excess.x * (content.pivot.x - 0.5f);
				contentSize.x = m_ViewBounds.size.x;
			}
			if (excess.y > 0) {
				contentPos.y -= excess.y * (content.pivot.y - 0.5f);
				contentSize.y = m_ViewBounds.size.y;
			}

			m_ContentBounds.size = contentSize;
			m_ContentBounds.center = contentPos;
		}

		private readonly Vector3[] m_Corners = new Vector3[4];
		private Bounds GetBounds()
		{
			if (content == null)
				return new Bounds();

			var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

			var toLocal = m_RectTransform.worldToLocalMatrix;
			content.GetWorldCorners(m_Corners);
			for (int j = 0; j < 4; j++) {
				Vector3 v = toLocal.MultiplyPoint3x4(m_Corners[j]);
				vMin = Vector3.Min(v, vMin);
				vMax = Vector3.Max(v, vMax);
			}

			var bounds = new Bounds(vMin, Vector3.zero);
			bounds.Encapsulate(vMax);
			return bounds;
		}

		private Vector2 CalculateOffset(Vector2 delta)
		{
			Vector2 offset = Vector2.zero;

			Vector2 min = m_ContentBounds.min;
			Vector2 max = m_ContentBounds.max;

			if (m_Direction == Direction.Vertical) {
				min.y += delta.y;
				max.y += delta.y;
				if (max.y < m_ViewBounds.max.y)
					offset.y = m_ViewBounds.max.y - max.y;
				else if (min.y > m_ViewBounds.min.y)
					offset.y = m_ViewBounds.min.y - min.y;
			} else {
				min.x += delta.x;
				max.x += delta.x;
				if (min.x > m_ViewBounds.min.x)
					offset.x = m_ViewBounds.min.x - min.x;
				else if (max.x < m_ViewBounds.max.x)
					offset.x = m_ViewBounds.max.x - max.x;
			}
			return offset;
		}

		protected void SetDirty()
		{
			if (!IsActive())
				return;

			LayoutRebuilder.MarkLayoutForRebuild(m_RectTransform);
		}

		protected void SetDirtyCaching()
		{
			if (!IsActive())
				return;

			CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
			LayoutRebuilder.MarkLayoutForRebuild(m_RectTransform);
		}

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			SetDirtyCaching();
		}

#endif
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Spark
{
    [XLua.LuaCallCSharp]
    public class UILongClickButton : Button, ICancelHandler
    {
        [Serializable]
        public class ButtonLongClickedEvent : UnityEvent<int>
        {
        }
        [Serializable]
        public class ButtonLongClickedCancelEvent : UnityEvent
        {
        }
        [Serializable]
        public class ButtonLongClickedStartEvent : UnityEvent
        {
        }

        [SerializeField]
        private ButtonLongClickedEvent m_OnLongClick = new ButtonLongClickedEvent();
        [SerializeField]
        private ButtonLongClickedCancelEvent m_OnCancelLongClick = new ButtonLongClickedCancelEvent();
        [SerializeField]
        private ButtonLongClickedStartEvent m_OnStartLongClick = new ButtonLongClickedStartEvent();

        public ButtonLongClickedEvent onLongClick { get { return m_OnLongClick; } set { m_OnLongClick = value; } }
        public ButtonLongClickedCancelEvent onCancelLongClick { get { return m_OnCancelLongClick; } set { m_OnCancelLongClick = value; } }
        public ButtonLongClickedStartEvent onStartLongClick { get { return m_OnStartLongClick; } set { m_OnStartLongClick = value; } }

        [SerializeField]
        private float m_LongClickDelay = 0.5f;
        [SerializeField]
        private float m_LongClickInterval = 0.2f;

        private enum StateEnum
        {
            NOT_PRESSING,
            PRESSING_WAITING_FOR_LONG_CLICK,
            PRESSING_AFTER_LONG_CLICK
        }

        StateEnum m_State;
        int m_PressedCount;
        float m_PressedTime;
        
        void Update()
        {
            if (m_State == StateEnum.PRESSING_WAITING_FOR_LONG_CLICK)
            {
                var time = Time.unscaledTime;
                if (time - m_PressedTime >= m_LongClickDelay)
                {
                    m_PressedTime = time;

                    if (!IsActive() || !IsInteractable())
                        return;

                    m_PressedCount = 1;
                    m_State = StateEnum.PRESSING_AFTER_LONG_CLICK;
                    m_OnLongClick.Invoke(m_PressedCount);
                }
            }
            else if (m_State == StateEnum.PRESSING_AFTER_LONG_CLICK)
            {
                var time = Time.unscaledTime;
                if (time - m_PressedTime >= m_LongClickInterval)
                {
                    m_PressedTime = time;

                    if (!IsActive() || !IsInteractable())
                        return;

                    m_OnLongClick.Invoke(++m_PressedCount);
                }
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive() || !IsInteractable())
                return;

            m_PressedCount = 0;
            m_PressedTime = Time.unscaledTime;
            m_State = StateEnum.PRESSING_WAITING_FOR_LONG_CLICK;
            m_OnStartLongClick?.Invoke();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            m_State = StateEnum.NOT_PRESSING;
            m_OnCancelLongClick?.Invoke();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (m_PressedCount == 0)
            {
                base.OnPointerClick(eventData);
            }
        }

        void ICancelHandler.OnCancel(BaseEventData eventData)
        {
            m_State = StateEnum.NOT_PRESSING;
            m_OnCancelLongClick?.Invoke();
        }

    }
}

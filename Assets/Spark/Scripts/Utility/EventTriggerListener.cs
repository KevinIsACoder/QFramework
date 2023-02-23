using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

#if !NOVEL_ART
[XLua.LuaCallCSharp]
#endif
public class EventTriggerListener : EventTrigger {

    public Action<PointerEventData> onBeginDrag;
    public Action<BaseEventData> onCancel;
    public Action<BaseEventData> onDeselect;
    public Action<PointerEventData> onDrag;
    public Action<PointerEventData> onDrog;
    public Action<PointerEventData> onEndDrag;
    public Action<PointerEventData> onInitializePotentialDrag;
    public Action<AxisEventData> onMove;
    public Action<PointerEventData> onPointerClick;
    public Action<PointerEventData> onPointerDown;
    public Action<PointerEventData> onPointerEnter;
    public Action<PointerEventData> onPointerExit;
    public Action<PointerEventData> onPointerUp;
    public Action<PointerEventData> onScroll;
    public Action<BaseEventData> onSelect;
    public Action<BaseEventData> onSubmit;
    public Action<BaseEventData> onUpdateSelected;

#if !SLG_ART
	[XLua.BlackList]
#endif
	public override void OnBeginDrag(PointerEventData eventData) {
        base.OnBeginDrag(eventData);
        if (onBeginDrag != null) {
            onBeginDrag(eventData);
        }
    }

#if !SLG_ART
	[XLua.BlackList]
#endif
	public override void OnCancel(BaseEventData eventData) {
        base.OnCancel(eventData);
        if (onCancel != null) {
            onCancel(eventData);
        }
    }

#if !SLG_ART
	[XLua.BlackList]
#endif
	public override void OnDeselect(BaseEventData eventData) {
        base.OnDeselect(eventData);
        if (onDeselect != null) {
            onDeselect(eventData);
        }
    }

#if !SLG_ART
	[XLua.BlackList]
#endif
	public override void OnDrag(PointerEventData eventData) {
        base.OnDrag(eventData);
        if (onDrag != null) {
            onDrag(eventData);
        }
    }

#if !SLG_ART
	[XLua.BlackList]
#endif
	public override void OnDrop(PointerEventData eventData) {
        base.OnDrop(eventData);
        if (onDrog != null) {
            onDrog(eventData);
        }
    }

#if !SLG_ART
	[XLua.BlackList]
#endif
	public override void OnEndDrag(PointerEventData eventData) {
        base.OnEndDrag(eventData);
        if (onEndDrag != null) {
            onEndDrag(eventData);
        }
    }

#if !SLG_ART
	[XLua.BlackList]
#endif
	public override void OnInitializePotentialDrag(PointerEventData eventData) {
        base.OnInitializePotentialDrag(eventData);
        if (onInitializePotentialDrag != null) {
            onInitializePotentialDrag(eventData);
        }
    }

#if !SLG_ART
	[XLua.BlackList]
#endif
	public override void OnMove(AxisEventData eventData) {
        base.OnMove(eventData);
        if (onMove != null) {
            onMove(eventData);
        }
    }

#if !SLG_ART
	[XLua.BlackList]
#endif
	public override void OnPointerClick(PointerEventData eventData) {
        base.OnPointerClick(eventData);
        if (onPointerClick != null) {
            onPointerClick(eventData);
        }
    }

#if !SLG_ART
	[XLua.BlackList]
#endif
	public override void OnPointerDown(PointerEventData eventData) {
        base.OnPointerDown(eventData);
        if (onPointerDown != null) {
            onPointerDown(eventData);
        }
    }

#if !SLG_ART
	[XLua.BlackList]
#endif
	public override void OnPointerEnter(PointerEventData eventData) {
        base.OnPointerEnter(eventData);
        if (onPointerEnter != null) {
            onPointerEnter(eventData);
        }
    }

#if !SLG_ART
	[XLua.BlackList]
#endif
	public override void OnPointerExit(PointerEventData eventData) {
        base.OnPointerExit(eventData);
        if (onPointerExit != null) {
            onPointerExit(eventData);
        }
    }

#if !SLG_ART
	[XLua.BlackList]
#endif
	public override void OnPointerUp(PointerEventData eventData) {
        base.OnPointerUp(eventData);
        if (onPointerUp != null) {
            onPointerUp(eventData);
        }
    }

#if !SLG_ART
	[XLua.BlackList]
#endif
	public override void OnScroll(PointerEventData eventData) {
        base.OnScroll(eventData);
        if (onScroll != null) {
            onScroll(eventData);
        }
    }

#if !SLG_ART
	[XLua.BlackList]
#endif
	public override void OnSelect(BaseEventData eventData) {
        base.OnSelect(eventData);
        if (onSelect != null) {
            onSelect(eventData);
        }
    }

#if !SLG_ART
	[XLua.BlackList]
#endif
	public override void OnSubmit(BaseEventData eventData) {
        base.OnSubmit(eventData);
        if (onSubmit != null) {
            onSubmit(eventData);
        }
    }

#if !SLG_ART
	[XLua.BlackList]
#endif
	public override void OnUpdateSelected(BaseEventData eventData) {
        base.OnUpdateSelected(eventData);
        if (onUpdateSelected != null) {
            onUpdateSelected(eventData);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Spark
{
	[XLua.LuaCallCSharp]
    public enum UIMode
    {
        Window,
        Modal,
        Modeless,
        Overlap,
        Popup,
    }

    [XLua.GCOptimize]
    [XLua.LuaCallCSharp]
	[Serializable]
	public struct UIStyle
	{
        [SerializeField]
        internal UIMode mode;
		[SerializeField]
		internal bool topmost;
		[SerializeField]
		internal bool overrideColor;
		[SerializeField]
		internal Color maskColor;

        internal bool IsModeless { get { return mode == UIMode.Modeless; } }
        internal bool IsPopup { get { return mode == UIMode.Popup; } }
        internal bool IsDialog { get { return mode == UIMode.Modal || mode == UIMode.Modeless; } }
        internal bool IsOverlap {  get { return mode == UIMode.Overlap; } }
        internal bool IsWindow { get { return mode == UIMode.Window; } }

        // 是否具备遮罩层
        internal bool maskable { get { return mode == UIMode.Modal || mode == UIMode.Modeless; } }
        // 是否可点击关闭
        internal bool closeable { get { return mode == UIMode.Modeless || mode == UIMode.Popup; } }
        // 是否具备事件底层
        internal bool blockable { get { return mode != UIMode.Overlap; } }
        // 是否能够接受focus事件
        internal bool focusable {  get { return mode != UIMode.Overlap && mode != UIMode.Popup; } }


        static public UIStyle Get(Color maskColor)
		{
			return new UIStyle() { overrideColor = true, maskColor = maskColor };
		}
        static public UIStyle Get(UIMode mode, bool topmost, Color maskColor)
        {
            return new UIStyle() { mode = mode, topmost = topmost, overrideColor = true, maskColor = maskColor };
        }
    }
}

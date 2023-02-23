using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XLua;
using LuaDLL = XLua.LuaDLL.Lua;

namespace Spark
{
    [XLua.LuaCallCSharp]
    public sealed class UIScriptableView : UIView
    {
        static internal XLua.LuaTable ScriptClass;

        public UIScriptableView()
        {
            @class = ScriptClass;
            ident = @class.GetInPath<string>("name").ToString();
            ScriptClass = null;
        }

        internal XLua.LuaTable @class
        {
            get; private set;
        }
        internal XLua.LuaTable @this
        {
            get; private set;
        }
        internal string ident
        {
            get; private set;
        }

        protected internal override string prefabPath
        {
            get
            {

                return @class.GetInPath<string>("prefabPath").ToString();
            }
        }

        [XLua.BlackList]
        public override bool blockBackButton
        {
            get
            {
                try
                {
                    return @this.GetInPath<bool>("blockBackButton");
                }
                catch (Exception)
                {
                    return false;
                };
            }
        }

        [CSharpCallLua]
        public delegate void VoidDelegate(LuaTable self);
        [CSharpCallLua]
        public delegate void BoolDelegate(LuaTable self, bool focus);
        [CSharpCallLua]
        public delegate void ComponentDelegate(LuaTable self, Component com);
        [CSharpCallLua]
        public delegate void ComponentStringDelegate(LuaTable self, Component com, string value);
        [CSharpCallLua]
        public delegate void ComponentBoolDelegate(LuaTable self, Component com, bool value);
        [CSharpCallLua]
        public delegate void ComponentFloatDelegate(LuaTable self, Component com, float value);
        [CSharpCallLua]
        public delegate void ComponentIntDelegate(LuaTable self, Component com, int value);
        [CSharpCallLua]
        public delegate void ViewStackDelegate(LuaTable self, UIViewStack stack);
        [CSharpCallLua]
        public delegate void TableViewClickDelegate(LuaTable self, UITableView tableView, UITableViewCell cell, GameObject target, object data);
        [CSharpCallLua]
        public delegate void TableViewInitDelegate(LuaTable self, UITableView tableView, UITableViewCell cell, object data);
        [CSharpCallLua]
        public delegate void TableViewSelectDelegate(LuaTable self, UITableView tableView, object data);

        private VoidDelegate m_OnCreated;
        private VoidDelegate m_OnAppear;
        private VoidDelegate m_OnOpened;
        private BoolDelegate m_OnFocusChanged;
        private VoidDelegate m_OnClosed;
        private VoidDelegate m_OnDisappear;
        private VoidDelegate m_OnDestroyed;

        private VoidDelegate m_OnBackButtonPressed;
        private VoidDelegate m_OnLocalized;

        private ComponentDelegate m_OnButtonClick;
        private ComponentStringDelegate m_OnTextLink;
        private ComponentStringDelegate m_OnStringValueChanged;
        private ComponentBoolDelegate m_OnBoolValueChanged;
        private ComponentFloatDelegate m_OnFloatValueChanged;
        private ComponentIntDelegate m_OnIntValueChanged;
        private ViewStackDelegate m_OnBeforeViewStackValueChanged;

        private TableViewInitDelegate m_OnTableViewCellInit;
        private TableViewClickDelegate m_OnTableViewCellClick;
        private TableViewSelectDelegate m_OnTableViewSelected;

        protected override void OnCreated()
        {
            @this = SparkLua.CreateInstance(@class);
            @this.Set("root", root);
            @this.Set("component", this);
            @this.Set("transform", transform);
            @this.Set("gameObject", gameObject);
            @this.Set("canvas", gameObject.GetComponentInParent<Canvas>());

#region Init lua functions
            m_OnCreated = @this.Get<VoidDelegate>("OnCreated");
            m_OnAppear = @this.Get<VoidDelegate>("OnAppear");
            m_OnOpened = @this.Get<VoidDelegate>("OnOpened");
            m_OnFocusChanged = @this.Get<BoolDelegate>("OnFocusChanged");
            m_OnClosed = @this.Get<VoidDelegate>("OnClosed");
            m_OnDisappear = @this.Get<VoidDelegate>("OnDisappear");
            m_OnDestroyed = @this.Get<VoidDelegate>("OnDestroyed");

            m_OnBackButtonPressed = @this.Get<VoidDelegate>("OnBackButtonPressed");
            m_OnLocalized = @this.Get<VoidDelegate>("OnLocalized");
#endregion

            base.OnCreated();

            m_OnCreated(@this);
        }

        protected override void OnBackButtonPressed()
        {
            m_OnBackButtonPressed(@this);
        }

        protected override void OnLocalized()
        {
            base.OnLocalized();
            m_OnLocalized(@this);
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            @this.Set("context", context);
            @this.Set("blockBackButton", false);

            m_OnAppear(@this);
            m_OnOpened(@this);
        }
        protected override void OnFocusChanged(bool focus)
        {
            base.OnFocusChanged(focus);
            m_OnFocusChanged(@this, focus);
        }
        protected override void OnClosed()
        {
            base.OnClosed();
            m_OnClosed(@this);
            m_OnDisappear(@this);
            @this.Set<String, UIContext>("context", null);
        }
        protected override void OnDestroyed()
        {
            base.OnDestroyed();
            m_OnDestroyed(@this);
            #region reset lua function
            m_OnCreated = null;
            m_OnAppear = null;
            m_OnOpened = null;
            m_OnFocusChanged = null;
            m_OnClosed = null;
            m_OnDisappear = null;
            m_OnDestroyed = null;

            m_OnBackButtonPressed = null;
            m_OnLocalized = null;

            m_OnButtonClick = null;
            m_OnTextLink = null;
            m_OnStringValueChanged = null;
            m_OnBoolValueChanged = null;
            m_OnFloatValueChanged = null;
            m_OnIntValueChanged = null;
            m_OnBeforeViewStackValueChanged = null;
            #endregion
            if (@this != null) {
                @this.Dispose();
                @this = null;
            }
            @class = null;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnButtonClick(Component com)
        {
            if (@this != null)
            {
                base.OnButtonClick(com);
                if (m_OnButtonClick == null)
                {
                    m_OnButtonClick = @this.Get<ComponentDelegate>("OnButtonClick");
                    if (m_OnButtonClick == null) return;
                }
                m_OnButtonClick(@this, com);
            }
        }
        protected override void OnTextLink(Component com, string value)
        {
            if (@this != null)
            {
                base.OnTextLink(com, value);
                if (m_OnTextLink == null)
                {
                    m_OnTextLink = @this.Get<ComponentStringDelegate>("OnTextLink");
                    if (m_OnTextLink == null) return;
                }
                m_OnTextLink(@this, com, value);
            }
        }
        protected override void OnValueChanged(Component com, bool value)
        {
            if (@this != null)
            {
                base.OnValueChanged(com, value);
                if (m_OnBoolValueChanged == null)
                {
                    m_OnBoolValueChanged = @this.Get<ComponentBoolDelegate>("OnValueChanged");
                    if (m_OnBoolValueChanged == null) return;
                }
                m_OnBoolValueChanged(@this, com, value);
            }
        }
        protected override void OnValueChanged(Component com, float value)
        {
            if (@this != null)
            {
                base.OnValueChanged(com, value);
                if (m_OnFloatValueChanged == null)
                {
                    m_OnFloatValueChanged = @this.Get<ComponentFloatDelegate>("OnValueChanged");
                    if (m_OnFloatValueChanged == null) return;
                }
                m_OnFloatValueChanged(@this, com, value);
            }
        }
        protected override void OnValueChanged(Component com, int value)
        {
            if (@this != null)
            {
                base.OnValueChanged(com, value);
                if (m_OnIntValueChanged == null)
                {
                    m_OnIntValueChanged = @this.Get<ComponentIntDelegate>("OnValueChanged");
                    if (m_OnIntValueChanged == null) return;
                }
                m_OnIntValueChanged(@this, com, value);
            }
        }
        protected override void OnValueChanged(Component com, string value)
        {
            if (@this != null)
            {
                base.OnValueChanged(com, value);
                if (m_OnStringValueChanged == null)
                {
                    m_OnStringValueChanged = @this.Get<ComponentStringDelegate>("OnValueChanged");
                    if (m_OnStringValueChanged == null) return;
                }
                m_OnStringValueChanged(@this, com, value);
            }
        }
        protected override void OnBeforeViewStackValueChanged(UIViewStack viewStack)
        {
            if (@this != null)
            {
                base.OnBeforeViewStackValueChanged(viewStack);
                if (m_OnBeforeViewStackValueChanged == null)
                {
                    m_OnBeforeViewStackValueChanged = @this.Get<ViewStackDelegate>("OnBeforeViewStackValueChanged");
                    if (m_OnBeforeViewStackValueChanged == null) return;
                }
                m_OnBeforeViewStackValueChanged(@this, viewStack);
            }
        }

        protected override void OnTableViewCellInit(UITableView tableView, UITableViewCell cell, object data)
        {
            if (@this != null)
            {
                base.OnTableViewCellInit(tableView, cell, data);
                if (m_OnTableViewCellInit == null)
                {
                    m_OnTableViewCellInit = @this.Get<TableViewInitDelegate>("OnTableViewCellInit");
                    if (m_OnTableViewCellInit == null) return;
                }
                m_OnTableViewCellInit(@this, tableView, cell, data);
            }
        }
        protected override void OnTableViewCellClick(UITableView tableView, UITableViewCell cell, GameObject target, object data)
        {
            if (@this != null)
            {
                base.OnTableViewCellClick(tableView, cell, target, data);
                if (m_OnTableViewCellClick == null)
                {
                    m_OnTableViewCellClick = @this.Get<TableViewClickDelegate>("OnTableViewCellClick");
                    if (m_OnTableViewCellClick == null) return;
                }
                m_OnTableViewCellClick(@this, tableView, cell, target, data);
            }
        }
        protected override void OnTableViewSelected(UITableView tableView, object data)
        {
            if (@this != null)
            {
                base.OnTableViewSelected(tableView, data);
                if (m_OnTableViewSelected == null)
                {
                    m_OnTableViewSelected = @this.Get<TableViewSelectDelegate>("OnTableViewSelected");
                    if (m_OnTableViewSelected == null) return;
                }
                m_OnTableViewSelected(@this, tableView, data);
            }
        }

        /// <summary>
        /// 隐藏UIView的GetComponets方法，便于导出到Lua端使用
        /// </summary>
        /// <param name="component"></param>
        /// <param name="bindEvents"></param>
        /// <returns></returns>
        public new UIComponentCollection GetComponents(Component component, bool bindEvents)
        {
            return base.GetComponents(component, bindEvents);
        }
        
        //public static Dictionary<int, Component> GetComponents(LuaTable table, int paramsCount)
        //{
        //    Dictionary<int, Component> list_ret = new Dictionary<int, Component>();
        //    try
        //    {
        //        IntPtr L = table.L;
        //        Component comp = null;
        //        LuaTable luaObj = null;
        //        int argc = LuaDLL.lua_gettop(L);
        //        LuaTypes lua_type = LuaDLL.lua_type(L, -1);

        //        if (argc == 3 || lua_type.GetType() == typeof(Component))
        //        {
        //            SparkLua.LuaEnviroment.translator.Get(L, 1, out comp);
        //            if (argc == 3)
        //            {
        //                SparkLua.LuaEnviroment.translator.Get(L, 3, out luaObj);
        //            }
        //        }
        //        else
        //        {
        //            SparkLua.LuaEnviroment.translator.Get(L, 1, out luaObj);
        //        }

        //        UIScriptableView view = null;
        //        if (luaObj != null)
        //        {
        //            view = (UIScriptableView)luaObj["__VIEW__"];
        //        }

        //        // 增加一个返回值，用于pcall
        //        LuaDLL.lua_pushboolean(L, true);
        //        // get components
        //        if (paramsCount > 0)
        //        {
        //            LuaDLL.lua_newtable(L);
        //            var components = view != null ? view.GetComponents(comp ?? view.transform, true) : comp.GetComponent<UIComponentCollection>();
        //            for (int i = 0; i < paramsCount; i++)
        //            {
        //                list_ret[i + 1] = components.Get<Component>(i);
        //                //bool is_first;
        //                //int type_id = SparkLua.LuaEnviroment.translator.getTypeId(L, o.GetType(), out is_first);
        //                //SparkLua.LuaEnviroment.translator.PushObject(L, o, type_id);
        //            }
        //            return list_ret;
        //        }

        //        return list_ret;
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}

        //public static Dictionary<int, Component> GetComponents(Transform t, int paramsCount)
        //{
        //    var components = t.GetComponent<UIComponentCollection>();
        //    Dictionary<int, Component> list_ret = new Dictionary<int, Component>();
        //    for (int i = 0; i < paramsCount; i++)
        //    {
        //        list_ret[i + 1] = components.Get<Component>(i);

        //    }
        //    return list_ret;
        //}
    }
}

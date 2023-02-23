using System;
using UnityEngine;
using UnityEngine.EventSystems;
using XLua;
using LuaDLL = XLua.LuaDLL.Lua;

namespace Spark
{
	[LuaCallCSharp]
	public partial class UIViewRoot : UIBehaviour
	{
        //public void Preload(LuaTable view)
        //{
        //	UIScriptableView.ScriptClass = view;
        //	Preload(typeof(UIScriptableView), view["name"].ToString());
        //	//UIScriptableView.ScriptClass = null;
        //}

        public void SetStyle(LuaTable view, UIStyle style)
        {
            Entity entity;
            if (TryGet(view, false, out entity))
            {
                SetStyle(entity, style);
            }
        }

		#region Open
		public LuaTable Open(LuaTable openView)
		{
			return OpenView(openView, null, null);
		}
		public LuaTable Open(LuaTable openView, UIContext context)
		{
			return OpenView(openView, null, context);
		}
		public LuaTable Open(LuaTable openView, UIStyle style)
		{
			return OpenView(openView, style, null);
		}
		public LuaTable Open(LuaTable openView, UIStyle style, UIContext context)
		{
			return OpenView(openView, style, context);
		}
		private LuaTable OpenView(LuaTable openView, UIStyle? style, UIContext context)
		{
			UIScriptableView.ScriptClass = openView;
			UIScriptableView view = Open(typeof(UIScriptableView), openView.Get<string>("name"), style, context) as UIScriptableView;
			//UIScriptableView.ScriptClass = null;
			return view.@this;
		}
		#endregion

		#region OpenAsync
		public void OpenAsync(LuaTable openView)
		{
			OpenViewAsync(openView, null, null, null);
		}
		public void OpenAsync(LuaTable openView, Action<LuaTable> callback)
		{
			OpenViewAsync(openView, null, null, callback);
		}
		public void OpenAsync(LuaTable openView, UIContext context)
		{
			OpenViewAsync(openView, null, context, null);
		}
		public void OpenAsync(LuaTable openView, UIContext context, Action<LuaTable> callback)
		{
			OpenViewAsync(openView, null, context, callback);
		}
		public void OpenAsync(LuaTable openView, UIStyle style)
		{
			OpenViewAsync(openView, style, null, null);
		}
		public void OpenAsync(LuaTable openView, UIStyle style, Action<LuaTable> callback)
		{
			OpenViewAsync(openView, style, null, callback);
		}
		public void OpenAsync(LuaTable openView, UIStyle style, UIContext context)
		{
			OpenViewAsync(openView, style, context, null);
		}
		public void OpenAsync(LuaTable openView, UIStyle style, UIContext context, Action<LuaTable> callback)
		{
			OpenViewAsync(openView, style, context, callback);
		}
		private void OpenViewAsync(LuaTable openView, UIStyle? style, UIContext context, Action<LuaTable> callback)
		{
			Action<UIScriptableView> onComplete = null;
			if (callback != null) {
				onComplete = (view) => {
					callback(view.@this);
				};
			}
			UIScriptableView.ScriptClass = openView;
			OpenAsync(openView.Get<string>("name"), style, context, onComplete);
		}
		#endregion

		#region Close
		public void Close(LuaTable closeView)
		{
			Close(closeView, false, true);
		}
		public void Close(LuaTable closeView, bool destroy)
		{
			Close(closeView, destroy, true);
		}

        public void CloseAllExclude(LuaTable[] excludeTypes, bool destroy = false)
        {
            if (excludeTypes.Length == 0)
            {
                CloseAll(destroy);
            }
            else
            {
                for (int i = m_Entities.Count - 1; i >= 0 ; i--)
                {
                    var entity = m_Entities[i];
                    var type = ((UIScriptableView)entity.view).@class;
                    bool excluded = Array.Exists(excludeTypes, (t) =>
                    {
                        return t.Equals(type);
                    });
                    if (!excluded)
                    {
                        Close(entity, destroy, false, false);
                    }
                }
            }
        }

        private void Close(LuaTable closeView, bool destroy, bool checkState)
		{
			Entity entity;
			if (TryGet(closeView, true, out entity)) {
				if (checkState && m_PushStates.Count > 0) {
					var type = ((UIScriptableView)entity.view).@class;
					if (m_PushStates.Exists((state) => type.Equals(state.next))) {
						ClearPushStates();
					}
				}
				Close(entity, destroy, false, false);
			}
		}

        public void DestroyAll(params LuaTable[] excludeTypes)
		{
			if (excludeTypes.Length == 0) {
				CloseAll(true);
			} else {
				while (m_Entities.Count > 0) {
					int count = m_Entities.Count;
					do {
						Entity entity = m_Entities[--count];
						var excluded = false;
						if (entity.view.GetType() == typeof(UIScriptableView)) {
							var type = ((UIScriptableView)entity.view).@class;
							excluded = Array.Exists(excludeTypes, (t) => t.Equals(type));
							if (excluded) {
								if (entity.phase == Entity.Phase.Closing) {
									if (count > 0)
										continue;
									return;
								}
							}
						}
						Close(entity, !excluded, false, false);
						break;
					} while (true);
				}
				ClearPushStates();
			}
		}
		#endregion

		#region CloseAsync
		public void CloseAsync(LuaTable closeView)
		{
			CloseAsync(closeView, false, true, null);
		}
		public void CloseAsync(LuaTable closeView, Action callback)
		{
			CloseAsync(closeView, false, true, callback);
		}
		public void CloseAsync(LuaTable closeView, bool destroy)
		{
			CloseAsync(closeView, destroy, true, null);
		}
		public void CloseAsync(LuaTable closeView, bool destroy, Action callback)
		{
			CloseAsync(closeView, destroy, true, callback);
		}
		private void CloseAsync(LuaTable closeView, bool destroy, bool checkState, Action callback)
		{
			Entity entity;
			if (TryGet(closeView, true, out entity)) {
				if (checkState && m_PushStates.Count > 0) {
					var type = ((UIScriptableView)entity.view).@class;
					if (m_PushStates.Exists((state) => type.Equals(state.next))) {
						ClearPushStates();
					}
				}
			}
			CloseAsync(entity, destroy, false, false, callback);
		}
		#endregion

		#region OpenAndClose
		public LuaTable OpenAndClose(LuaTable openView, LuaTable closeView)
		{
			return OpenAndClose(openView, closeView, null, false);
		}
		public LuaTable OpenAndClose(LuaTable openView, LuaTable closeView, bool destroy)
		{
			return OpenAndClose(openView, closeView, null, destroy);
		}
		public LuaTable OpenAndClose(LuaTable openView, LuaTable closeView, UIContext context)
		{
			return OpenAndClose(openView, closeView, context, false);
		}
		public LuaTable OpenAndClose(LuaTable openView, LuaTable closeView, UIContext context, bool destroy)
		{
			Close(closeView, destroy, true);
			return Open(openView, context);
		}
		public LuaTable OpenAndClose(LuaTable openView, LuaTable closeView, UIStyle style)
		{
			return OpenAndClose(openView, closeView, style, null, false);
		}
		public LuaTable OpenAndClose(LuaTable openView, LuaTable closeView, UIStyle style, bool destroy)
		{
			return OpenAndClose(openView, closeView, style, null, destroy);
		}
		public LuaTable OpenAndClose(LuaTable openView, LuaTable closeView, UIStyle style, UIContext context)
		{
			return OpenAndClose(openView, closeView, style, context, false);
		}
		public LuaTable OpenAndClose(LuaTable openView, LuaTable closeView, UIStyle style, UIContext context, bool destroy)
		{
			Close(closeView, destroy, true);
			return Open(openView, style, context);
		}
		#endregion

		#region OpenAndCloseAsync
		public void OpenAndCloseAsync(LuaTable openView, LuaTable closeView)
		{
			OpenAndCloseAsync(openView, closeView, null, false, null);
		}
		public void OpenAndCloseAsync(LuaTable openView, LuaTable closeView, Action<LuaTable> callback)
		{
			OpenAndCloseAsync(openView, closeView, null, false, callback);
		}
		public void OpenAndCloseAsync(LuaTable openView, LuaTable closeView, bool destroy)
		{
			OpenAndCloseAsync(openView, closeView, null, destroy, null);
		}
		public void OpenAndCloseAsync(LuaTable openView, LuaTable closeView, bool destroy, Action<LuaTable> callback)
		{
			OpenAndCloseAsync(openView, closeView, null, destroy, callback);
		}
		public void OpenAndCloseAsync(LuaTable openView, LuaTable closeView, UIContext context)
		{
			OpenAndCloseAsync(openView, closeView, context, false, null);
		}
		public void OpenAndCloseAsync(LuaTable openView, LuaTable closeView, UIContext context, Action<LuaTable> callback)
		{
			OpenAndCloseAsync(openView, closeView, context, false, callback);
		}
		public void OpenAndCloseAsync(LuaTable openView, LuaTable closeView, UIContext context, bool destroy)
		{
			OpenAndCloseAsync(openView, closeView, context, destroy, null);
		}
		public void OpenAndCloseAsync(LuaTable openView, LuaTable closeView, UIContext context, bool destroy, Action<LuaTable> callback)
		{
			CloseAsync(closeView, destroy, true, () => {
				OpenAsync(openView, context, callback);
			});
		}
		public void OpenAndCloseAsync(LuaTable openView, LuaTable closeView, UIStyle style)
		{
			OpenAndCloseAsync(openView, closeView, style, null, false, null);
		}
		public void OpenAndCloseAsync(LuaTable openView, LuaTable closeView, UIStyle style, Action<LuaTable> callback)
		{
			OpenAndCloseAsync(openView, closeView, style, null, false, callback);
		}
		public void OpenAndCloseAsync(LuaTable openView, LuaTable closeView, UIStyle style, bool destroy)
		{
			OpenAndCloseAsync(openView, closeView, style, null, destroy, null);
		}
		public void OpenAndCloseAsync(LuaTable openView, LuaTable closeView, UIStyle style, bool destroy, Action<LuaTable> callback)
		{
			OpenAndCloseAsync(openView, closeView, style, null, destroy, callback);
		}
		public void OpenAndCloseAsync(LuaTable openView, LuaTable closeView, UIStyle style, UIContext context)
		{
			OpenAndCloseAsync(openView, closeView, style, context, false, null);
		}
		public void OpenAndCloseAsync(LuaTable openView, LuaTable closeView, UIStyle style, UIContext context, Action<LuaTable> callback)
		{
			OpenAndCloseAsync(openView, closeView, style, context, false, callback);
		}
		public void OpenAndCloseAsync(LuaTable openView, LuaTable closeView, UIStyle style, UIContext context, bool destroy)
		{
			OpenAndCloseAsync(openView, closeView, style, context, destroy, null);
		}
		public void OpenAndCloseAsync(LuaTable openView, LuaTable closeView, UIStyle style, UIContext context, bool destroy, Action<LuaTable> callback)
		{
			CloseAsync(closeView, destroy, true, () => {
				OpenAsync(openView, style, context, callback);
			});
		}
		#endregion

		#region OpenOrClose
		public void OpenOrClose(LuaTable view)
		{
			OpenOrClose(view, null, false);
		}
		public void OpenOrClose(LuaTable view, bool destroy)
		{
			OpenOrClose(view, null, destroy);
		}
		public void OpenOrClose(LuaTable view, UIContext context)
		{
			OpenOrClose(view, context, false);
		}
		public void OpenOrClose(LuaTable view, UIContext context, bool destroy)
		{
			if (Exists(view, false)) {
				Close(view, destroy, true);
			} else {
				Open(view, context);
			}
		}
		public void OpenOrClose(LuaTable view, UIStyle style)
		{
			OpenOrClose(view, style, null, false);
		}
		public void OpenOrClose(LuaTable view, UIStyle style, bool destroy)
		{
			OpenOrClose(view, style, null, destroy);
		}
		public void OpenOrClose(LuaTable view, UIStyle style, UIContext context)
		{
			OpenOrClose(view, style, context, false);
		}
		public void OpenOrClose(LuaTable view, UIStyle style, UIContext context, bool destroy)
		{
			if (Exists(view, false)) {
				Close(view, destroy, true);
			} else {
				Open(view, style, context);
			}
		}
		#endregion

		#region OpenOrCloseAsync
		public void OpenOrCloseAsync(LuaTable view)
		{
			OpenOrCloseAsync(view, null, false);
		}
		public void OpenOrCloseAsync(LuaTable view, bool destroy)
		{
			OpenOrCloseAsync(view, null, destroy);
		}
		public void OpenOrCloseAsync(LuaTable view, UIContext context)
		{
			OpenOrCloseAsync(view, context, false);
		}
		public void OpenOrCloseAsync(LuaTable view, UIContext context, bool destroy)
		{
			if (Exists(view, false)) {
				CloseAsync(view, destroy, true, null);
			} else {
				OpenAsync(view, context);
			}
		}
		public void OpenOrCloseAsync(LuaTable view, UIStyle style)
		{
			OpenOrCloseAsync(view, style, null, false);
		}
		public void OpenOrCloseAsync(LuaTable view, UIStyle style, bool destroy)
		{
			OpenOrCloseAsync(view, style, null, destroy);
		}
		public void OpenOrCloseAsync(LuaTable view, UIStyle style, UIContext context)
		{
			OpenOrCloseAsync(view, style, context, false);
		}
		public void OpenOrCloseAsync(LuaTable view, UIStyle style, UIContext context, bool destroy)
		{
			if (Exists(view, false)) {
				CloseAsync(view, destroy, true, null);
			} else {
				OpenAsync(view, style, context);
			}
		}
		#endregion

		#region OpenAndPush
		public LuaTable OpenAndPush(LuaTable openView, LuaTable pushView)
		{
			return OpenAndPush(openView, pushView, null, false);
		}
		public LuaTable OpenAndPush(LuaTable openView, LuaTable pushView, bool destroy)
		{
			return OpenAndPush(openView, pushView, null, destroy);
		}
		public LuaTable OpenAndPush(LuaTable openView, LuaTable pushView, UIContext context)
		{
			return OpenAndPush(openView, pushView, context, false);
		}
		public LuaTable OpenAndPush(LuaTable openView, LuaTable pushView, UIContext context, bool destroy)
		{
			Entity entity;
			if (TryGet(pushView, false, out entity)) {
				m_PushStates.Add(new State() { type = ((UIScriptableView)entity.view).@class, context = entity.context, style = entity.style, next = openView });
				Close(entity, destroy, false, true);
			}
			return Open(openView, context);
		}
		public LuaTable OpenAndPush(LuaTable openView, LuaTable pushView, UIStyle style)
		{
			return OpenAndPush(openView, pushView, style, null, false);
		}
		public LuaTable OpenAndPush(LuaTable openView, LuaTable pushView, UIStyle style, bool destroy)
		{
			return OpenAndPush(openView, pushView, style, null, destroy);
		}
		public LuaTable OpenAndPush(LuaTable openView, LuaTable pushView, UIStyle style, UIContext context)
		{
			return OpenAndPush(openView, pushView, style, context, false);
		}
		public LuaTable OpenAndPush(LuaTable openView, LuaTable pushView, UIStyle style, UIContext context, bool destroy)
		{
			Entity entity;
			if (TryGet(pushView, false, out entity)) {
				m_PushStates.Add(new State() { type = ((UIScriptableView)entity.view).@class, context = entity.context, style = entity.style, next = openView });
				Close(entity, destroy, false, true);
			}
			return Open(openView, style, context);
		}
		#endregion

		#region OpenAndPushAsync
		public void OpenAndPushAsync(LuaTable openView, LuaTable pushView)
		{
			OpenAndPushAsync(openView, pushView, null, false, null);
		}
		public void OpenAndPushAsync(LuaTable openView, LuaTable pushView, Action<LuaTable> callback)
		{
			OpenAndPushAsync(openView, pushView, null, false, callback);
		}
		public void OpenAndPushAsync(LuaTable openView, LuaTable pushView, bool destroy)
		{
			OpenAndPushAsync(openView, pushView, null, destroy, null);
		}
		public void OpenAndPushAsync(LuaTable openView, LuaTable pushView, bool destroy, Action<LuaTable> callback)
		{
			OpenAndPushAsync(openView, pushView, null, destroy, callback);
		}
		public void OpenAndPushAsync(LuaTable openView, LuaTable pushView, UIContext context)
		{
			OpenAndPushAsync(openView, pushView, context, false, null);
		}
		public void OpenAndPushAsync(LuaTable openView, LuaTable pushView, UIContext context, Action<LuaTable> callback)
		{
			OpenAndPushAsync(openView, pushView, context, false, callback);
		}
		public void OpenAndPushAsync(LuaTable openView, LuaTable pushView, UIContext context, bool destroy)
		{
			OpenAndPushAsync(openView, pushView, context, destroy, null);
		}
		public void OpenAndPushAsync(LuaTable openView, LuaTable pushView, UIContext context, bool destroy, Action<LuaTable> callback)
		{
			Entity entity;
			if (TryGet(pushView, false, out entity)) {
				m_PushStates.Add(new State() { type = ((UIScriptableView)entity.view).@class, context = entity.context, style = entity.style, next = openView });
				CloseAsync(entity, destroy, false, true, () => {
					OpenAsync(openView, context, callback);
				});
			} else {
				OpenAsync(openView, context, callback);
			}
		}
		public void OpenAndPushAsync(LuaTable openView, LuaTable pushView, UIStyle style)
		{
			OpenAndPushAsync(openView, pushView, style, null, false, null);
		}
		public void OpenAndPushAsync(LuaTable openView, LuaTable pushView, UIStyle style, Action<LuaTable> callback)
		{
			OpenAndPushAsync(openView, pushView, style, null, false, callback);
		}
		public void OpenAndPushAsync(LuaTable openView, LuaTable pushView, UIStyle style, bool destroy)
		{
			OpenAndPushAsync(openView, pushView, style, null, destroy, null);
		}
		public void OpenAndPushAsync(LuaTable openView, LuaTable pushView, UIStyle style, bool destroy, Action<LuaTable> callback)
		{
			OpenAndPushAsync(openView, pushView, style, null, destroy, callback);
		}
		public void OpenAndPushAsync(LuaTable openView, LuaTable pushView, UIStyle style, UIContext context)
		{
			OpenAndPushAsync(openView, pushView, style, context, false, null);
		}
		public void OpenAndPushAsync(LuaTable openView, LuaTable pushView, UIStyle style, UIContext context, Action<LuaTable> callback)
		{
			OpenAndPushAsync(openView, pushView, style, context, false, callback);
		}
		public void OpenAndPushAsync(LuaTable openView, LuaTable pushView, UIStyle style, UIContext context, bool destroy)
		{
			OpenAndPushAsync(openView, pushView, style, context, destroy, null);
		}
		public void OpenAndPushAsync(LuaTable openView, LuaTable pushView, UIStyle style, UIContext context, bool destroy, Action<LuaTable> callback)
		{
			Entity entity;
			if (TryGet(pushView, false, out entity)) {
				m_PushStates.Add(new State() { type = ((UIScriptableView)entity.view).@class, context = entity.context, style = entity.style, next = openView });
				CloseAsync(entity, destroy, false, true, () => {
					OpenAsync(openView, style, context, callback);
				});
			} else {
				OpenAsync(openView, style, context, callback);
			}
		}
		#endregion

		#region CloseAndBack
		public LuaTable CloseAndBack(LuaTable closeView)
		{
			return CloseAndBack(closeView, false);
		}
		public LuaTable CloseAndBack(LuaTable closeView, bool destroy)
		{
			Close(closeView, destroy, false);

			if (m_PushStates.Count > 0) {
				var data = m_PushStates[m_PushStates.Count - 1];
				if (object.Equals(data.next, closeView.Get<LuaTable>("class") ?? closeView)) {
					m_PushStates.RemoveAt(m_PushStates.Count - 1);
					return OpenView((LuaTable)data.type, data.style, data.context);
				}
			}

			return null;
		}
		public LuaTable CloseAndBack(LuaTable closeView, LuaTable backView)
		{
			return CloseAndBack(closeView, backView, false);
		}
		public LuaTable CloseAndBack(LuaTable closeView, LuaTable backView, bool destroy)
		{
			Close(closeView, destroy, false);

			if (m_PushStates.Count > 0) {
				if (object.Equals(m_PushStates[m_PushStates.Count - 1].next, closeView.Get<LuaTable>("class") ?? closeView)) {
					int index = m_PushStates.FindLastIndex((data) => backView.Equals(data.type));
					if (index >= 0) {
						var data = m_PushStates[index];
						ClearPushStates(index);
						return OpenView((LuaTable)data.type, data.style, data.context);
					}
				}
			}

			return null;
		}
		#endregion

		#region CloseAndBackAsync
		public void CloseAndBackAsync(LuaTable closeView)
		{
			CloseAndBackAsync(closeView, false, null);
		}
		public void CloseAndBackAsync(LuaTable closeView, Action<LuaTable> callback)
		{
			CloseAndBackAsync(closeView, false, callback);
		}
		public void CloseAndBackAsync(LuaTable closeView, bool destroy)
		{
			CloseAndBackAsync(closeView, destroy, null);
		}
		public void CloseAndBackAsync(LuaTable closeView, bool destroy, Action<LuaTable> callback)
		{
			CloseAsync(closeView, destroy, false, () => {
				if (m_PushStates.Count > 0) {
					var data = m_PushStates[m_PushStates.Count - 1];
					if (object.Equals(data.next, closeView.Get<LuaTable>("class") ?? closeView)) {
						m_PushStates.RemoveAt(m_PushStates.Count - 1);
						OpenViewAsync((LuaTable)data.type, data.style, data.context, callback);
					}
				}
			});
		}
		public void CloseAndBackAsync(LuaTable closeView, LuaTable backView)
		{
			CloseAndBackAsync(closeView, backView, false, null);
		}
		public void CloseAndBackAsync(LuaTable closeView, LuaTable backView, Action<LuaTable> callback)
		{
			CloseAndBackAsync(closeView, backView, false, callback);
		}
		public void CloseAndBackAsync(LuaTable closeView, LuaTable backView, bool destroy)
		{
			CloseAndBackAsync(closeView, backView, destroy, null);
		}
		public void CloseAndBackAsync(LuaTable closeView, LuaTable backView, bool destroy, Action<LuaTable> callback)
		{
			CloseAsync(closeView, destroy, false, () => {
				if (m_PushStates.Count > 0) {
					if (object.Equals(m_PushStates[m_PushStates.Count - 1].next, closeView.Get<LuaTable>("class") ?? closeView)) {
						int index = m_PushStates.FindLastIndex((data) => backView.Equals(data.type));
						if (index >= 0) {
							var data = m_PushStates[index];
							ClearPushStates(index);
							OpenViewAsync((LuaTable)data.type, data.style, data.context, callback);
						}
					}
				}
			});
		}
		#endregion

		#region Exist
		public bool Exists(string ident)
		{
			return Exists(ident, false);
		}
		public bool Exists(string ident, bool checkInvisible)
		{
			Entity entity;
			return TryGet(ident, checkInvisible, out entity);
		}
		public bool Exists(LuaTable table)
		{
			return Exists(table, false);
		}
		public bool Exists(LuaTable table, bool checkInvisible)
		{
			Entity entity;
			return TryGet(table, checkInvisible, out entity);
		}
		#endregion

		#region Get
		public LuaTable Get(string ident)
		{
			return Get(ident, false);
		}
		public LuaTable Get(string ident, bool checkInvisible)
		{
			Entity entity;
			if (TryGet(ident, checkInvisible, out entity)) {
				return ((UIScriptableView)entity.view).@this;
			}
			return null;
		}
		public object Get(LuaTable table)
		{
			return Get(table, false);
		}
		public object Get(LuaTable table, bool checkInvisible)
		{
			Entity entity;
			if (TryGet(table, checkInvisible, out entity)) {
				UIView view = entity.view;
				if (view.GetType() == typeof(UIScriptableView)) {
					return ((UIScriptableView)view).@this;
				} else {
					return view;
				}
			}
			return null;
		}

		public object GetTopView()
		{
			var view = GetTopView<UIView>();
			if (view != null) {
				if (view.GetType() == typeof(UIScriptableView)) {
					return ((UIScriptableView)view).@this;
				}
			}
			return view;
		}
		#endregion

		public bool IsFocused(LuaTable view)
		{
			Entity entity;
			if (TryGet(view, true, out entity)) {
				return entity.focused;
			}
			return false;
		}

		public bool IsOnTop(LuaTable view)
		{
			Entity entity;
			if (TryGet(view, true, out entity)) {
				if (entity.phase == Entity.Phase.Running) {
					bool ret = false;
					for (int i = m_Entities.Count - 1; i >= 0; i--) {
						Entity e = m_Entities[i];
						if (e.phase == Entity.Phase.Running) {
							if (ret) {
								if (e.style.Value.topmost)
									return false;
							} else if (e == entity) {
								if (e.style.Value.topmost)
									return true;
								ret = true;
							}
						}
					}
					return ret;
				}
			}
			return false;
		}

		private bool TryGet(string ident, bool checkInvisible, out Entity entity)
		{
			var type = typeof(UIScriptableView);
			for (int i = m_Entities.Count - 1; i >= 0; i--) {
				entity = m_Entities[i];
				if (entity.view.GetType() == type) {
					if (checkInvisible || entity.phase != Entity.Phase.Closing) {
						UIScriptableView v = (UIScriptableView)entity.view;
						if (v.ident == ident)
							return true;
					}
				}
			}
			entity = null;
			return false;
		}

		private bool TryGet(LuaTable table, bool checkInvisible, out Entity entity)
		{
			if (table != null) {
				bool ret;
				IntPtr L = table.L;
				int top = LuaDLL.lua_gettop(L);
				LuaDLL.lua_getref(L, table.Ref);
				LuaDLL.lua_pushstring(L, "class");
				LuaDLL.lua_rawget(L, -2);
				if (!LuaDLL.lua_istable(L, -1)) {
					LuaDLL.lua_pop(L, 1);
				}
				LuaDLL.lua_pushstring(L, "name");
				LuaDLL.lua_rawget(L, -2);
				ret = TryGet(LuaDLL.lua_tostring(L, -1), checkInvisible, out entity);
				LuaDLL.lua_settop(L, top);
				return ret;
			}

			entity = null;
			return false;
		}
	}
}
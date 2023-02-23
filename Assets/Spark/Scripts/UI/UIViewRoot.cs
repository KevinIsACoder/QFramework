using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XLua;

namespace Spark
{
    [RequireComponent(typeof(RectTransform))]
    public partial class UIViewRoot : UIBehaviour
    {
        private struct State
        {
            public object type; // Type or LuaTable
            public object next; // 下一个界面的类型：Type or LuaTable
            public UIStyle? style;
            public UIContext context;
        }

        private class Entity
        {
            public enum Phase
            {
                Opening,
                Running,
                Closing
            }

            public string name;
            public Layer container;
            public UIView view;
            public Phase? phase;
            public bool focused;
            public UIStyle? style;
            public UIContext context;
            // async vars
            public bool blocking;
            public Coroutine coroutine;
            public ViewLayer placeholder;
            public Assets.Async<GameObject> async;
        }

        internal enum InvisibleMode
        {
            SetActive,
            SetLayer
        }

        internal enum MaskMode
        {
            CameraBlur,
            ColorLayer,
        }

        public Vector2 DesignResolution
        {
            get { return m_DesignResolution; }
            set { m_DesignResolution = value; }
        }

        private GameObject m_RootCanvasGameObject;

        [SerializeField]
        private Camera m_Camera;

        [SerializeField]
        private int m_MaxDepth = 0;

        [SerializeField]
        private Vector2 m_DesignResolution = new Vector2(720, 1280);

        [SerializeField]
        private InvisibleMode m_InvisibleMode = InvisibleMode.SetActive;

        [SerializeField]
        private int m_InvisibleLayer = 0;

        [SerializeField]
        private MaskMode m_MaskMode = MaskMode.ColorLayer;

        [SerializeField]
        private Camera m_BlurCamera;

        [SerializeField]
        private Color m_DefaultMaskColor = new Color(0, 0, 0, 0.5f);

        private bool m_EntityViewChanged = false;
        private ObjectPool<Layer> m_ContainerPool;
        private ObjectPool<ViewLayer> m_PlaceholderPool;
        private List<Entity> m_Entities = new List<Entity>();
        private List<State> m_PushStates = new List<State>();

        private bool? m_IsRootView;
        public bool isRootView
        {
            get
            {
                if (m_IsRootView.HasValue) return m_IsRootView.Value;
                var value = true;
                var parent = transform.parent;
                if (parent != null)
                {
                    var root = parent.GetComponentInParent<UIViewRoot>();
                    value = root == null;
                }
                m_IsRootView = value;
                return value;
            }
        }

        protected override void Awake()
        {
            m_ContainerPool = new ObjectPool<Layer>(
                () => {
                    var layer = Layer.Create<Layer>(this);
                    var go = layer.gameObject;
                    go.AddComponent<GraphicRaycaster>();
                    return layer;
                },
                (layer) => layer.Show(),
                (layer) => layer.Hide()
            );
            m_PlaceholderPool = new ObjectPool<ViewLayer>(
                () => Layer.Create<ViewLayer>(this),
                (layer) => layer.Show(),
                (layer) => layer.Hide()
            );

            // Initialize m_RootCanvasGameObject
            m_RootCanvasGameObject = new GameObject("[INTERNAL_CANVAS_PREFAB]");
            m_RootCanvasGameObject.layer = gameObject.layer;
            var rect = m_RootCanvasGameObject.AddComponent<RectTransform>();
            rect.SetParent(transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            var canvas = m_RootCanvasGameObject.AddComponent<Canvas>();
            if (isRootView)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = m_Camera;

                var scaler = m_RootCanvasGameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0;
                scaler.referenceResolution = m_DesignResolution;
            }
        }

        public void SetDesignResolution(int w, int h)
        {
            m_DesignResolution = new Vector2(w, h);
            var scaler = m_RootCanvasGameObject.GetComponent<CanvasScaler>();
            scaler.matchWidthOrHeight = w > h ? 1 : 0;
            scaler.referenceResolution = m_DesignResolution;
        }
        protected override void Start()
        {
            if (isRootView)
            {
                if (m_SafeAreaLayer == null)
                {
                    m_SafeAreaLayer = Layer.Create<Layer>(this);
                    m_SafeAreaLayer.gameObject.name = "[INTERNAL]";
                }
            }
        }

        #region Entity Functions
        private Entity CreateEntity(Type type, string ident, UIStyle? style, UIContext context)
        {
            UIView view;
            Entity entity;
            if (
#if USE_LUA
                !string.IsNullOrEmpty(ident) ? TryGet(ident, true, out entity) :
#endif
                TryGet(type, true, out entity))
            {
                view = entity.view;
                if (entity.coroutine != null)
                {
                    StopCoroutine(entity.coroutine);
                    entity.coroutine = null;
                }
                else if (view.setting != null)
                {
                    // complete active tween
                    view.setting.CompleteActiveAnimations();
                }
                m_Entities.Remove(entity);
            }
            else
            {
                view = (UIView)Activator.CreateInstance(type);
                view.root = this;
                entity = new Entity() { view = view };
            }
            entity.name = view is UIScriptableView ? ((UIScriptableView)view).ident : view.GetType().Name;
            entity.style = style;
            entity.blocking = false;
            if (entity.context == null)
            {
                entity.context = context ?? UIContext.Get();
            }
            if (!entity.phase.HasValue || entity.phase == Entity.Phase.Closing)
            {
                entity.phase = Entity.Phase.Opening;
            }
            m_Entities.Add(entity);
            return entity;
        }
        private void OpenEntity(Entity entity, GameObject prefab)
        {
            CleanEntity(entity);
            var view = entity.view;
            if (prefab != null)
            {
                entity.container = m_ContainerPool.Get();
                entity.container.name = entity.name;
                UIView.InternalCreated(view, prefab, entity.container.transform, false);
                // if (isRootView)
                // {
                //     var comps = view.gameObject.GetComponents<UISafeArea>();
                //     if (comps == null || comps.Length == 0)
                //     {
                //         view.gameObject.AddComponent<UISafeArea>();
                //     }
                // }
            }

            //// complete active tween
            //view.setting.CompleteActiveAnimations();

            // apply style
            var style = entity.style.HasValue ? entity.style.Value : view.setting.style;
            if (!style.overrideColor)
            {
                style.maskColor = m_DefaultMaskColor;
            }
            entity.style = view.setting.style = style;
            SetEntityVisible(entity, true);

            // opening
            if (entity.phase == Entity.Phase.Opening)
            {
                UIView.InternalOpened(view, entity.context);
            }

            // running
            if (entity.phase != Entity.Phase.Closing)
            {
                entity.phase = Entity.Phase.Running;
            }
            m_EntityViewChanged = true;
        }
        private void CleanEntity(Entity entity)
        {
            if (entity.coroutine != null)
            {
                StopCoroutine(entity.coroutine);
                entity.coroutine = null;
            }
            entity.async = null;
            if (entity.placeholder != null)
            {
                m_PlaceholderPool.Release(entity.placeholder);
                entity.placeholder = null;
            }
        }
        private void DestroyEntity(Entity entity)
        {
            CleanEntity(entity);
            var view = entity.view;
            m_Entities.Remove(entity);
            if (view.gameObject != null)
            {
                entity.container.name = "__POOL_OBJ";
                m_ContainerPool.Release(entity.container);
                Destroy(entity.view.gameObject);
                UIView.InternalDestroyed(view);
            }
        }

        public void DestroyContainerPool()
        {
            m_ContainerPool.Dispose();
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.name == "__POOL_OBJ")
                {
                    Destroy(child.gameObject);
                }
            }
            m_ContainerPool = new ObjectPool<Layer>(
                () => {
                    var layer = Layer.Create<Layer>(this);
                    var go = layer.gameObject;
                    go.AddComponent<GraphicRaycaster>();
                    return layer;
                },
                (layer) => layer.Show(),
                (layer) => layer.Hide()
            );
        }

        private void SetStyle(Entity entity, UIStyle style)
        {
            if (entity != null)
            {
                entity.style = style;
                m_EntityViewChanged = true;
            }
        }
        [BlackList]
        public void SetStyle<T>(UIStyle style)
        {
            Entity entity = null;
            if (TryGet(typeof(T), false, out entity))
            {
                SetStyle(entity, style);
            }
        }

        #endregion

        #region Open
        //		public void Preload<T>()
        //			where T : UIView, new()
        //		{
        //			Preload(typeof(T), null);
        //		}

        //		private void Preload(Type type, string ident)
        //		{
        //			Entity entity;
        //			if (
        //#if USE_LUA
        //				!string.IsNullOrEmpty(ident) ? TryGet(ident, true, out entity) :
        //#endif
        //				TryGet(type, true, out entity)) {
        //				return;
        //			}

        //			UIView view = (UIView)Activator.CreateInstance(type);
        //			view.root = this;
        //			UIView.InternalCreated(view, Assets.LoadAsset<GameObject>(view.prefabPath));
        //			view.transform.SetParent(transform, false);
        //			if (view.transform.GetComponent<Canvas>() == null) {
        //				view.gameObject.AddComponent<Canvas>();
        //			}
        //			if (view.transform.GetComponent<GraphicRaycaster>() == null) {
        //				view.gameObject.AddComponent<GraphicRaycaster>();
        //			}
        //			view.transform.SetAsFirstSibling();
        //			foreach (var stack in view.transform.GetComponentsInChildren<UIViewStack>(true)) {
        //				stack.Preload();
        //			}
        //			m_Entities.Add(new Entity() { view = view });
        //			SetViewVisible(view, false);
        //		}

        [BlackList]
        public T Open<T>(UIContext context = null)
            where T : UIView, new()
        {
            return (T)Open(typeof(T), null, null, context);
        }
        [BlackList]
        public T Open<T>(UIStyle style, UIContext context = null)
            where T : UIView, new()
        {
            return (T)Open(typeof(T), null, style, context);
        }
        private UIView Open(Type type, string ident, UIStyle? style, UIContext context)
        {
            Entity entity = CreateEntity(type, ident, style, context);
            GameObject prefab = null;
            UIView view = entity.view;
            if (view.gameObject == null)
            {
                prefab = Assets.LoadAsset<GameObject>(view.prefabPath);
            }
            OpenEntity(entity, prefab);
            view.setting.PlayOpenAnimations(null);
            view.setting.CompleteActiveAnimations();

            return view;
        }
        #endregion

        #region OpenAsync
        //public IEnumerator OpenAsync<T>(UIContext context = null)
        //	where T : UIView, new()
        //{
        //	return OpenAsync<T>(null, null, context);
        //}
        //public IEnumerator OpenAsync<T>(UIStyle style, UIContext context = null)
        //	where T : UIView, new()
        //{
        //	return OpenAsync<T>(null, style, context);
        //}
        //private IEnumerator OpenAsync<T>(string ident, UIStyle? style, UIContext context)
        //	where T : UIView, new()
        //{
        //	return OpenAsync<T>(CreateEntity(typeof(T), ident, style, context), null);
        //}

        [BlackList]
        public void OpenAsync<T>(Action<T> callback)
            where T : UIView, new()
        {
            OpenAsync(null, null, null, callback);
        }
        [BlackList]
        public void OpenAsync<T>(UIContext context, Action<T> callback)
            where T : UIView, new()
        {
            OpenAsync(null, null, context, callback);
        }
        [BlackList]
        public void OpenAsync<T>(UIStyle style, Action<T> callback)
            where T : UIView, new()
        {
            OpenAsync(null, style, null, callback);
        }
        [BlackList]
        public void OpenAsync<T>(UIStyle style, UIContext context, Action<T> callback)
            where T : UIView, new()
        {
            OpenAsync(null, style, context, callback);
        }
        private void OpenAsync<T>(string ident, UIStyle? style, UIContext context, Action<T> callback)
            where T : UIView, new()
        {
            OpenAsync(typeof(T), ident, style, context, callback);
        }
        private void OpenAsync<T>(Type type, string ident, UIStyle? style, UIContext context, Action<T> callback)
            where T : UIView
        {
            Entity entity = CreateEntity(type, ident, style, context);
            var coroutine = StartCoroutine(OpenAsync(entity, callback));
            if (entity.async != null)
            {
                entity.coroutine = coroutine;
            }
        }
        private IEnumerator OpenAsync<T>(Entity entity, Action<T> callback)
            where T : UIView
        {
            GameObject prefab = null;
            UIView view = entity.view;
            if (view.gameObject == null)
            {
                if (entity.placeholder == null)
                {
                    entity.placeholder = m_PlaceholderPool.Get();
                }
                // entity.placeholder.Show();
                entity.blocking = true;
                if (entity.async == null)
                {
                    entity.async = Assets.LoadAssetAsync<GameObject>(view.prefabPath);
                }
                // 标记变化，在Update中处理层次
                m_EntityViewChanged = true;
                yield return entity.async;
                entity.blocking = false;
                prefab = entity.async.asset;
            }

            // 保存coroutine
            var isOpening = entity.phase == Entity.Phase.Opening;

            OpenEntity(entity, prefab);

            if (isOpening)
            {
                entity.blocking = true;
                entity.placeholder = m_PlaceholderPool.Get();
                // entity.placeholder.Show();
                entity.view.setting.PlayOpenAnimations(() =>
                {
                    if (entity.placeholder != null)
                    {
                        m_PlaceholderPool.Release(entity.placeholder);
                        entity.placeholder = null;
                    }
                    entity.blocking = false;
                    if (callback != null)
                    {
                        callback((T)view);
                    }
                });
            }
            else
            {
                if (callback != null)
                {
                    callback((T)view);
                }
            }
        }
        #endregion

        #region Close
        [BlackList]
        public void Close<T>(bool destroy = false)
            where T : UIView
        {
            Close<T>(destroy, true);
        }

        private void Close<T>(bool destroy, bool checkState)
            where T : UIView
        {
            Entity entity;
            if (TryGet(typeof(T), false, out entity))
            {
                Close(entity, destroy, checkState, false);
            }
        }

        [BlackList]
        public void Close(UIView view, bool destroy = false)
        {
            Close(view, destroy, true);
        }

        private void Close(UIView view, bool destroy, bool checkState)
        {
            for (int i = m_Entities.Count - 1; i >= 0; i--)
            {
                Entity entity = m_Entities[i];
                if (entity.view == view)
                {
                    Close(entity, destroy, checkState, false);
                    return;
                }
            }
        }

        public void CloseAll(bool destroy)
        {
            if (destroy)
            {
                while (m_Entities.Count > 0)
                {
                    Close(m_Entities.Last(), destroy, false, false);
                }
            }
            else
            {
                while (m_Entities.Count > 0)
                {
                    int count = m_Entities.Count;
                    do
                    {
                        Entity entity = m_Entities[--count];
                        if (entity.phase == Entity.Phase.Closing)
                        {
                            if (count > 0)
                                continue;
                            return;
                        }
                        Close(entity, destroy, false, false);
                        break;
                    } while (true);
                }
            }
            ClearPushStates();
        }

        [BlackList]
        public void DestroyAll(params Type[] excludeTypes)
        {
            if (excludeTypes.Length == 0)
            {
                CloseAll(true);
            }
            else
            {
                while (m_Entities.Count > 0)
                {
                    int count = m_Entities.Count;
                    do
                    {
                        Entity entity = m_Entities[--count];
                        var type = entity.view.GetType();
                        var excluded = Array.Exists(excludeTypes, (t) => t == type);
                        if (excluded)
                        {
                            if (entity.phase == Entity.Phase.Closing)
                            {
                                if (count > 0)
                                    continue;
                                return;
                            }
                        }
                        Close(entity, !excluded, false, false);
                        break;
                    } while (true);
                }
                ClearPushStates();
            }
        }

        private void Close(Entity entity, bool destroy, bool checkState, bool pushState)
        {
            if (entity.phase == Entity.Phase.Closing)
            {
                if (destroy)
                {
                    DestroyEntity(entity);
                }
                return;
            }

            var context = entity.context;
            entity.context = null;
            entity.phase = Entity.Phase.Closing;

            var view = entity.view;

            if (view.gameObject != null)
            {
                view.setting.CompleteActiveAnimations();
                if (entity.focused)
                {
                    entity.focused = false;
                    UIView.InternalFocusChanged(view, false);
                }
                m_EntityViewChanged = true;
                if (entity.phase != Entity.Phase.Closing)
                    return;
                UIView.InternalClosed(view);
                // 处理SubViewRoot的清理工作
                var subViewRoots = view.gameObject.GetComponentsInChildren<UIViewRoot>(true);
                foreach(var subViewRoot in subViewRoots)
                {
                    subViewRoot.CloseAll(destroy);
                }
            }
            else
            {
                CleanEntity(entity);
            }

            if (!pushState)
            {
                UIContext.Release(context);
            }

            if (entity.phase == Entity.Phase.Closing)
            {
                if (destroy)
                {
                    DestroyEntity(entity);
                }
                else if (view.gameObject != null)
                {
                    SetEntityVisible(entity, false);
                }
            }

            if (checkState && m_PushStates.Count > 0)
            {
                var type = view.GetType();
                if (m_PushStates.Exists((state) => type == state.next.GetType()))
                {
                    ClearPushStates();
                }
            }
        }
        #endregion

        #region CloseAsync
        [BlackList]
        public void CloseAsync<T>(bool destroy = false)
            where T : UIView
        {
            CloseAsync<T>(destroy, true, null);
        }
        [BlackList]
        public void CloseAsync<T>(bool destroy, Action callback)
            where T : UIView
        {
            CloseAsync<T>(destroy, true, callback);
        }
        private void CloseAsync<T>(bool destroy, bool checkState)
            where T : UIView
        {
            CloseAsync<T>(destroy, checkState, null);
        }
        [BlackList]
        public void CloseAsync<T>(bool destroy, bool checkState, Action callback)
            where T : UIView
        {
            Entity entity = null;
            TryGet(typeof(T), false, out entity);
            CloseAsync(entity, destroy, false, false, callback);
        }

        [BlackList]
        public void CloseAsync(UIView view, bool destroy = false)
        {
            CloseAsync(view, destroy, true, null);
        }
        public void CloseAsync(UIView view, Action callback)
        {
            CloseAsync(view, false, true, callback);
        }
        public void CloseAsync(UIView view, bool destroy, Action callback)
        {
            CloseAsync(view, destroy, true, callback);
        }
        private void CloseAsync(UIView view, bool destroy, bool checkState, Action callback)
        {
            for (int i = m_Entities.Count - 1; i >= 0; i--)
            {
                Entity entity = m_Entities[i];
                if (entity.view == view)
                {
                    CloseAsync(entity, destroy, checkState, false, callback);
                    return;
                }
            }
            if (callback != null)
            {
                callback();
            }
        }

        private void CloseAsync(Entity entity, bool destroy, bool checkState, bool pushState, Action callback)
        {
            if (entity != null && entity.view != null && entity.view.gameObject != null)
            {
                entity.view.setting.CompleteActiveAnimations();

                if (entity.phase != Entity.Phase.Closing)
                {
                    entity.blocking = true;
                    entity.placeholder = m_PlaceholderPool.Get();
                    // entity.placeholder.Show();
                    entity.view.setting.PlayCloseAnimations(() =>
                    {
                        if (entity.placeholder != null)
                        {
                            m_PlaceholderPool.Release(entity.placeholder);
                            entity.placeholder = null;
                        }
                        entity.blocking = false;
                        Close(entity, destroy, checkState, pushState);
                        if (callback != null)
                        {
                            callback();
                        }
                    });
                    return;
                }
                Close(entity, destroy, checkState, pushState);
            }
            if (callback != null)
            {
                callback();
            }
        }
        #endregion

        #region OpenAndClose
        [BlackList]
        public TOpen OpenAndClose<TOpen, TClose>(bool destroy = false)
            where TOpen : UIView, new()
            where TClose : UIView
        {
            return OpenAndClose<TOpen, TClose>(null, destroy);
        }
        [BlackList]
        public TOpen OpenAndClose<TOpen, TClose>(UIContext context, bool destroy = false)
            where TOpen : UIView, new()
            where TClose : UIView
        {
            Close<TClose>(destroy, true);
            return Open<TOpen>(context);
        }
        [BlackList]
        public TOpen OpenAndClose<TOpen, TClose>(UIStyle style, bool destroy = false)
            where TOpen : UIView, new()
            where TClose : UIView
        {
            return OpenAndClose<TOpen, TClose>(style, null, destroy);
        }
        [BlackList]
        public TOpen OpenAndClose<TOpen, TClose>(UIStyle style, UIContext context, bool destroy = false)
            where TOpen : UIView, new()
            where TClose : UIView
        {
            Close<TClose>(destroy, true);
            return Open<TOpen>(style, context);
        }
        #endregion

        #region OpenAndCloseAsync
        //public IEnumerator OpenAndCloseAsync<TOpen, TClose>(bool destroy = false)
        //	where TOpen : UIView, new()
        //	where TClose : UIView
        //{
        //	return OpenAndCloseAsync<TOpen, TClose>(null, destroy);
        //}
        //public IEnumerator OpenAndCloseAsync<TOpen, TClose>(UIContext context, bool destroy = false)
        //	where TOpen : UIView, new()
        //	where TClose : UIView
        //{
        //	Close<TClose>(destroy, true);
        //	return OpenAsync<TOpen>(context);
        //}
        //public IEnumerator OpenAndCloseAsync<TOpen, TClose>(UIStyle style, bool destroy = false)
        //	where TOpen : UIView, new()
        //	where TClose : UIView
        //{
        //	return OpenAndCloseAsync<TOpen, TClose>(style, null, destroy);
        //}
        //public IEnumerator OpenAndCloseAsync<TOpen, TClose>(UIStyle style, UIContext context, bool destroy = false)
        //	where TOpen : UIView, new()
        //	where TClose : UIView
        //{
        //	Close<TClose>(destroy, true);
        //	return OpenAsync<TOpen>(style, context);
        //}
        [BlackList]
        public void OpenAndCloseAsync<TOpen, TClose>()
            where TOpen : UIView, new()
            where TClose : UIView
        {
            OpenAndCloseAsync<TOpen, TClose>(null, true, null);
        }
        [BlackList]
        public void OpenAndCloseAsync<TOpen, TClose>(Action<TOpen> callback)
            where TOpen : UIView, new()
            where TClose : UIView
        {
            OpenAndCloseAsync<TOpen, TClose>(null, true, callback);
        }
        [BlackList]
        public void OpenAndCloseAsync<TOpen, TClose>(bool destroy)
           where TOpen : UIView, new()
           where TClose : UIView
        {
            OpenAndCloseAsync<TOpen, TClose>(null, destroy, null);
        }
        [BlackList]
        public void OpenAndCloseAsync<TOpen, TClose>(bool destroy, Action<TOpen> callback)
            where TOpen : UIView, new()
            where TClose : UIView
        {
            OpenAndCloseAsync<TOpen, TClose>(null, destroy, callback);
        }
        [BlackList]
        public void OpenAndCloseAsync<TOpen, TClose>(UIContext context)
            where TOpen : UIView, new()
            where TClose : UIView
        {
            OpenAndCloseAsync<TOpen, TClose>(context, true, null);
        }
        [BlackList]
        public void OpenAndCloseAsync<TOpen, TClose>(UIContext context, Action<TOpen> callback)
            where TOpen : UIView, new()
            where TClose : UIView
        {
            OpenAndCloseAsync<TOpen, TClose>(context, true, callback);
        }
        [BlackList]
        public void OpenAndCloseAsync<TOpen, TClose>(UIContext context, bool destroy)
            where TOpen : UIView, new()
            where TClose : UIView
        {
            OpenAndCloseAsync<TOpen, TClose>(context, destroy, null);
        }
        [BlackList]
        public void OpenAndCloseAsync<TOpen, TClose>(UIContext context, bool destroy, Action<TOpen> callback)
            where TOpen : UIView, new()
            where TClose : UIView
        {
            CloseAsync<TClose>(destroy, true, () =>
            {
                OpenAsync<TOpen>(context, callback);
            });
        }
        [BlackList]
        public void OpenAndCloseAsync<TOpen, TClose>(UIStyle style)
            where TOpen : UIView, new()
            where TClose : UIView
        {
            OpenAndCloseAsync<TOpen, TClose>(style, null, true, null);
        }
        [BlackList]
        public void OpenAndCloseAsync<TOpen, TClose>(UIStyle style, Action<TOpen> callback)
            where TOpen : UIView, new()
            where TClose : UIView
        {
            OpenAndCloseAsync<TOpen, TClose>(style, null, true, callback);
        }
        [BlackList]
        public void OpenAndCloseAsync<TOpen, TClose>(UIStyle style, bool destroy)
            where TOpen : UIView, new()
            where TClose : UIView
        {
            OpenAndCloseAsync<TOpen, TClose>(style, null, destroy, null);
        }
        [BlackList]
        public void OpenAndCloseAsync<TOpen, TClose>(UIStyle style, bool destroy, Action<TOpen> callback)
            where TOpen : UIView, new()
            where TClose : UIView
        {
            OpenAndCloseAsync<TOpen, TClose>(style, null, destroy, callback);
        }
        [BlackList]
        public void OpenAndCloseAsync<TOpen, TClose>(UIStyle style, UIContext context)
           where TOpen : UIView, new()
           where TClose : UIView
        {
            OpenAndCloseAsync<TOpen, TClose>(style, context, true, null);
        }
        [BlackList]
        public void OpenAndCloseAsync<TOpen, TClose>(UIStyle style, UIContext context, Action<TOpen> callback)
            where TOpen : UIView, new()
            where TClose : UIView
        {
            OpenAndCloseAsync<TOpen, TClose>(style, context, true, callback);
        }
        [BlackList]
        public void OpenAndCloseAsync<TOpen, TClose>(UIStyle style, UIContext context, bool destroy)
            where TOpen : UIView, new()
            where TClose : UIView
        {
            OpenAndCloseAsync<TOpen, TClose>(style, context, destroy, null);
        }
        [BlackList]
        public void OpenAndCloseAsync<TOpen, TClose>(UIStyle style, UIContext context, bool destroy, Action<TOpen> callback)
            where TOpen : UIView, new()
            where TClose : UIView
        {
            CloseAsync<TClose>(destroy, true, () =>
            {
                OpenAsync<TOpen>(style, context, callback);
            });
        }
        #endregion

        #region OpenOrClose
        [BlackList]
        public void OpenOrClose<T>(bool destroy = false)
            where T : UIView, new()
        {
            OpenOrClose<T>(null, destroy);
        }
        [BlackList]
        public void OpenOrClose<T>(UIContext context, bool destroy = false)
            where T : UIView, new()
        {
            if (Exists<T>(false))
            {
                Close<T>(destroy, true);
            }
            else
            {
                Open<T>(context);
            }
        }
        [BlackList]
        public void OpenOrClose<T>(UIStyle style, bool destroy = false)
            where T : UIView, new()
        {
            OpenOrClose<T>(style, null, destroy);
        }
        [BlackList]
        public void OpenOrClose<T>(UIStyle style, UIContext context, bool destroy = false)
            where T : UIView, new()
        {
            if (Exists<T>(false))
            {
                Close<T>(destroy, true);
            }
            else
            {
                Open<T>(style, context);
            }
        }
        #endregion

        #region OpenOrCloseAsync
        //public IEnumerator OpenOrCloseAsync<T>(bool destroy = false)
        //	where T : UIView, new()
        //{
        //	return OpenOrCloseAsync<T>(null, destroy);
        //}
        //public IEnumerator OpenOrCloseAsync<T>(UIContext context, bool destroy = false)
        //	where T : UIView, new()
        //{
        //	if (Exists<T>(false)) {
        //		Close<T>(destroy, true);
        //		return null;
        //	} else {
        //		return OpenAsync<T>(context);
        //	}
        //}
        //public IEnumerator OpenOrCloseAsync<T>(UIStyle style, bool destroy = false)
        //	where T : UIView, new()
        //{
        //	return OpenOrCloseAsync<T>(style, null, destroy);
        //}
        //public IEnumerator OpenOrCloseAsync<T>(UIStyle style, UIContext context, bool destroy = false)
        //	where T : UIView, new()
        //{
        //	if (Exists<T>(false)) {
        //		Close<T>(destroy, true);
        //		return null;
        //	} else {
        //		return OpenAsync<T>(style, context);
        //	}
        //}

        [BlackList]
        public void OpenOrCloseAsync<T>()
            where T : UIView, new()
        {
            OpenOrCloseAsync<T>(null, true, null);
        }
        [BlackList]
        public void OpenOrCloseAsync<T>(Action callback)
            where T : UIView, new()
        {
            OpenOrCloseAsync<T>(null, true, callback);
        }
        [BlackList]
        public void OpenOrCloseAsync<T>(bool destroy)
            where T : UIView, new()
        {
            OpenOrCloseAsync<T>(null, destroy, null);
        }
        [BlackList]
        public void OpenOrCloseAsync<T>(bool destroy, Action callback)
            where T : UIView, new()
        {
            OpenOrCloseAsync<T>(null, destroy, callback);
        }
        [BlackList]
        public void OpenOrCloseAsync<T>(UIContext context)
            where T : UIView, new()
        {
            OpenOrCloseAsync<T>(context, true, null);
        }
        [BlackList]
        public void OpenOrCloseAsync<T>(UIContext context, Action callback)
            where T : UIView, new()
        {
            OpenOrCloseAsync<T>(context, true, callback);
        }
        [BlackList]
        public void OpenOrCloseAsync<T>(UIContext context, bool destroy, Action callback)
            where T : UIView, new()
        {
            if (Exists<T>(false))
            {
                CloseAsync<T>(destroy, true, callback);
            }
            else
            {
                OpenAsync<T>(context, (v) =>
                {
                    if (callback != null)
                    {
                        callback();
                    }
                });
            }
        }

        [BlackList]
        public void OpenOrCloseAsync<T>(UIStyle style)
            where T : UIView, new()
        {
            OpenOrCloseAsync<T>(style, null, true, null);
        }
        [BlackList]
        public void OpenOrCloseAsync<T>(UIStyle style, Action callback)
            where T : UIView, new()
        {
            OpenOrCloseAsync<T>(style, null, true, callback);
        }
        [BlackList]
        public void OpenOrCloseAsync<T>(UIStyle style, bool destroy)
            where T : UIView, new()
        {
            OpenOrCloseAsync<T>(style, null, destroy, null);
        }
        [BlackList]
        public void OpenOrCloseAsync<T>(UIStyle style, bool destroy, Action callback)
            where T : UIView, new()
        {
            OpenOrCloseAsync<T>(style, null, destroy, callback);
        }
        [BlackList]
        public void OpenOrCloseAsync<T>(UIStyle style, UIContext context)
            where T : UIView, new()
        {
            OpenOrCloseAsync<T>(style, context, true, null);
        }
        [BlackList]
        public void OpenOrCloseAsync<T>(UIStyle style, UIContext context, Action callback)
            where T : UIView, new()
        {
            OpenOrCloseAsync<T>(style, context, true, callback);
        }
        [BlackList]
        public void OpenOrCloseAsync<T>(UIStyle style, UIContext context, bool destroy)
           where T : UIView, new()
        {
            OpenOrCloseAsync<T>(style, context, destroy, null);
        }
        [BlackList]
        public void OpenOrCloseAsync<T>(UIStyle style, UIContext context, bool destroy, Action callback)
            where T : UIView, new()
        {
            if (Exists<T>(false))
            {
                CloseAsync<T>(destroy, true, callback);
            }
            else
            {
                OpenAsync<T>(style, context, (v) =>
                {
                    if (callback != null)
                    {
                        callback();
                    }
                });
            }
        }
        #endregion

        #region OpenAndPush
        [BlackList]
        public TOpen OpenAndPush<TOpen, TPush>(bool destroy = false)
            where TOpen : UIView, new()
            where TPush : UIView
        {
            return OpenAndPush<TOpen, TPush>(null, destroy);
        }
        [BlackList]
        public TOpen OpenAndPush<TOpen, TPush>(UIContext context, bool destroy = false)
            where TOpen : UIView, new()
            where TPush : UIView
        {
            Entity entity;
            Type type = typeof(TPush);
            if (TryGet(type, false, out entity))
            {
                m_PushStates.Add(new State() { type = type, context = entity.context, style = entity.style, next = typeof(TOpen) });
                Close(entity, destroy, false, true);
            }
            return Open<TOpen>(context);
        }
        [BlackList]
        public TOpen OpenAndPush<TOpen, TPush>(UIStyle style, bool destroy = false)
            where TOpen : UIView, new()
            where TPush : UIView
        {
            return OpenAndPush<TOpen, TPush>(style, null, destroy);
        }
        [BlackList]
        public TOpen OpenAndPush<TOpen, TPush>(UIStyle style, UIContext context, bool destroy = false)
            where TOpen : UIView, new()
            where TPush : UIView
        {
            Entity entity;
            Type type = typeof(TPush);
            if (TryGet(type, false, out entity))
            {
                m_PushStates.Add(new State() { type = type, context = entity.context, style = entity.style, next = typeof(TOpen) });
                Close(entity, destroy, false, true);
            }
            return Open<TOpen>(style, context);
        }
        #endregion

        #region OpenAndPushAsync
        //public IEnumerator OpenAndPushAsync<TOpen, TPush>(bool destroy = false)
        //	where TOpen : UIView, new()
        //	where TPush : UIView
        //{
        //	return OpenAndPushAsync<TOpen, TPush>(null, destroy);
        //}
        //public IEnumerator OpenAndPushAsync<TOpen, TPush>(UIContext context, bool destroy = false)
        //	where TOpen : UIView, new()
        //	where TPush : UIView
        //{
        //	Entity entity;
        //	Type type = typeof(TPush);
        //	if (TryGet(type, false, out entity)) {
        //		m_PushStates.Add(new State() { type = type, context = entity.context, style = entity.style, next = typeof(TOpen) });
        //		Close(entity, destroy, false, true);
        //	}
        //	return OpenAsync<TOpen>(context);
        //}
        //public IEnumerator OpenAndPushAsync<TOpen, TPush>(UIStyle style, bool destroy = false)
        //	where TOpen : UIView, new()
        //	where TPush : UIView
        //{
        //	return OpenAndPushAsync<TOpen, TPush>(style, null, destroy);
        //}
        //public IEnumerator OpenAndPushAsync<TOpen, TPush>(UIStyle style, UIContext context, bool destroy = false)
        //	where TOpen : UIView, new()
        //	where TPush : UIView
        //{
        //	Entity entity;
        //	Type type = typeof(TPush);
        //	if (TryGet(type, false, out entity)) {
        //		m_PushStates.Add(new State() { type = type, context = entity.context, style = entity.style, next = typeof(TOpen) });
        //		Close(entity, destroy, false, true);
        //	}
        //	return OpenAsync<TOpen>(style, context);
        //}

        [BlackList]
        public void OpenAndPushAsync<TOpen, TPush>()
            where TOpen : UIView, new()
            where TPush : UIView
        {
            OpenAndPushAsync<TOpen, TPush>(null, true, null);
        }
        [BlackList]
        public void OpenAndPushAsync<TOpen, TPush>(Action<TOpen> callback)
            where TOpen : UIView, new()
            where TPush : UIView
        {
            OpenAndPushAsync<TOpen, TPush>(null, true, callback);
        }
        [BlackList]
        public void OpenAndPushAsync<TOpen, TPush>(bool destroy)
            where TOpen : UIView, new()
            where TPush : UIView
        {
            OpenAndPushAsync<TOpen, TPush>(null, destroy, null);
        }
        [BlackList]
        public void OpenAndPushAsync<TOpen, TPush>(bool destroy, Action<TOpen> callback)
            where TOpen : UIView, new()
            where TPush : UIView
        {
            OpenAndPushAsync<TOpen, TPush>(null, destroy, callback);
        }
        [BlackList]
        public void OpenAndPushAsync<TOpen, TPush>(UIContext context)
            where TOpen : UIView, new()
            where TPush : UIView
        {
            OpenAndPushAsync<TOpen, TPush>(context, true, null);
        }
        [BlackList]
        public void OpenAndPushAsync<TOpen, TPush>(UIContext context, Action<TOpen> callback)
            where TOpen : UIView, new()
            where TPush : UIView
        {
            OpenAndPushAsync<TOpen, TPush>(context, true, callback);
        }
        [BlackList]
        public void OpenAndPushAsync<TOpen, TPush>(UIContext context, bool destroy)
           where TOpen : UIView, new()
           where TPush : UIView
        {
            OpenAndPushAsync<TOpen, TPush>(context, destroy, null);
        }
        [BlackList]
        public void OpenAndPushAsync<TOpen, TPush>(UIContext context, bool destroy, Action<TOpen> callback)
            where TOpen : UIView, new()
            where TPush : UIView
        {
            Entity entity;
            Type type = typeof(TPush);
            if (TryGet(type, false, out entity))
            {
                m_PushStates.Add(new State() { type = type, context = entity.context, style = entity.style, next = typeof(TOpen) });
                CloseAsync(entity, destroy, false, true, () =>
                {
                    OpenAsync<TOpen>(context, callback);
                });
            }
            else
            {
                OpenAsync<TOpen>(context, callback);
            }
        }

        [BlackList]
        public void OpenAndPushAsync<TOpen, TPush>(UIStyle style)
            where TOpen : UIView, new()
            where TPush : UIView
        {
            OpenAndPushAsync<TOpen, TPush>(style, null, true, null);
        }
        [BlackList]
        public void OpenAndPushAsync<TOpen, TPush>(UIStyle style, Action<TOpen> callback)
            where TOpen : UIView, new()
            where TPush : UIView
        {
            OpenAndPushAsync<TOpen, TPush>(style, null, true, callback);
        }
        [BlackList]
        public void OpenAndPushAsync<TOpen, TPush>(UIStyle style, bool destroy)
            where TOpen : UIView, new()
            where TPush : UIView
        {
            OpenAndPushAsync<TOpen, TPush>(style, null, destroy, null);
        }
        [BlackList]
        public void OpenAndPushAsync<TOpen, TPush>(UIStyle style, bool destroy, Action<TOpen> callback)
            where TOpen : UIView, new()
            where TPush : UIView
        {
            OpenAndPushAsync<TOpen, TPush>(style, null, destroy, callback);
        }
        [BlackList]
        public void OpenAndPushAsync<TOpen, TPush>(UIStyle style, UIContext context)
            where TOpen : UIView, new()
            where TPush : UIView
        {
            OpenAndPushAsync<TOpen, TPush>(style, context, true, null);
        }
        [BlackList]
        public void OpenAndPushAsync<TOpen, TPush>(UIStyle style, UIContext context, Action<TOpen> callback)
            where TOpen : UIView, new()
            where TPush : UIView
        {
            OpenAndPushAsync<TOpen, TPush>(style, context, true, callback);
        }
        [BlackList]
        public void OpenAndPushAsync<TOpen, TPush>(UIStyle style, UIContext context, bool destroy)
            where TOpen : UIView, new()
            where TPush : UIView
        {
            OpenAndPushAsync<TOpen, TPush>(style, context, destroy, null);
        }
        [BlackList]
        public void OpenAndPushAsync<TOpen, TPush>(UIStyle style, UIContext context, bool destroy, Action<TOpen> callback)
            where TOpen : UIView, new()
            where TPush : UIView
        {
            Entity entity;
            Type type = typeof(TPush);
            if (TryGet(type, false, out entity))
            {
                m_PushStates.Add(new State() { type = type, context = entity.context, style = entity.style, next = typeof(TOpen) });
                CloseAsync(entity, destroy, false, true, () =>
                {
                    OpenAsync<TOpen>(style, context, callback);
                });
            }
            else
            {
                OpenAsync<TOpen>(style, context, callback);
            }
        }
        #endregion

        #region CloseAndBack
        [BlackList]
        public UIView CloseAndBack<TClose>(bool destroy = false)
           where TClose : UIView
        {
            Close<TClose>(destroy, false);

            if (m_PushStates.Count > 0)
            {
                var data = m_PushStates[m_PushStates.Count - 1];
                if (data.next.GetType() == typeof(TClose))
                {
                    m_PushStates.RemoveAt(m_PushStates.Count - 1);
                    return Open((Type)data.type, null, data.style, data.context);
                }
            }

            return null;
        }
        [BlackList]
        public TBack CloseAndBack<TClose, TBack>(bool destroy = false)
            where TClose : UIView
            where TBack : UIView, new()
        {
            Close<TClose>(destroy, false);

            if (m_PushStates.Count > 0)
            {
                if (m_PushStates[m_PushStates.Count - 1].next.GetType() == typeof(TClose))
                {
                    var type = typeof(TBack);
                    int index = m_PushStates.FindLastIndex((data) => data.type.GetType() == type);
                    if (index >= 0)
                    {
                        var data = m_PushStates[index];
                        ClearPushStates(index);
                        return (TBack)Open(type, null, data.style, data.context);
                    }
                }
            }

            return null;
        }

        private void CloseAndBack(Entity entity)
        {
            Close(entity, false, false, false);

            if (m_PushStates.Count > 0)
            {
                var data = m_PushStates[m_PushStates.Count - 1];
                if (data.type is Type)
                {
                    if (data.next.GetType() == entity.view.GetType())
                    {
                        m_PushStates.RemoveAt(m_PushStates.Count - 1);
                        Open((Type)data.type, null, data.style, data.context);
                    }
                }
#if USE_LUA
                else
                {
                    if (object.Equals(data.next, ((UIScriptableView)entity.view).@class))
                    {
                        m_PushStates.RemoveAt(m_PushStates.Count - 1);
                        OpenView((XLua.LuaTable)data.type, data.style, data.context);
                    }
                }
#endif
            }
        }
        #endregion

        #region CloseAndBackAsync
        [BlackList]
        public void CloseAndBackAsync<TClose>(bool destroy = false)
           where TClose : UIView
        {
            CloseAndBackAsync<TClose>(destroy, null);
        }
        [BlackList]
        public void CloseAndBackAsync<TClose>(Action<UIView> callback)
           where TClose : UIView
        {
            CloseAndBackAsync<TClose>(false, callback);
        }
        [BlackList]
        public void CloseAndBackAsync<TClose>(bool destroy, Action<UIView> callback)
           where TClose : UIView
        {
            CloseAsync<TClose>(destroy, false, () =>
            {
                if (m_PushStates.Count > 0)
                {
                    var data = m_PushStates[m_PushStates.Count - 1];
                    if (data.next.GetType() == typeof(TClose))
                    {
                        m_PushStates.RemoveAt(m_PushStates.Count - 1);
                        OpenAsync((Type)data.type, null, data.style, data.context, callback);
                    }
                }
            });
        }
        [BlackList]
        public void CloseAndBackAsync<TClose, TBack>(bool destroy = false)
            where TClose : UIView
            where TBack : UIView, new()
        {
            CloseAndBackAsync<TClose, TBack>(destroy, null);
        }
        [BlackList]
        public void CloseAndBackAsync<TClose, TBack>(Action<TBack> callback)
            where TClose : UIView
            where TBack : UIView, new()
        {
            CloseAndBackAsync<TClose, TBack>(false, callback);
        }
        [BlackList]
        public void CloseAndBackAsync<TClose, TBack>(bool destroy, Action<TBack> callback)
           where TClose : UIView
           where TBack : UIView, new()
        {
            CloseAsync<TClose>(destroy, false, () =>
            {
                if (m_PushStates.Count > 0)
                {
                    if (m_PushStates[m_PushStates.Count - 1].next.GetType() == typeof(TClose))
                    {
                        var type = typeof(TBack);
                        int index = m_PushStates.FindLastIndex((data) => data.type.GetType() == type);
                        if (index >= 0)
                        {
                            var data = m_PushStates[index];
                            ClearPushStates(index);
                            OpenAsync<TBack>(null, data.style.Value, data.context, callback);
                        }
                    }
                }
            });
        }

        private void CloseAndBackAsync(Entity entity, Action callback = null)
        {
            CloseAsync(entity, false, false, false, () =>
            {
                callback?.Invoke();
                if (m_PushStates.Count > 0)
                {
                    var data = m_PushStates[m_PushStates.Count - 1];
                    if (data.type is Type)
                    {
                        if (data.next.GetType() == entity.view.GetType())
                        {
                            m_PushStates.RemoveAt(m_PushStates.Count - 1);
                            OpenAsync<UIView>((Type)data.type, null, data.style, data.context, null);
                        }
                    }
#if USE_LUA
                    else
                    {
                        if (object.Equals(data.next, ((UIScriptableView)entity.view).@class))
                        {
                            m_PushStates.RemoveAt(m_PushStates.Count - 1);
                            OpenViewAsync((XLua.LuaTable)data.type, data.style, data.context, null);
                        }
                    }
#endif
                }
            });
        }
        #endregion

        [BlackList]
        public bool Exists<T>(bool checkInvisible = false)
            where T : UIView
        {
            return Get<T>(checkInvisible) != null;
        }

        [BlackList]
        public T Get<T>(bool checkInvisible = false)
            where T : UIView
        {
            Entity entity;
            if (TryGet(typeof(T), checkInvisible, out entity))
            {
                return (T)entity.view;
            }
            return null;
        }

        [BlackList]
        public bool IsFocused<T>()
            where T : UIView
        {
            Entity entity;
            if (TryGet(typeof(T), true, out entity))
            {
                return entity.focused;
            }
            return false;
        }

        [BlackList]
        public UIView GetTopView<T>()
        {
            Entity found = null;
            for (int i = m_Entities.Count - 1; i >= 0; i--)
            {
                Entity entity = m_Entities[i];
                if (entity.phase == Entity.Phase.Running)
                {
                    if (entity.style.Value.topmost)
                    {
                        found = entity;
                        break;
                    }
                    else if (found == null)
                    {
                        found = entity;
                    }
                }
            }
            return found != null ? found.view : null;
        }

        [BlackList]
        public bool IsOnTop<T>()
            where T : UIView
        {
            bool ret = false;
            for (int i = m_Entities.Count - 1; i >= 0; i--)
            {
                Entity entity = m_Entities[i];
                if (entity.phase == Entity.Phase.Running)
                {
                    if (ret)
                    {
                        if (entity.style.Value.topmost)
                            return false;
                    }
                    else if (entity.view.GetType() == typeof(T))
                    {
                        if (entity.style.Value.topmost)
                            return true;
                        ret = true;
                    }
                }
            }
            return ret;
        }

        private bool TryGet(Type type, bool checkInvisible, out Entity entity)
        {
            for (int i = m_Entities.Count - 1; i >= 0; i--)
            {
                entity = m_Entities[i];
                if (entity.view.GetType() == type)
                {
                    if (checkInvisible || entity.phase != Entity.Phase.Closing)
                    {
                        return true;
                    }
                    break;
                }
            }
            entity = null;
            return false;
        }

        private void ClearPushStates()
        {
            if (m_PushStates.Count > 0)
            {
                using (var it = m_PushStates.GetEnumerator())
                {
                    UIContext.Release(it.Current.context);
                }
                m_PushStates.Clear();
            }
        }
        private void ClearPushStates(int index)
        {
            int count = m_PushStates.Count;
            for (int i = index + 1; i < count; i++)
            {
                UIContext.Release(m_PushStates[i].context);
            }
            m_PushStates.RemoveRange(index, count - index);
        }

        private Layer m_MaskLayer, m_EventLayer;

        private void SetEntityVisible(Entity entity, bool visible)
        {
            UIView view = entity.view;
            var gameObject = entity.container.gameObject;
            if ((view.setting.overrideMode ? view.setting.invisibleMode : m_InvisibleMode) == InvisibleMode.SetActive)
            {
                gameObject.SetActive(visible);
            }
            else
            {
                SparkHelper.SetLayer(gameObject, visible ? gameObject.layer : (view.setting.overrideMode ? view.setting.invisibleLayer : m_InvisibleLayer));
                var raycaster = gameObject.GetComponent<GraphicRaycaster>();
                if (raycaster != null)
                    raycaster.enabled = visible;
            }
        }

        private void SetDepth(Component component, bool masked, int depth)
        {
            if (component != null)
            {
                var transform = component.transform;
                if (transform != null)
                {
                    if (isRootView)
                    {
                        var camera = (m_MaskMode == MaskMode.ColorLayer || !masked) ? m_Camera : m_BlurCamera;
                        var canvas = transform.GetComponent<Canvas>();
                        canvas.sortingOrder = depth;
                        if (canvas.worldCamera != camera)
                        {
                            canvas.worldCamera = camera;
                        }
#if UNITY_EDITOR
                        transform.SetAsFirstSibling();
#endif
                    }
                    else
                    {
                        transform.SetAsFirstSibling();
                    }
                }
            }
        }

        /*static private void SetSiblingIndex(Transform transform, int siblingIndex)
		{
			var oldIndex = transform.GetSiblingIndex();

			if (oldIndex < siblingIndex) {
				transform.SetSiblingIndex(Math.Max(0, siblingIndex - 1));
			} else if (oldIndex > siblingIndex) {
				transform.SetSiblingIndex(Math.Max(siblingIndex, 0));
			}
		}*/

        private void SetViewFocused(Entity entity, ref bool focused, ref bool maskLayerUsed, ref bool eventLayerUsed, ref int depth)
        {
            if (entity.phase == Entity.Phase.Closing) return;

            if (entity.phase == Entity.Phase.Running)
            {
                if (entity.focused != focused)
                {
                    entity.focused = focused;
                    UIView.InternalFocusChanged(entity.view, focused);
                }
            }

            if (entity.placeholder != null)
            {
                SetDepth(entity.placeholder, maskLayerUsed, depth--);
            }
            if (entity.container != null)
            {
                SetDepth(entity.container, maskLayerUsed, depth--);
            }

            if (!entity.style.HasValue) return;

            var style = entity.style.Value;
            if (focused && style.focusable)
            {
                focused = false;
            }
            if (!eventLayerUsed && style.blockable)
            {
                eventLayerUsed = true;
                if (m_EventLayer == null)
                {
                    m_EventLayer = Layer.Create<EventLayer>(this);
                }
                ((EventLayer)m_EventLayer).Show(entity);
                SetDepth(m_EventLayer, maskLayerUsed, depth--);
            }
            if (!maskLayerUsed && style.maskable)
            {
                if (style.maskColor.a != 0f)
                {
                    maskLayerUsed = true;
                    if (!isRootView || m_MaskMode == MaskMode.ColorLayer)
                    {
                        if (m_MaskLayer == null)
                        {
                            m_MaskLayer = Layer.Create<MaskLayer>(this);
                        }
                        m_MaskLayer.Show();
                        SetDepth(m_MaskLayer, maskLayerUsed, depth--);
                    }
                }
            }
        }

        Layer m_SafeAreaLayer;
        // static Rect s_lastSafeArea = Rect.zero;

        void Update()
        {
            // if (isRootView)
            // {
            //     if (Screen.safeArea != s_lastSafeArea && m_SafeAreaLayer != null)
            //     {
            //         s_lastSafeArea = Screen.safeArea;
            //         if (s_lastSafeArea.position == Vector2.zero && s_lastSafeArea.size == new Vector2(Screen.width, Screen.height))
            //         {
            //             UISafeArea.padding = Vector4.zero;
            //         }
            //         else
            //         {
            //             var canvas = m_SafeAreaLayer.GetComponent<Canvas>();
            //             var factor = canvas.scaleFactor;
            //             var leftBottom = s_lastSafeArea.position / factor;
            //             var rightTop = (new Vector2(Screen.width, Screen.height) - (s_lastSafeArea.position + s_lastSafeArea.size)) / factor;
            //             UISafeArea.padding = new Vector4(leftBottom.x, leftBottom.y, rightTop.x, rightTop.y);
            //         }
            //     }
            // }

            if (!m_EntityViewChanged)
                return;
            m_EntityViewChanged = false;

            int depth = m_MaxDepth;
            bool focused = true, maskLayerUsed = false, eventLayerUsed = false;

            for (int i = m_Entities.Count - 1; !m_EntityViewChanged && i >= 0; i--)
            {
                Entity entity = m_Entities[i];
                if (entity.style.HasValue && entity.style.Value.topmost)
                {
                    SetViewFocused(entity, ref focused, ref maskLayerUsed, ref eventLayerUsed, ref depth);
                }
            }
            for (int i = m_Entities.Count - 1; !m_EntityViewChanged && i >= 0; i--)
            {
                Entity entity = m_Entities[i];
                if (!entity.style.HasValue || !entity.style.Value.topmost)
                {
                    SetViewFocused(entity, ref focused, ref maskLayerUsed, ref eventLayerUsed, ref depth);
                }
            }

            // 隐藏遮罩
            if (isRootView && m_MaskMode == MaskMode.CameraBlur)
            {
                if (m_BlurCamera != null)
                {
                    var blur = m_BlurCamera.GetComponent<CameraBlur>();
                    if (blur != null)
                    {
                        blur.enabled = maskLayerUsed;
                    }
                }
            }
            else if (!maskLayerUsed)
            {
                if (m_MaskLayer != null)
                {
                    m_MaskLayer.Hide();
                }
            }

            if (!eventLayerUsed && m_EventLayer != null)
                m_EventLayer.Hide();

            SetDepth(m_SafeAreaLayer, false, depth);
        }

        #region Adaptive Layout

        /// <summary>
        /// 设置适配屏幕间距，以Portrait方向设置参数
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        static public void SetAdaptivePadding(float left, float right, float top, float bottom)
        {
        }
        #endregion

        static private List<UIViewRoot> m_ViewRootList = new List<UIViewRoot>();
        protected override void OnEnable()
        {
            m_ViewRootList.Add(this);
        }

        protected override void OnDisable()
        {
            m_ViewRootList.Remove(this);
        }

#if UNITY_EDITOR || UNITY_ANDROID
        private bool OnBackButtonPressed(Entity entity)
        {
            if (entity.phase == Entity.Phase.Opening)
            {
                return true;
            }
            else if (entity.phase == Entity.Phase.Running)
            {
                if (entity.view.blockBackButton)
                {
                    if (!entity.blocking)
                    {
                        UIView.InternalBackButtonPressed(entity.view);
                    }
                    return true;
                }
                else
                {
                    var style = entity.style.Value;
                    if (!style.IsOverlap)
                    {
                        if (!entity.blocking)
                        {
                            //root.CloseAndBack(entity);
                            CloseAndBackAsync(entity);
                        }
                        if (!style.IsPopup)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        static internal bool OnBackButtonPressed()
        {
            foreach (var root in m_ViewRootList)
            {
                for (int i = root.m_Entities.Count - 1; i >= 0; i--)
                {
                    var entity = root.m_Entities[i];
                    if (entity.style.HasValue && entity.style.Value.topmost)
                    {
                        if (root.OnBackButtonPressed(entity))
                        {
                            return true;
                        }
                    }
                }

                for (int i = root.m_Entities.Count - 1; i >= 0; i--)
                {
                    var entity = root.m_Entities[i];
                    if (entity.style.HasValue && !entity.style.Value.topmost)
                    {
                        if (root.OnBackButtonPressed(entity))
                        {
                            return true;
                        }
                    }
                }
                /*
                while (--i >= 0)
                {
                    var entity = root.m_Entities[i];
                    if (entity.phase == Entity.Phase.Opening)
                    {
                        return true;
                    }
                    else if (entity.phase == Entity.Phase.Running)
                    {
                        if (entity.view.blockBackButton)
                        {
                            if (!entity.blocking)
                            {
                                UIView.InternalBackButtonPressed(entity.view);
                            }
                            return true;
                        }
                        else
                        {
                            var style = entity.style.Value;
                            if (style.focusable)
                            {
                                if (!entity.blocking)
                                {
                                    //root.CloseAndBack(entity);
                                    root.CloseAndBackAsync(entity);
                                }
                                return true;
                            }
                        }
                    }
                }*/
            }
            return false;
        }
#endif
        #region Layer
        private class ViewLayer : Layer
        {
            protected override void Awake()
            {
                gameObject.name = "View Opening";
                gameObject.AddComponent<UIRect>();
                gameObject.AddComponent<GraphicRaycaster>();
                var button = gameObject.AddComponent<Button>();
                button.transition = Selectable.Transition.None;
            }
        }
        private class EventLayer : Layer, IPointerClickHandler
        {
            private Entity m_Entity;

            protected override void Awake()
            {
                base.Awake();
                gameObject.name = "__UI_EVENT_LAYER__";
                gameObject.AddComponent<UIRect>();
                gameObject.AddComponent<GraphicRaycaster>();
            }
            public void Show(Entity entity)
            {
                m_Entity = entity;
                base.Show();
            }
            public override void Hide()
            {
                m_Entity = null;
                base.Hide();
            }
            void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
            {
                if (m_Entity != null && !m_Entity.blocking)
                {
                    if (m_Entity.style.Value.closeable)
                    {
                        var popup = m_Entity.style.Value.IsPopup;
                        if (popup)
                        {
                            m_Entity.view.root.CloseAndBackAsync(m_Entity, () => {
                                // POPUP模式在UI关闭后，立即调整UI层次，以便于能够定位并检测是否关闭其他的POPUP窗口
                                var root = gameObject.GetComponentInParent<UIViewRoot>();
                                root.m_EntityViewChanged = true;
                                root.Update();

                                // 检测其他对象的触摸事件
                                List<RaycastResult> results = new List<RaycastResult>();
                                EventSystem.current.RaycastAll(eventData, results);
                                foreach (RaycastResult result in results)
                                {
                                    /*if (result.gameObject == this.gameObject)
                                    {
                                        //print("current gameobject");
                                        continue;
                                    }*/
                                    var gameObject = ExecuteEvents.ExecuteHierarchy(result.gameObject, eventData, ExecuteEvents.pointerClickHandler);
                                    //print("Hit " + result.gameObject.name +", Exec " + gameObject);
                                    if (gameObject != null)
                                    {
                                        //ExecuteEvents.Execute(gameObject, eventData, ExecuteEvents.pointerClickHandler);
                                        break;
                                    }
                                }
                            });
                        }
                        else
                        {
                            m_Entity.view.root.CloseAndBackAsync(m_Entity);
                        }
                    }
                }
            }
        }
        private class MaskLayer : Layer
        {
            private RawImage m_Image;
            protected override void Awake()
            {
                base.Awake();
                gameObject.name = "__UI_MASK_LAYER__";
                m_Image = gameObject.AddComponent<RawImage>();
                m_Image.raycastTarget = false;
            }
        }

        private class Layer : UIBehaviour
        {
            public virtual void Show()
            {
                gameObject.SetActive(true);
            }

            public virtual void Hide()
            {
                gameObject.SetActive(false);
            }

            static public T Create<T>(UIViewRoot root)
                where T : Layer
            {
                GameObject go = Instantiate(root.m_RootCanvasGameObject, root.transform, false);
                return go.AddComponent<T>();
            }
        }
        #endregion
    }
}

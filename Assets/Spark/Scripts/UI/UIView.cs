using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Spark
{
	public abstract class UIView
	{
		private Scheduler.Proxy m_Scheduler;

		public UIView()
		{
		}

		internal protected abstract string prefabPath
		{
			get;
		}

		public Scheduler.Proxy scheduler
		{
			get
			{
				if (m_Scheduler == null) {
					m_Scheduler = Scheduler.Proxy.Get();
				}
				return m_Scheduler;
			}
		}

		public UIViewRoot root
		{
			get; internal set;
		}

		public UIContext context
		{
			get; private set;
		}

		public GameObject gameObject
		{
			get; private set;
		}

		public Transform transform
		{
			get; private set;
		}

		internal UIViewSettings setting
		{
			get; private set;
		}

		public virtual bool blockBackButton
		{
			get; protected set;
		}

		virtual protected void OnBackButtonPressed()
		{
		}

		virtual protected void OnLocalized()
		{
		}

		/******************************************************\
		| View Life Circle Methods                             |
		\******************************************************/
		virtual protected void OnCreated()
		{
		}
		virtual protected void OnOpened()
		{
		}
		virtual protected void OnFocusChanged(bool focus)
		{
		}
		virtual protected void OnClosed()
		{
		}
		virtual protected void OnDestroyed()
		{
		}

		/*********************************************************\
		| UI Event Handlers                                       |
		\*********************************************************/
		virtual protected void OnButtonClick(Component com)
		{
		}
		// UIText
		virtual protected void OnTextLink(Component com, string value)
		{
		}
		// Dropdown
		virtual protected void OnValueChanged(Component com, int value)
		{
		}
		// Slider, ScrollBar
		virtual protected void OnValueChanged(Component com, float value)
		{
		}
		// Toggle
		virtual protected void OnValueChanged(Component com, bool value)
		{
		}
		// InputField
		virtual protected void OnValueChanged(Component com, string value)
		{
		}

		// UITableView
		virtual protected void OnTableViewCellInit(UITableView tableView, UITableViewCell cell, object data)
		{
		}
		virtual protected void OnTableViewSelected(UITableView tableView, object data)
		{
		}
		virtual protected void OnTableViewCellClick(UITableView tableView, UITableViewCell cell, GameObject target, object data)
		{
		}

		virtual protected void OnBeforeViewStackValueChanged(UIViewStack viewStack)
		{
		}

		/*******************************************************************\
		| Internal Methods (Don't call these methods)                       |
		\*******************************************************************/
		static internal void InternalCreated(UIView view, GameObject prefab, Transform parent, bool world)
		{
#if SPARK_DEBUG
			Debug.Log("OnCreated: " + view);
#endif
			var gameObject = GameObject.Instantiate(prefab,parent,world );
			view.gameObject = gameObject;
			view.transform = gameObject.transform;
			view.setting = gameObject.GetComponent<UIViewSettings>();
			if (view.setting == null) {
				view.setting = gameObject.AddComponent<UIViewSettings>();
			}
			view.OnCreated();
		}
		static internal void InternalOpened(UIView view, UIContext context)
		{
#if SPARK_DEBUG
			Debug.Log("OnOpened: " + view);
#endif
			// Locale.onInternalLocalized += view.OnLocalized;
			view.context = context;
			view.OnOpened();
		}
		static internal void InternalFocusChanged(UIView view, bool active)
		{
#if SPARK_DEBUG
			Debug.Log("OnFocusChanged: " + view + "," + active);
#endif
			var raycaster = view.gameObject.GetComponent<GraphicRaycaster>();
			if (raycaster != null) {
				raycaster.enabled = active;
			}
			view.OnFocusChanged(active);
		}
		static internal void InternalClosed(UIView view)
		{
#if SPARK_DEBUG
			Debug.Log("OnClosed: " + view);
#endif
			view.OnClosed();
			// Locale.onInternalLocalized -= view.OnLocalized;
			foreach (var root in view.gameObject.GetComponentsInChildren<UIViewRoot>(true)) {
				root.CloseAll(false);
			}
			view.context = null;
			if (view.m_Scheduler != null) {
				Scheduler.Proxy.Release(view.m_Scheduler);
				view.m_Scheduler = null;
			}
		}
		static internal void InternalDestroyed(UIView view)
		{
#if SPARK_DEBUG
			Debug.Log("OnDestroyed: " + view);
#endif
			view.OnDestroyed();
		}

		static internal void InternalBackButtonPressed(UIView view)
		{
			view.OnBackButtonPressed();
		}
		
		internal protected UIComponentCollection GetComponents(Component component, bool bindEvents)
		{
			var componentCollection = component.GetComponent<UIComponentCollection>();
			if (bindEvents) {
				BindEvents(componentCollection.components);
			}
			return componentCollection;
		}

		void BindEvents(List<Component> coms)
		{
			for (int i = 0, count = coms.Count; i < count; i++) {
				Component comp = coms[i];
				if (comp is Button) {
					((Button)comp).onClick.AddListener(delegate {
						OnButtonClick(comp);
					});
				} else if (comp is Slider) {
					((Slider)comp).onValueChanged.AddListener(delegate (float value) {
						OnValueChanged(comp, value);
					});
				} else if (comp is Scrollbar) {
					((Scrollbar)comp).onValueChanged.AddListener(delegate (float value) {
						OnValueChanged(comp, value);
					});
				} else if (comp is Toggle) {
					((Toggle)comp).onValueChanged.AddListener(delegate (bool value) {
						OnValueChanged(comp, value);
					});
				} else if (comp is Dropdown) {
					((Dropdown)comp).onValueChanged.AddListener(delegate (int value) {
						OnValueChanged(comp, value);
					});
				} else if (comp is InputField) {
					((InputField)comp).onValueChanged.AddListener(delegate (string value) {
						OnValueChanged(comp, value);
					});
				} else if (comp is UIViewStack) {
					((UIViewStack)comp).onValueChanged += delegate (int value) {
						OnBeforeViewStackValueChanged((UIViewStack)comp);
						OnValueChanged(comp, value);
					};
				} else if (comp is UITableView) {
					UITableView table = (UITableView)comp;
					table.onCellInit += OnTableViewCellInit;
					table.onSelected += OnTableViewSelected;
					table.onCellClick += OnTableViewCellClick;
                }
			}
		}
	}
}

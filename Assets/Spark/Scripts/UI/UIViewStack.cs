using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Spark
{
	[XLua.LuaCallCSharp]
	public class UIViewStack : MonoBehaviour
	{
		[Serializable]
		public struct View
		{
			public string name;
			public string path;
			public Transform value;
		}

		public Action<int> onValueChanged;

		[HideInInspector, SerializeField]
		private List<View> m_ViewList = new List<View>();

		private int m_SelectedIndex = -1;

		public int selectedIndex
		{
			get
			{
				return m_SelectedIndex;
			}
			set
			{
				value = Mathf.Clamp(value, -1, m_ViewList.Count - 1);
				if (m_SelectedIndex == value)
					return;
				var transform = selectedValue;
				if (transform != null) {
					transform.gameObject.SetActive(false);
				}
				m_SelectedIndex = value;
				if (m_SelectedIndex >= 0 && m_SelectedIndex < m_ViewList.Count) {
					transform = selectedValue;
					if (transform != null) {
						transform.gameObject.SetActive(true);
					} else {
						m_ViewList[m_SelectedIndex] = LoadView(m_ViewList[m_SelectedIndex], false);
					}
				}
				if (onValueChanged != null)
					onValueChanged(m_SelectedIndex);
			}
		}

		public string selectedName
		{
			get
			{
				if (m_SelectedIndex >= 0 && m_SelectedIndex < m_ViewList.Count) {
					return m_ViewList[m_SelectedIndex].name;
				}
				return null;
			}
			set
			{
				if (selectedName != value) {
					if (string.IsNullOrEmpty(value)) {
						selectedIndex = -1;
					} else {
						for (int i = m_ViewList.Count - 1; i >= 0; i--) {
							if (m_ViewList[i].name == value) {
								selectedIndex = i;
								break;
							}
						}
					}
				}
			}
		}

		public Transform selectedValue
		{
			get
			{
				if (m_SelectedIndex >= 0 && m_SelectedIndex < m_ViewList.Count) {
					return m_ViewList[m_SelectedIndex].value;
				}
				return null;
			}
		}

		public void Push(View view)
		{
			m_ViewList.Add(view);
		}
		public void Push(string path)
		{
			Push(new View() { path = path });
		}

		public void Push(string name, string path)
		{
			Push(new View() { name = name, path = path });
		}

		public void Push(string name, string path, Transform transform)
		{
			Push(new View() { name = name, path = path, value = transform });
		}

		public void Clear()
		{
			m_ViewList.Clear();
			m_SelectedIndex = -1;
		}

		private View LoadView(View view, bool preload)
		{
			if (!string.IsNullOrEmpty(view.path)) {
				var go = GameObject.Instantiate<GameObject>(Assets.LoadAsset<GameObject>(view.path));
				go.transform.SetParent(transform, false);
				go.name = view.name;
				if (preload) {
					SparkHelper.SetLayer(go, gameObject.layer);
				} else {
					go.SetActive(true);
				}
				view.value = go.transform;
			}
			return view;
		}

		internal void Preload()
		{
			for (int i = m_ViewList.Count - 1; i >= 0; i--) {
				m_ViewList[i] = LoadView(m_ViewList[i], true);
			}
		}
	}
}

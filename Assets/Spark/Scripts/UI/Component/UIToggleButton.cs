using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Spark
{
	[XLua.LuaCallCSharp]
	[AddComponentMenu("Spark/ToggleButton", 35)]
	public class UIToggleButton : Toggle
	{
		[SerializeField]
		private GameObject m_OnState = null;

		[SerializeField]
		private GameObject m_OffState = null;

		protected UIToggleButton()
		{
		}

		protected override void Awake()
		{
			base.Awake();

#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			onValueChanged.AddListener(ChangeState);
		}

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();
			ChangeState(isOn);
		}
#endif

		protected override void Start()
		{
			base.Start();
			ChangeState(isOn);
		}

		private void ChangeState(bool isOn)
		{
			if (m_OnState != null) {
				m_OnState.SetActive(isOn);
			}
			if (m_OffState != null) {
				m_OffState.SetActive(!isOn);
			}
		}
	}
}

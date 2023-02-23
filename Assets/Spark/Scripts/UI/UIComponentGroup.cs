using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Spark
{
    [XLua.LuaCallCSharp]
    public class UIComponentGroup : MonoBehaviour, ISerializationCallbackReceiver, IControl
    {
        [System.Serializable]
        public class Item
        {
            public string name;
            public GameObject gameObject;
            public Component component;
        }

        [SerializeField]
        private Item[] m_Components;

        private Dictionary<string, UnityEngine.Component> m_ComponentDict;

        public UnityEngine.Component Get(string name)
        {
            UnityEngine.Component obj;
            if (!m_ComponentDict.TryGetValue(name, out obj)) {
                Debug.LogError("UIComponentGroup not contain component:" + name);
                return null;
            } 
            
            return obj;
        }

        [XLua.BlackList]
        public void OnBeforeSerialize() { }

        [XLua.BlackList]
        public void OnAfterDeserialize()
        {
            m_ComponentDict = new Dictionary<string, Component>();
            for (int i = 0; i < m_Components.Length; i++)
            {
                m_ComponentDict[m_Components[i].name] = m_Components[i].component;
            }
        }
    }
}

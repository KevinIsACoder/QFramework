using UnityEngine;
using System.Collections;
using XLua;

namespace Spark
{
    public static partial class ExtensionMethod
    {
        public static LuaTable GetLuaBehaviour(this UnityEngine.GameObject go, string scriptName)
        {
            var behaviours = go.GetComponents<LuaBehaviour>();
            foreach(var bh in behaviours)
            {
                if (bh.luaObject!=null && bh.luaScriptName == scriptName)
                {
                    return bh.luaObject;
                }
            }
            return null;
        }

        public static LuaTable AddLuaBehaviour(this UnityEngine.GameObject go, string scriptName)
        {
            LuaBehaviour.s_LuaClassName = scriptName;
            var bh = go.AddComponent<LuaBehaviour>();
            LuaBehaviour.s_LuaClassName = string.Empty;
            return bh.luaObject;
        }
    }

    [LuaCallCSharp]
    public class LuaBehaviour : MonoBehaviour
    {
        // 临时变量，用于传递参数
        static internal string s_LuaClassName;

        [CSharpCallLua]
        public delegate void LuaCallbackDelegate(LuaTable self);

        [CSharpCallLua]
        public delegate void LuaCallbackBoolDelegate(LuaTable self, bool pause);
        [CSharpCallLua]
        public delegate void LuaCallbackCollisionDelegate(LuaTable self, Collision collision);
        [CSharpCallLua]
        public delegate void LuaCallbackCollision2DDelegate(LuaTable self, Collision2D collision);
        [CSharpCallLua]
        public delegate void LuaCallbackColliderDelegate(LuaTable self, Collider collider);
        [CSharpCallLua]
        public delegate void LuaCallbackCollider2DDelegate(LuaTable self, Collider2D collider);

        private LuaCallbackDelegate m_Awake;
        private LuaCallbackDelegate m_Start;
        private LuaCallbackDelegate m_Update;
        private LuaCallbackDelegate m_LateUpdate;
        private LuaCallbackDelegate m_FixedUpdate;
        private LuaCallbackDelegate m_OnDestroy;
        private LuaCallbackDelegate m_OnEnable;
        private LuaCallbackDelegate m_OnDisable;

        private LuaCallbackDelegate m_OnTransformParentChanged;
        private LuaCallbackDelegate m_OnTransformChildrenChanged;
        private LuaCallbackDelegate m_OnBeforeTransformParentChanged;

        private LuaCallbackDelegate m_OnApplicationQuit;
        private LuaCallbackBoolDelegate m_OnApplicationPause;
        private LuaCallbackBoolDelegate m_OnApplicationFocus;

        private LuaCallbackCollisionDelegate m_OnCollisionEnter;
        private LuaCallbackCollisionDelegate m_OnCollisionExit;
        private LuaCallbackCollisionDelegate m_OnCollisionStay;
        private LuaCallbackCollision2DDelegate m_OnCollisionEnter2D;
        private LuaCallbackCollision2DDelegate m_OnCollisionExit2D;
        private LuaCallbackCollision2DDelegate m_OnCollisionStay2D;

        private LuaCallbackColliderDelegate m_OnTriggerEnter;
        private LuaCallbackColliderDelegate m_OnTriggerExit;
        private LuaCallbackColliderDelegate m_OnTriggerStay;
        private LuaCallbackCollider2DDelegate m_OnTriggerEnter2D;
        private LuaCallbackCollider2DDelegate m_OnTriggerExit2D;
        private LuaCallbackCollider2DDelegate m_OnTriggerStay2D;

        [SerializeField]
        private string m_LuaScriptName = "";

        [SerializeField]
        private bool m_UpdateEnabled = false;

        [SerializeField]
        private bool m_CollisionEnabled = false;

        [SerializeField]
        private bool m_TriggerEnabled = false;

        [SerializeField]
        private bool m_TransformEnabled = false;

        [SerializeField]
        private bool m_ApplicationEnabled = false;

        public LuaTable luaObject { get; private set; }
        public string luaScriptName { get { return m_LuaScriptName; } }

        private void Awake()
        {
            //读取并创建脚本
            if (string.IsNullOrEmpty(m_LuaScriptName))
            {
                if (string.IsNullOrEmpty(s_LuaClassName))
                {
                    Debug.LogWarning("Lua script name is empty. gameobject name:" + gameObject.name);
                    return;
                }
                m_LuaScriptName = s_LuaClassName;
            }

            luaObject = SparkLua.CreateLuaBehaviour(m_LuaScriptName);
            luaObject.Set("component", this);

            luaObject.Get("Awake", out m_Awake);
            luaObject.Get("Start", out m_Start);
            luaObject.Get("OnEnable", out m_OnEnable);
            luaObject.Get("OnDisable", out m_OnDisable);
            luaObject.Get("OnDestroy", out m_OnDestroy);
            if (m_ApplicationEnabled)
            {
                luaObject.Get("OnApplicationPause", out m_OnApplicationQuit);
                luaObject.Get("OnApplicationPause", out m_OnApplicationPause);
                luaObject.Get("OnApplicationFocus", out m_OnApplicationFocus);
            }

            OnAwake();
            if (m_Awake != null) m_Awake(luaObject);
        }

        private void Start()
        {
            if (m_UpdateEnabled)
            {
                luaObject.Get("Update", out m_Update);
                luaObject.Get("LateUpdate", out m_LateUpdate);
                luaObject.Get("FixedUpdate", out m_FixedUpdate);
            }
            if (m_TransformEnabled)
            {
                luaObject.Get("OnTransformParentChanged", out m_OnTransformParentChanged);
                luaObject.Get("OnTransformChildrenChanged", out m_OnTransformChildrenChanged);
                luaObject.Get("OnBeforeTransformParentChanged", out m_OnBeforeTransformParentChanged);
            }
            if (m_CollisionEnabled)
            {
                luaObject.Get("OnCollisionEnter", out m_OnCollisionEnter);
                luaObject.Get("OnCollisionExit", out m_OnCollisionExit);
                luaObject.Get("OnCollisionStay", out m_OnCollisionStay);
                luaObject.Get("OnCollisionEnter2D", out m_OnCollisionEnter2D);
                luaObject.Get("OnCollisionExit2D", out m_OnCollisionExit2D);
                luaObject.Get("OnCollisionStay2D", out m_OnCollisionStay2D);
            }
            if (m_TriggerEnabled)
            {
                luaObject.Get("OnTriggerEnter", out m_OnTriggerEnter);
                luaObject.Get("OnTriggerExit", out m_OnTriggerExit);
                luaObject.Get("OnTriggerStay", out m_OnTriggerStay);
                luaObject.Get("OnTriggerEnter2D", out m_OnTriggerEnter2D);
                luaObject.Get("OnTriggerExit2D", out m_OnTriggerExit2D);
                luaObject.Get("OnTriggerStay2D", out m_OnTriggerStay2D);
            }

            OnStart();
            if (m_Start != null) m_Start(luaObject);
        }

        public virtual void OnAwake()
        {

        }

        public virtual void OnStart()
        {

        }

        private void Update()
        {
            if (m_UpdateEnabled && m_Update != null) m_Update(luaObject);
        }
        private void LateUpdate()
        {
            if (m_UpdateEnabled && m_LateUpdate != null) m_LateUpdate(luaObject);
        }
        private void FixedUpdate()
        {
            if (m_UpdateEnabled && m_FixedUpdate != null) m_FixedUpdate(luaObject);
        }

        private void OnEnable()
        {
            if (m_OnEnable != null) m_OnEnable(luaObject);
        }
        private void OnDisable()
        {
            if (m_OnDisable != null) m_OnDisable(luaObject);
        }

        private void OnApplicationPause(bool pause)
        {
            if (m_ApplicationEnabled && m_OnApplicationPause != null) m_OnApplicationPause(luaObject, pause);
        }

        private void OnApplicationFocus(bool focus)
        {
            if (m_ApplicationEnabled && m_OnApplicationFocus != null) m_OnApplicationFocus(luaObject, focus);
        }

        private void OnApplicationQuit()
        {
            if (m_ApplicationEnabled && m_OnApplicationQuit != null) m_OnApplicationQuit(luaObject);
        }

        private void OnBeforeTransformParentChanged()
        {
            if (m_TransformEnabled && m_OnBeforeTransformParentChanged != null) m_OnBeforeTransformParentChanged(luaObject);
        }
        private void OnTransformChildrenChanged()
        {
            if (m_TransformEnabled && m_OnTransformChildrenChanged != null) m_OnTransformChildrenChanged(luaObject);
        }
        private void OnTransformParentChanged()
        {
            if (m_TransformEnabled && m_OnTransformParentChanged != null) m_OnTransformParentChanged(luaObject);
        }

#region Collision
        private void OnCollisionEnter(Collision collision)
        {
            if (m_CollisionEnabled && m_OnCollisionEnter != null) m_OnCollisionEnter(luaObject, collision);
        }
        private void OnCollisionExit(Collision collision)
        {
            if (m_CollisionEnabled && m_OnCollisionExit != null) m_OnCollisionExit(luaObject, collision);
        }
        private void OnCollisionStay(Collision collision)
        {
            if (m_CollisionEnabled && m_OnCollisionStay != null) m_OnCollisionStay(luaObject, collision);
        }
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (m_CollisionEnabled && m_OnCollisionEnter2D != null) m_OnCollisionEnter2D(luaObject, collision);
        }
        private void OnCollisionExit2D(Collision2D collision)
        {
            if (m_CollisionEnabled && m_OnCollisionExit2D != null) m_OnCollisionExit2D(luaObject, collision);
        }
        private void OnCollisionStay2D(Collision2D collision)
        {
            if (m_CollisionEnabled && m_OnCollisionStay2D != null) m_OnCollisionStay2D(luaObject, collision);
        }
        private void OnTriggerEnter(Collider other)
        {
            if (m_TriggerEnabled && m_OnTriggerEnter != null) m_OnTriggerEnter(luaObject, other);
        }
        private void OnTriggerExit(Collider other)
        {
            if (m_TriggerEnabled && m_OnTriggerExit != null) m_OnTriggerExit(luaObject, other);
        }
        private void OnTriggerStay(Collider other)
        {
            if (m_TriggerEnabled && m_OnTriggerStay != null) m_OnTriggerStay(luaObject, other);
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (m_TriggerEnabled && m_OnTriggerEnter2D != null) m_OnTriggerEnter2D(luaObject, collision);
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (m_TriggerEnabled && m_OnTriggerExit2D != null) m_OnTriggerExit2D(luaObject, collision);
        }
        private void OnTriggerStay2D(Collider2D collision)
        {
            if (m_TriggerEnabled && m_OnTriggerStay2D != null) m_OnTriggerStay2D(luaObject, collision);
        }
#endregion

        public virtual void Dispose()
        {
            m_Awake = null;
            m_Start = null;
            m_Update = null;
            m_LateUpdate = null;
            m_FixedUpdate = null;
            m_OnDestroy = null;
            m_OnEnable = null;
            m_OnDisable = null;

            m_OnTransformParentChanged = null;
            m_OnTransformChildrenChanged = null;
            m_OnBeforeTransformParentChanged = null;

            m_OnCollisionEnter = null;
            m_OnCollisionExit = null;
            m_OnCollisionStay = null;
            m_OnCollisionEnter2D = null;
            m_OnCollisionExit2D = null;
            m_OnCollisionStay2D = null;

            m_OnTriggerEnter = null;
            m_OnTriggerExit = null;
            m_OnTriggerStay = null;
            m_OnTriggerEnter2D = null;
            m_OnTriggerExit2D = null;
            m_OnTriggerStay2D = null;

            m_OnApplicationQuit = null;
            m_OnApplicationPause = null;
            m_OnApplicationFocus = null;

            luaObject.Dispose();
            luaObject = null;
        }

        private void OnDestroy()
        {
            if (m_OnDestroy != null) m_OnDestroy(luaObject);
            Dispose();
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.UI;
using XLua;
using LuaDLL = XLua.LuaDLL.Lua;

namespace Spark
{
    [LuaCallCSharp]
    public static partial class ExtensionMethod
    {
        //Component Extension//
        public static void SetActive(this UnityEngine.Component com, bool value)
        {
            com.gameObject.SetActive(value);
        }
        public static void IsActiveSelf(this UnityEngine.Component com, out bool value)
        {
            value = com.gameObject.activeSelf;
        }
        public static void IsActiveInHierarchy(this UnityEngine.Component com, out bool value)
        {
            value = com.gameObject.activeInHierarchy;
        }

        //RectTransform Extension//
        //Get//
        public static void GetAnchoredPosition(this UnityEngine.RectTransform o, out float x, out float y)
        {
            x = o.anchoredPosition.x;
            y = o.anchoredPosition.y;
        }

        public static void GetAnchoredPosition3D(this UnityEngine.RectTransform o, out float x, out float y,
            out float z)
        {
            x = o.anchoredPosition3D.x;
            y = o.anchoredPosition3D.y;
            z = o.anchoredPosition3D.z;
        }

        public static void GetSizeDelta(this UnityEngine.RectTransform o, out float x, out float y)
        {
            x = o.sizeDelta.x;
            y = o.sizeDelta.y;
        }

        public static void GetOffsetMax(this UnityEngine.RectTransform o, out float x, out float y)
        {
            x = o.offsetMax.x;
            y = o.offsetMax.y;
        }

        public static void GetOffsetMin(this UnityEngine.RectTransform o, out float x, out float y)
        {
            x = o.offsetMin.x;
            y = o.offsetMin.y;
        }

        //Set//
        public static void SetAnchoredPosition(this UnityEngine.RectTransform o, float x, float y)
        {
            o.anchoredPosition = new Vector2(x, y);
        }

        public static void SetAnchoredPosition3D(this UnityEngine.RectTransform o, float x, float y, float z)
        {
            o.anchoredPosition3D = new Vector3(x, y, z);
        }

        public static void SetSizeDelta(this UnityEngine.RectTransform o, float x, float y)
        {
            o.sizeDelta = new Vector2(x, y);
        }

        public static void SetOffsetMax(this UnityEngine.RectTransform o, float x, float y)
        {
            o.offsetMax = new Vector2(x, y);
        }

        public static void SetOffsetMin(this UnityEngine.RectTransform o, float x, float y)
        {
            o.offsetMin = new Vector2(x, y);
        }

        //Transform Extension//
        //Get//
        public static void GetPosition(this UnityEngine.Transform o, out float x, out float y, out float z)
        {
            x = o.position.x;
            y = o.position.y;
            z = o.position.z;
        }

        public static void GetLocalPosition(this UnityEngine.Transform o, out float x, out float y, out float z)
        {
            x = o.localPosition.x;
            y = o.localPosition.y;
            z = o.localPosition.z;
        }

        public static void GetRotation(this UnityEngine.Transform o, out float x, out float y, out float z, out float w)
        {
            x = o.rotation.x;
            y = o.rotation.y;
            z = o.rotation.z;
            w = o.rotation.w;
        }

        public static void GetLocalRotation(this UnityEngine.Transform o, out float x, out float y, out float z, out float w)
        {
            x = o.localRotation.x;
            y = o.localRotation.y;
            z = o.localRotation.z;
            w = o.localRotation.w;
        }

        public static void GetEulerAngles(this UnityEngine.Transform o, out float x, out float y, out float z)
        {
            x = o.eulerAngles.x;
            y = o.eulerAngles.y;
            z = o.eulerAngles.z;
        }

        public static void GetLocalEulerAngles(this UnityEngine.Transform o, out float x, out float y, out float z)
        {
            x = o.localEulerAngles.x;
            y = o.localEulerAngles.y;
            z = o.localEulerAngles.z;
        }

        public static void GetLocalScale(this UnityEngine.Transform o, out float x, out float y, out float z)
        {
            x = o.localScale.x;
            y = o.localScale.y;
            z = o.localScale.z;
        }

        //Set//
        public static void SetPosition(this UnityEngine.Transform o, float x, float y, float z)
        {
            o.position = new Vector3(x, y, z);
        }

        public static void SetLocalPosition(this UnityEngine.Transform o, float x, float y, float z)
        {
            o.localPosition = new Vector3(x, y, z);
        }

        public static void SetLocalScale(this UnityEngine.Transform o, float x, float y, float z)
        {
            o.localScale = new Vector3(x, y, z);
        }

        public static void SetEulerAngles(this UnityEngine.Transform o, float x, float y, float z)
        {
            o.eulerAngles = new Vector3(x, y, z);
        }

        public static void SetLocalEulerAngles(this UnityEngine.Transform o, float x, float y, float z)
        {
            o.localEulerAngles = new Vector3(x, y, z);
        }

        public static void SetRotation(this UnityEngine.Transform o, float x, float y, float z, float w)
        {
            o.rotation = new Quaternion(x, y, z, w);
        }

        public static void SetLocalRotation(this UnityEngine.Transform o, float x, float y, float z, float w)
        {
            o.localRotation = new Quaternion(x, y, z, w);
        }

        public static void SetScale(this UnityEngine.Transform o, float x, float y, float z)
        {
            o.localScale = new Vector3(x, y, z);
        }

        //public static Component GetComponent(this UnityEngine.Component o, XLua.LuaTable table)
        //{
        //    IntPtr L = table.L;
        //    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);

        //    LuaTypes lua_type = LuaDLL.lua_type(L, 2);

        //    if (lua_type == LuaTypes.LUA_TTABLE)
        //    {
        //        Component comp = null;
        //        if (translator.Assignable<Component>(L, 2))
        //        {
        //            comp = table.Get<Component>("Component");
        //            return comp;
        //        }
        //    }

        //    return null;
        //}
    }
}
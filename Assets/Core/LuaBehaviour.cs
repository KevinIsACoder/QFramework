/*
 * @Author: zhendong liang
 * @Date: 2022-08-08 20:10:24
 * @LastEditors: Do not edit
 * @LastEditTime: 2022-08-08 20:31:53
 * @Description: lua绑定unity中的方法
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
namespace QFramwork
{
    [LuaCallCSharp]
    public class LuaBehaviour : MonoBehaviour
    {
        [CSharpCallLua]
        public delegate void LuaCallBackDelegate(LuaTable luaTable);
        [CSharpCallLua]
        public delegate void LuaCallBackBoolDelegate(LuaTable luaTable, bool pause);
        [CSharpCallLua]
        public delegate void LuaCallbackCollisionDelegate(LuaTable self, Collision collision);
        [CSharpCallLua]
        public delegate void LuaCallbackCollision2DDelegate(LuaTable self, Collision2D collision);
        [CSharpCallLua]
        public delegate void LuaCallbackColliderDelegate(LuaTable self, Collider collider);
        [CSharpCallLua]
        public delegate void LuaCallbackCollider2DDelegate(LuaTable self, Collider2D collider);
    }
}

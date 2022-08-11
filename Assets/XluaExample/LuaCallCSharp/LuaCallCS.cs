/*
 * @Author: zhendong liang
 * @Date: 2022-08-10 16:15:26
 * @LastEditors: Do not edit
 * @LastEditTime: 2022-08-10 17:59:23
 * @Description: Lua调用c#示例
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using System;

namespace QFramWork
{
    public struct Param
    {
        public int a;
        public string b;
    }

    [LuaCallCSharp]
    public class ClsA
    {
        public void TestFunc(int a)
        {
            Debug.Log("TestFunc++++++++++ " + a);
        }

        public void TestFunc(string b)
        {
            Debug.Log("TestFunc+++++++++++ " + b);
        }

        public int MultiReturnFunc(Param param, ref int a, out int b, out Action<int> cb)
        {
            b = param.a + 1;
            cb = (num) => { num += 1; Debug.Log("MultiReturnFunc++++++++++++++ " + num); };
            a = 100;
            b = 200;
            return b;
        }

        public Action<string> TestDelegate = (param) =>
        {
            Debug.Log("TestDelegate in c#:" + param);
        };

    }

    public class LuaCallCS : MonoBehaviour
    {
        private LuaEnv env;
        private string luaScript;
        // Start is called before the first frame update
        void Start()
        {
            luaScript = @"
            function testFunc()
                local clsA = CS.QFramWork.ClsA()
                clsA:TestFunc(1)
                clsA:TestFunc('test func')

                local ret, a, b, func = clsA:MultiReturnFunc({a = 1, b = 'lzd'}, 2, 3, function() print('MultiReturnFunc+++++++++ 1')  end)
                func()
            end

            testFunc()
        ";
            env = new LuaEnv();
            env.DoString(luaScript);
        }

        // Update is called once per frame
        void Update()
        {
            env.Tick();
        }
    }
}

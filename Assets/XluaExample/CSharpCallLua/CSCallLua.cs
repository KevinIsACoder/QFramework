using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
namespace QFramWork
{
    public class CSCallLua : MonoBehaviour
    {
        [CSharpCallLua]
        public delegate int DMethodTest(int a, int b, out DBClass c); //lua的多返回值映射：从左往右映射到c#的输出参数，输出参数包括返回值，out参数，ref参数

        private DMethodTest dMethodTest;
        private LuaEnv luaEnv;
        private string script = @"
            function a(a, b)
                return 1, {f1 = 3}
            end
        ";
        void Start()
        {
            luaEnv = new LuaEnv();
            luaEnv.DoString(script);
            luaEnv.Global.Get("a", out dMethodTest);

            DBClass dB;
            int result = dMethodTest(1, 1, out dB);
            Debug.Log("result is " + result + "dClass is " + dB.f1);
        }

        void Update()
        {

        }
        void OnDestroy()
        {
            dMethodTest = null;   //这块需要把lua端的引用释放掉
            luaEnv.Dispose();
        }
    }

    public class DBClass
    {
        public int f1 = 1;
        public int f2 = 2;
    }

}
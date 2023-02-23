using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace XLua
{
    public interface ILuaGCHandler
    {
        void OnLuaGC();
    }
}

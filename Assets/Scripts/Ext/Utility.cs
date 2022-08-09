/*
 * @Author: zhendong liang
 * @Date: 2022-08-09 16:25:14
 * @LastEditors: Do not edit
 * @LastEditTime: 2022-08-09 16:27:45
 * @Description: 封装一些常用的方法
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
public class Utility
{
    [LuaCallCSharp]
    public static bool CheckIsNull(object obj)
    {
        return obj == null;
    }
}

/*
 * @Author: zhendong liang
 * @Date: 2022-08-17 15:30:56
 * @LastEditors: Do not edit
 * @LastEditTime: 2022-08-17 17:28:10
 * @Description: 资源加载管理器
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QFramWork
{
    public static class AssetsManager
    {
        public static void Init()
        {
            
        }

        public static T LoadAssets<T>(string path, System.Type type) where T : Object
        {
            return LoadAssets(path, type) as T;
        }

        public static Object LoadAssets(string path, System.Type type)
        {
            Object obj = null;
            return obj;
        }
        

        private static void UnLoadAssets(string ab)
        {
            
        }
    }

}
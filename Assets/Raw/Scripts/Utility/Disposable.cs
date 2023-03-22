/*
 * @Author: zhendong liang
 * @Date: 2022-08-24 16:19:58
 * @LastEditors: Do not edit
 * @LastEditTime: 2022-08-24 18:01:11
 * @Description: 释放非托管资源
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace QFramWork
{
    public abstract class Disposable : IDisposable
    {
        ~Disposable()
        {
            Dispose(false);
        }
        private bool m_disposed = false;

        public void Dispose()
        {
            //必须为true
            Dispose(true);
            //通知垃圾回收器不再调用终结器
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if(m_disposed)
            {
                return;
            }

            if(disposing)
            {
                if(disposing)
                {
                    DisposeManagedObjects();
                }
                DisposeUnmanagedObjects();
            }
            m_disposed = true;
        }

        //释放托管资源
        protected abstract void DisposeManagedObjects();

        //释放非托管资源
        protected virtual void DisposeUnmanagedObjects()
        {

        }
    }
}

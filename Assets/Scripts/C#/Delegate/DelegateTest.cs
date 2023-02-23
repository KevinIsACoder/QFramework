/*
 * @Author: zhendong liang
 * @Date: 2023-02-22 16:12:03
 * @LastEditors: Do not edit
 * @LastEditTime: 2023-02-22 18:30:47
 * @Description: 
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Runtime.Remoting.Messaging;
public class DelegateTest : MonoBehaviour
{
    void Start()
    {
        var pb = new Calculater(new Subscriber());
    }
}

public class Calculater
{
    public delegate int Add(int x, int y);

    public event Add PublishTest;

    private Add AddDelegate;

    public Calculater(Subscriber sb)
    {
        AddDelegate = sb.Add;
        AsyncCallback cb = new AsyncCallback(OnAddResult);
        AddDelegate.BeginInvoke(2, 2, cb, "");
    }

    public void OnAddResult(IAsyncResult result)
    {
        // var asyncResult = (AsyncResult)
    }
    public int DOSomthing(int x, int y)
    {
        var subscriber = new Subscriber();
        return x + y;
    }

    public int AddNum(int x, int y)
    {
        return x + y;
    }

}

public class Subscriber
{
    public int Add(int x, int y)
    {
        return x + y;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[XLua.LuaCallCSharp]
public class PlatformResponse : MonoBehaviour
{
    public bool IsReceiveMessage = true;
    private Action<string> m_onMessage;
    private Action<byte[]> m_onSocketMessage;

    //注册platform接收回调
    public void OnReceiveMessage(Action<string> callback, Action<byte[]> socketCallback)
    {
        m_onMessage = callback;
        m_onSocketMessage = socketCallback;
#if UNITY_EDITOR && !GAME_SOCKET
       FakeSocket.Get().Init(m_onSocketMessage, m_onMessage);
#endif
    }

    /// <summary>
    /// 接收来自android和ios的消息
    /// </summary>
    /// <param name="msg"></param>
    [XLua.BlackList]
    public void OnMessage(string msg)
    {
        Debug.Log("OnMessage=" + msg);
        m_onMessage?.Invoke(msg);
    }

    [XLua.BlackList]
    public void OnSocketMessage(string msgStr)
    {
        if (m_onSocketMessage != null)
        {
            byte[] msgBytes = Convert.FromBase64String(msgStr);
            //Debug.Log("OnSocketMessage C#:::" + msgBytes.Length + ":::" + msgStr.Length + ":::" + msgStr);
            m_onSocketMessage(msgBytes);
        }
    }

#if UNITY_EDITOR && !GAME_SOCKET
   private void OnGUI()
   {
       DebugGUI.ShowGUI();
   }
#endif

    #region Socket

    public void SendMessageToNative(string msg)
    {
#if GAME_SOCKET
        //Debug.Log("发送消息到服务端:"+msg);
        if (IsReceiveMessage)
        {
            _client?.SendMessage(msg);
        }
#endif
    }

#if GAME_SOCKET
    private SocketClient _client;
    private Queue<string> _receivedMessages = new Queue<string>();

    [XLua.BlackList]
    public void OpenSocket(string ip = "119.28.109.92", int port = 58887)
    {
        Debug.Log($"Connecting to gateway {ip}:{port}");
        //game-test.starmakerstudios.com
//        _client = new SocketClient("119.28.109.92", 58887);
        //_client = new SocketClient("10.41.3.176", 58887);       //亚斌
        //_client = new SocketClient("10.100.6.2", 58887);  //马健
        _client = new SocketClient(ip, port);  //马健
        
        _client.OnMessage += (msg) =>
        {
            lock (_receivedMessages)
            {
                _receivedMessages.Enqueue(msg);
            }
        };
        _client.OnConnected += () =>
        {
            Debug.Log("Connected");
        };
        _client.Connect();
    }

    private void Start()
    {
        UIDebugLogin.Instance.OnLogin = (uid, rid, gtype, reset, robotNum, role) =>
        {
            Debug.LogError("role = " + role);
            var ht = new Hashtable()
            {
                {"messageName", "login"},
                {
                    "data", new Dictionary<string, long>()
                    {
                        {"user_id", uid},
                        {"room_id", rid},
                        {"game_type", gtype},
                        {"reset", reset},
                        {"robot_num", robotNum},
                        {"role", role }
                    }
                },
            };

            var msg = Spark.JSON.Stringify(ht);

            SendMessageToNative(msg);
        };
        //OpenSocket();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Z))
        {
            IsReceiveMessage = !IsReceiveMessage;
        }

        lock (_receivedMessages)
        {
            while (_receivedMessages.Count > 0)
            {
                var msg = _receivedMessages.Dequeue();
                try
                {
                    if (IsReceiveMessage)
                    {
                        Debug.Log(msg);
                        var jsonObj = Spark.JSON.Parse(msg) as Hashtable;
                        if (jsonObj["method_name"].ToString() == "gameMsg")
                        {
                            OnSocketMessage(jsonObj["data"].ToString());
                        }
                        else
                        {
                            OnMessage(msg);
                        }
                    }
                    
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                }
            }
        }
    }
#endif

    #endregion
}
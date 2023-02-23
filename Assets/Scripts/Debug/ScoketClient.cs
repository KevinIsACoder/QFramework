using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System;
using System.Threading;

public class SocketClient
{
    public Action OnClosed;
    public Action OnConnected;
    public Action<string> OnMessage;
    //public Action 

    private Socket m_ClientSocket;
    private string m_Host;
    private int m_Port;

    public SocketClient(string host, int port)
    {
        this.m_Host = host;
        this.m_Port = port;
    }

    public void Connect()
    {
        m_ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAddress = IPAddress.Parse(m_Host);
        IPEndPoint ipEndpoint = new IPEndPoint(ipAddress, m_Port);

        IAsyncResult result = m_ClientSocket.BeginConnect(ipEndpoint, new AsyncCallback(ConnectCallback), m_ClientSocket);
        bool success = result.AsyncWaitHandle.WaitOne(5000, true);
        if (!success)
        {
            //超时
            Close();
            Debug.Log("connect Time Out");
        }
        else
        {
            //与socket建立连接成功，开启线程接受服务端数据。  
            //worldpackage = new List<JFPackage.WorldPackage>();
            Thread thread = new Thread(new ThreadStart(ReceiveMessage));
            thread.IsBackground = true;
            thread.Start();
        }
    }

    public void SendMessage(string msg)
    {
        if (m_ClientSocket == null || !m_ClientSocket.Connected) return;

        var body = System.Text.Encoding.UTF8.GetBytes(msg);
        var data = new byte[body.Length + 6];
        Array.Copy(body, 0, data, 6, body.Length);
        var length = body.Length;
        data[0] = (byte)((length & 0x00FF));
        data[1] = (byte)((length & 0xFF00) >> 8);
        m_ClientSocket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), m_ClientSocket);
    }

    private void SendCallback(IAsyncResult result)
    {
        Debug.Log("send complete: " + result.IsCompleted);
    }

    bool m_Connected = false;

    private void ConnectCallback(IAsyncResult result)
    {
        m_Connected = true;
        OnConnected?.Invoke();
    }

    private byte[] m_TempBuffer = new byte[4096];
    private SocketBuffer m_DataBuffer = new SocketBuffer();

    private void ReceiveMessage()
    {
        //在这个线程中接受服务器返回的数据  
        while (true)
        {
            if (!m_ClientSocket.Connected)
            {
                //与服务器断开连接跳出循环  
                Debug.Log("Failed to clientSocket server.");
                m_ClientSocket.Close();

                OnClosed?.Invoke();
                break;
            }

            try
            {
                int receiveLength = m_ClientSocket.Receive(m_TempBuffer);
                if (receiveLength > 0)
                {
//                    Debug.Log("receive bytes:" + receiveLength);
                    m_DataBuffer.Add(m_TempBuffer, 0, receiveLength);
                    while(true)
                    {
                        var msg = m_DataBuffer.GetMessage();
                        if (msg == null) break;
                        OnMessage?.Invoke(msg);
                        //Debug.Log("receive message: " + msg);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                m_ClientSocket.Close();
                m_Connected = false;
                OnClosed?.Invoke();
                Debug.LogError("Close-------------------------------");
                break;
            }
        }
    }

    //关闭Socket  
    public void Close()
    {
        if (m_ClientSocket != null && m_ClientSocket.Connected)
        {
            m_ClientSocket.Shutdown(SocketShutdown.Both);
            m_ClientSocket.Close();
        }
        m_ClientSocket = null;


        OnClosed?.Invoke();
    }
}

class SocketBuffer
{
    private byte[] _data;

    private int _begin;
    private int _count;
    private int _capacity;

    public SocketBuffer()
    {
        _begin = 0;
        _count = 0;
        _capacity = 4096;
        _data = new byte[_capacity];
    }

    public void Add(byte[] bytes, int begin, int length)
    {
        if (_capacity - _count < length)
        {
            _capacity += length * 2;
            var temp = new byte[_capacity];
            if (_count > 0)
            {
                Array.Copy(_data, _begin, temp, 0, _count);
                _begin = 0;
            }
            _data = temp;
        } else if (_capacity - _begin - _count < length)
        {
            Array.Copy(_data, _begin, _data, 0, _count);
            _begin = 0;
        }
        Array.Copy(bytes, 0, _data, _begin + _count, length);
        _count = _count + length;
    }

    public string GetMessage()
    {
        if (_count < 6) return null;
        int length = _data[_begin] | (_data[_begin + 1] << 8);
        //Debug.Log("length:" + length + ",byte[0]:" + _data[_begin] + ",byte[1]:" + _data[_begin + 1]);
        if (_count < (6 + length)) return null;
        var msg = System.Text.Encoding.UTF8.GetString(_data, _begin + 6, length);
        _begin = _begin + 6 + length;
        _count = _count - 6 - length;
        return msg;
    }
}

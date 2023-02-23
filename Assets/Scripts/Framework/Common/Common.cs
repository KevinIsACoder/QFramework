using System.Text;
using UnityEngine;
using System.Security.Cryptography;
using System;
using UnityEngine;
using System.IO;
public static class Common
{
    public static Component AddTSRGameObject(string objName, string compName)
    {
        GameObject TSR = GameObject.Find("TSR");
        GameObject go = null;
        Transform tf = TSR.transform.Find(objName);
        if (tf != null) { go = tf.gameObject; }
        if (go == null)
        {
            go = new GameObject(objName);
            go.transform.parent = TSR.transform;
        }

        Component ret;
        System.Type scriptType = System.Type.GetType(compName);
        ret = go.GetComponent(scriptType);
        if (ret==null)
        {
            ret = go.AddComponent(scriptType);
        }

        return ret;
    }
    
    public static Component AddGameObject(string objName, string compName)
    {
        GameObject go = null;
        GameObject tf = GameObject.Find(objName);
        if (tf != null) { go = tf.gameObject; }
        if (go == null)
        {
            go = new GameObject(objName);
        }

        Component ret;
        System.Type scriptType = System.Type.GetType(compName);
        ret = go.GetComponent(scriptType);
        if (ret==null)
        {
            ret = go.AddComponent(scriptType);
        }

        return ret;
    }

    public static byte[] GetBytesUTF8(string str)
    {
        return System.Text.Encoding.UTF8.GetBytes(str);
    }
    
    /// 去除文件bom头后的字符
    public static string RemoveBOMRetString(byte[] buffer)
    {
        if (buffer == null)
            return null;

        if (buffer.Length <= 3)
        {
            return Encoding.UTF8.GetString(buffer);
        }

        byte[] bomBuffer = new byte[] { 0xef, 0xbb, 0xbf };

        if (buffer[0] == bomBuffer[0]
          && buffer[1] == bomBuffer[1]
          && buffer[2] == bomBuffer[2])
        {
            return new UTF8Encoding(false).GetString(buffer, 3, buffer.Length - 3);
        }

        return Encoding.UTF8.GetString(buffer);
    }

    public static byte[] RemoveBOMRetByte(byte[] buffer)
    {
        if (buffer == null)
            return null;

        if (buffer.Length <= 3)
        {
            return buffer;
        }

        byte[] bomBuffer = new byte[] { 0xef, 0xbb, 0xbf };

        if (buffer[0] == bomBuffer[0]
          && buffer[1] == bomBuffer[1]
          && buffer[2] == bomBuffer[2])
        {
            byte[] newBuf = new byte[buffer.Length - 3];
            System.Buffer.BlockCopy(buffer, 3, newBuf, 0, buffer.Length - 3);
            return newBuf;
        }

        return buffer;
    }

    public static System.Uri GetPathAndQueryFromURL(string url){
        System.Uri uriAddress = new System.Uri (url);
//        UnityEngine.Debug.Log(uriAddress.Scheme);
//		UnityEngine.Debug.Log(uriAddress.Authority);
//		UnityEngine.Debug.Log(uriAddress.Host);
//		UnityEngine.Debug.Log(uriAddress.Port);
//		UnityEngine.Debug.Log(uriAddress.AbsolutePath);
//		UnityEngine.Debug.Log(uriAddress.Query);
//		UnityEngine.Debug.Log(uriAddress.Fragment);
//		//通过UriPartial枚举获取指定的部分
//		UnityEngine.Debug.Log(uriAddress.GetLeftPart(UriPartial.Path));
//		//获取整个URI
//		UnityEngine.Debug.Log(uriAddress.AbsoluteUri);
        return uriAddress;
    }

	public static string URLBase64Encode(byte[] plainTextBytes){
           var base64 = Convert.ToBase64String(plainTextBytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
           return base64;
    }

	public static byte[] URLBase64Decode(string secureUrlBase64){
        secureUrlBase64 = secureUrlBase64.Replace('-', '+').Replace('_', '/');
            switch (secureUrlBase64.Length % 4) {
                case 2:
                    secureUrlBase64 += "==";
                    break;
                case 3:
                    secureUrlBase64 += "=";
                    break;
            }
            var bytes = Convert.FromBase64String(secureUrlBase64);
		return bytes;
    }
}

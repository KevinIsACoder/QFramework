//----------------------------------------------------
// Spark: A Framework For Unity
// Copyright © 2014 - 2015 Jay Hu (Q:156809986)
//----------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Spark
{
    [XLua.LuaCallCSharp]
    public static class FileHelper
    {
        private static readonly UTF8Encoding UTF8 = new UTF8Encoding(false);

#if !UNITY_WEBPLAYER
        public static string GetMD5Hash(string path)
        {
            if (!File.Exists(path))
                return string.Empty;

            return GetMD5Hash(File.ReadAllBytes(path));
        }

        [XLua.BlackList]
        public static string GetMD5Hash(byte[] buffer)
        {
            if (buffer == null)
                return string.Empty;

            MD5 md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(buffer)).Replace("-", "").ToLower();
        }

        public static byte[] ReadBytes(string path)
        {
            return !File.Exists(path) ? null : File.ReadAllBytes(path);
        }

        public static string ReadString(string path)
        {
            return !File.Exists(path) ? null : File.ReadAllText(path, UTF8);
        }

        public static void WriteBytes(string path, byte[] data)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, data);
        }

        public static void WriteString(string path, string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, content, UTF8);
        }

        public static void AppendText(string path, string content)
        {
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            File.AppendAllText(path, content, UTF8);
        }

        public static bool ExistsFile(string path)
        {
            return File.Exists(path);
        }

        public static bool ExistsDirectory(string path)
        {
            return Directory.Exists(path);
        }

        public static void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public static void DeleteDirectory(string path, bool recursive)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive);
            }
        }
        
        // createNew 是否重新创建新的路径
        public static void MakeDirs(string path, bool pathIsFile = true, bool createNew = false)
        {
            if (pathIsFile) {
                path = Path.GetDirectoryName(path);
            }
            if (!Directory.Exists(path)) {
                try {
                    Directory.CreateDirectory(path);
                }
                catch(Exception e) {
                    throw new UnityEngine.UnityException("Can't create directories for '" + path + "' (" + e.Message + ")");
                }
            }
            else
            {
                if (createNew)
                {
                    DeleteDirectory(path, true);
                    MakeDirs(path, pathIsFile);
                }
            }
        }
        
        public static void CopyFile(string from, string to, bool overwrite)
        {
            MakeDirs(to);
            File.Copy(from, to, overwrite);
        }

        public static long Length(string path)
        {
            return !ExistsFile(path) ? 0 : new FileInfo(path).Length;
        }

        public static string HttpRequest(string url)
        {
            var hwRequest = (HttpWebRequest) WebRequest.Create(url);
            //hwRequest.Timeout = 30000;
            hwRequest.Method = "GET";
            hwRequest.ContentType = "application/x-www-form-urlencoded";

            var hwResponse = (HttpWebResponse) hwRequest.GetResponse();
            var srReader = new StreamReader(hwResponse.GetResponseStream(), Encoding.ASCII);
            var strResult = srReader.ReadToEnd();
            srReader.Close();
            hwResponse.Close();
            return strResult;
        }

        public static void Download(string url, string path, Action onComplete, Action<long, string> onError,
            Action<ulong> onDownloadedBytes)
        {
            SparkHelper.StartCoroutine(_Download(url, path, onComplete, onError, onDownloadedBytes));
        }

        private static IEnumerator _Download(string url, string path, Action onComplete, Action<long, string> onError,
            Action<ulong> onDownloadedBytes)
        {
            using (var www = UnityWebRequest.Get(url))
            {
                var dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                www.downloadHandler = new DownloadHandlerFile(path) {removeFileOnAbort = true};
                www.timeout = 30;
                www.disposeDownloadHandlerOnDispose = true;

                var op = www.SendWebRequest();
                if (onDownloadedBytes != null)
                {
                    ulong downloadedBytes = 0;
                    do
                    {
                        var d = www.downloadedBytes;
                        if (d != downloadedBytes)
                        {
                            downloadedBytes = d;
                            onDownloadedBytes(d);
                        }

                        yield return null;
                    } while (!op.isDone);
                }
                else
                {
                    yield return op;
                }

                if (www.isNetworkError || www.isHttpError)
                {
                    onError?.Invoke(www.responseCode, www.error);
                }
                else
                {
                    onComplete?.Invoke();
                }
            }
        }
#endif
    }
}
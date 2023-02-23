using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.ObjectModel;
namespace Spark
{
    [XLua.LuaCallCSharp]
    public class HttpResponse
    {
        public byte[] data { get; internal set; }

        private string _text;
        public string text
        {
            get
            {
                if (_text == null && data != null)
                {
                    _text = System.Text.Encoding.UTF8.GetString(data);
                }
                return _text;
            }
        }

        internal UnityWebRequest.Result _result;

        public string error { get; internal set; }
        public long statusCode { get; internal set; }

        public bool isDone => !isError;

        public bool isError => _result != UnityWebRequest.Result.Success;
        public bool isDataError => _result == UnityWebRequest.Result.DataProcessingError;
        public bool isHttpError => _result == UnityWebRequest.Result.ProtocolError;
        public bool isNetworkError => _result == UnityWebRequest.Result.ConnectionError;

        public ReadOnlyDictionary<string, string> headers { get; internal set; }

        public bool HasHeader(string header) => headers != null && headers.ContainsKey(header);

        public bool TryGetHeaderValue(string header, out string value)
        {
            if (headers == null)
            {
                value = null;
                return false;
            }
            return headers.TryGetValue(header, out value);
        }
        public string GetHeaderValue(string header)
        {
            string value;
            TryGetHeaderValue(header, out value);
            return value;
        }
    }

    [XLua.LuaCallCSharp]
    public static class HttpHelper
    {
        [XLua.CSharpCallLua]
        public delegate void OnResponseDelegate(HttpResponse response);
        [XLua.CSharpCallLua]
        public delegate void OnProgressDelegate(float progress, ulong downloadedBytes);

        #region GET
        static public void Get(string url, OnResponseDelegate onResponse = null, OnProgressDelegate onProgress = null)
        {
            Get(url, null, onResponse, onProgress);
        }
        static public void Get(string url, Dictionary<string, string> headers, OnResponseDelegate onResponse = null, OnProgressDelegate onProgress = null)
        {
            SparkHelper.StartCoroutine(_Request(UnityWebRequest.Get(url), headers, onResponse, onProgress));
        }
        #endregion

        #region POST
        static public void Post(string url, OnResponseDelegate onResponse = null, OnProgressDelegate onProgress = null)
        {
            Post(url, new byte[0], onResponse, onProgress);
        }
        static public void Post(string url, WWWForm form, OnResponseDelegate onResponse = null, OnProgressDelegate onProgress = null)
        {
            Post(url, form, null, onResponse, onProgress);
        }
        static public void Post(string url, WWWForm form, Dictionary<string, string> headers, OnResponseDelegate onResponse = null, OnProgressDelegate onProgress = null)
        {
            SparkHelper.StartCoroutine(_Request(UnityWebRequest.Post(url, form), headers, onResponse, onProgress));
        }

        static public void Post(string url, byte[] data, OnResponseDelegate onResponse = null, OnProgressDelegate onProgress = null)
        {
            Post(url, data, null, onResponse, onProgress);
        }
        static public void Post(string url, byte[] data, Dictionary<string, string> headers, OnResponseDelegate onResponse = null, OnProgressDelegate onProgress = null)
        {
            // var sections = new List<IMultipartFormSection>();
            // if (data != null && data.Length > 0)
            // {
            //     sections.Add(new MultipartFormDataSection(data));
            // }
            // Debug.Log(sections);
            var www = new UnityWebRequest(url, "POST");
            www.disposeDownloadHandlerOnDispose = true;
            www.downloadHandler = new DownloadHandlerBuffer();
            if (data != null && data.Length > 0) {
                www.disposeUploadHandlerOnDispose = true;
                www.uploadHandler = new UploadHandlerRaw(data);
            }
            SparkHelper.StartCoroutine(_Request(www, headers, onResponse, onProgress));
        }
        #endregion

        static private IEnumerator _Request(UnityWebRequest www, Dictionary<string, string> headers, OnResponseDelegate onResponse, OnProgressDelegate onProgress)
        {
            www.timeout = 20;
            if (headers != null)
            {
                foreach (var kv in headers)
                {
                    www.SetRequestHeader(kv.Key, kv.Value);
                }
            }
            var op = www.SendWebRequest();
            if (onProgress != null)
            {
                ulong downloadedBytes = 0;
                do
                {
                    var d = www.downloadedBytes;
                    if (d != downloadedBytes)
                    {
                        downloadedBytes = d;
                        onProgress(www.downloadProgress, d);
                    }
                    yield return null;
                } while (!op.isDone);
            }
            else
            {
                yield return op;
            }

            if (onResponse != null)
            {
                var response = new HttpResponse()
                {
                    _result = www.result,
                };
                if (www.result == UnityWebRequest.Result.Success)
                {
                    response.data = www.downloadHandler.data;
                    response.headers = new ReadOnlyDictionary<string, string>(www.GetResponseHeaders());
                }
                else
                {
                    response.error = www.error;
                    response.statusCode = www.responseCode;
                }
                onResponse(response);
            }

            //if (www.result == UnityWebRequest.Result.Success)
            //{
            //    if (onComplete != null)
            //    {
            //        var response = new HttpResponse()
            //        {
            //            _result = www.result,
            //            data = www.downloadHandler.data,
            //            headers = new ReadOnlyDictionary<string, string>(www.GetResponseHeaders())
            //        };
            //        onComplete(response);
            //    }
            //}
            //else
            //{
            //    if(onError != null)
            //    {
            //        var response = new HttpResponse()
            //        {
            //            error = www.error,
            //            statusCode = www.responseCode,
            //            _result = www.result
            //        };

            //        onComplete(response);
            //        onError?.Invoke(www.result, www.responseCode, www.error);
            //    }
            //}
        }
    }
}

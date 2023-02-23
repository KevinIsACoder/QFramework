using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Spark
{
    [XLua.LuaCallCSharp]
    public class LuaTexture : XLua.ILuaGCHandler, System.IDisposable
    {
        static public LuaTexture LoadFromFile(string file)
        {
            LuaTexture texture = null;
            UnityEngine.Profiling.Profiler.BeginSample("Load:" + file);
            var data = FileHelper.ReadBytes(file);
            if (data != null)
            {
                texture = new LuaTexture(data);
            }
            UnityEngine.Profiling.Profiler.EndSample();
            return texture;
        }

        static public LuaTexture LoadFromBytes(byte[] data)
        {
            LuaTexture texture = null;
            UnityEngine.Profiling.Profiler.BeginSample("LoadFromBytes");
            if (data != null)
            {
                texture = new LuaTexture(data);
            }
            UnityEngine.Profiling.Profiler.EndSample();
            return texture;
        }

        private Texture2D _tex;
        public Texture2D texture => _tex;

        private Sprite _sprite;
        public Sprite sprite
        {
            get
            {
                if (_sprite == null)
                {
                    if (_tex != null)
                    {
                        UnityEngine.Profiling.Profiler.BeginSample("Load Sprite");
                        _sprite = Sprite.Create(_tex, new Rect(0.0f, 0.0f, _tex.width, _tex.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
                        UnityEngine.Profiling.Profiler.EndSample();
                    }
                }
                return _sprite;
            }
        }

        private LuaTexture(byte[] data)
        {
            _tex = LoadTexture(data);
        }

        private Texture2D LoadTexture(byte[] data)
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.wrapMode = UnityEngine.TextureWrapMode.Clamp;
            if (!ImageConversion.LoadImage(tex, data, false))
            {
                UnityEngine.Object.Destroy(tex);
                tex = null;
            }
            return tex;
        }

        public void Destroy()
        {
            if (_sprite != null)
            {
                UnityEngine.Object.Destroy(_sprite);
                _sprite = null;
            }
            if (_tex != null)
            {
                UnityEngine.Object.Destroy(_tex);
                _tex = null;
            }
        }

        [XLua.BlackList]
        public void Dispose()
        {
            Destroy();
        }

        [XLua.BlackList]
        public void OnLuaGC()
        {
            Destroy();
        }
    }
}
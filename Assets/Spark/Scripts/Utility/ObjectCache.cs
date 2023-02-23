using System;
using System.Collections;
using System.Collections.Generic;

namespace Spark
{
    [XLua.LuaCallCSharp]
    public static class ObjectCache {
		static private readonly Dictionary<string, WeakReference> s_ObjectCaches = new Dictionary<string, WeakReference>();
		static private readonly ObjectPool<WeakReference> s_ObjRefPool = new ObjectPool<WeakReference>(() => new WeakReference(null), null, (v) => v.Target = null);

		static public void SetObject(string key, Object obj) {
			WeakReference reference;
			if (s_ObjectCaches.TryGetValue(key, out reference)) {
				reference.Target = obj;
			}else {
				reference = s_ObjRefPool.Get();
				reference.Target = obj;
				s_ObjectCaches[key] = reference;
			}
		}

		static public Object GetObject(string key) {
			WeakReference reference;
			if (s_ObjectCaches.TryGetValue(key, out reference)) {
				if (IsAlive(reference)) {
					return reference.Target as Object;
				}
				s_ObjectCaches.Remove(key);
				s_ObjRefPool.Release(reference);
			}
			return null;
		}

        static public void RemoveObject(string key) {
            WeakReference reference;
			if (s_ObjectCaches.TryGetValue(key, out reference)) {
                s_ObjectCaches[key] = null;
                s_ObjRefPool.Release(reference);
			}
        }

        static private bool IsAlive(WeakReference reference)
		{
			var target = reference.Target;
			return target != null && !target.Equals(null);
		}

        static internal void RemoveUnusedObjects() {
            // foreach(var kv in s_ObjectCaches) {
            //     var reference = kv.Value;
            //     if (!IsAlive(reference)) {
            //         s_ObjectCaches[kv.Key] = null;
            //         s_ObjRefPool.Release(reference);
            //     }
            // }
        }
    }
}
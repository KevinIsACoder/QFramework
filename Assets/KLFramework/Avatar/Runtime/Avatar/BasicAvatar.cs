using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KLFramework.Avatar
{
    //******************************************
    // BasicAvatar
    //
    // @Author: Kakashi
    // @Email: john.cha@qq.com
    // @Date: 2022-07-25 16:04
    //******************************************
    public class BasicAvatar<C> : IAvatar<C>
    {
        private static int _ID;
        
        private ISkeleton _Skeleton;
        private IPartManager<C> _PartManager;

        private GameObject _GameObject;
        private Transform _Transform;
        private bool _Dirty;
        
        public BasicAvatar(IPartManager<C> partManager = null, Transform parent = null, string name = null)
        {
            _PartManager = partManager ?? new BasicPartManager<C>();
            _GameObject = new GameObject(name ?? ("Avatar#" + _ID++));
            _Transform = _GameObject.transform;
            if (parent != null)
            {
                _Transform.SetParent(parent, false);
            }
        }

        public void SetSkeleton(ISkeleton skeleton)
        {
            _Dirty = true;
            _Skeleton = skeleton;
        }

        public ISkeleton GetSkeleton()
        {
            return _Skeleton;
        }

        public void AddPart(int partType, IAvatarPart<C> part)
        {
            _Dirty = true;
            _PartManager.AddPart(partType, part);
        }

        public IAvatarPart<C> RemovePart(int partType)
        {
            return _PartManager.RemovePart(partType);
        }

        public IAvatarPart<C> GetPart(int partType)
        {
            return _PartManager.GetPart(partType);
        }

        public GameObject GetGameObject()
        {
            return _GameObject;
        }

        public Transform GetTransform()
        {
            return _Transform;
        }

        public Animator GetAnimator()
        {
            var obj = _Skeleton.GetSkeleton().gameObject;
            return obj.GetComponent<Animator>();
        }

        public int GetAvatarId()
        {
            return _ID - 1;
        }

        public void ApplyChange()
        {
            _TryApply();
        }

        private void _TryApply()
        {
            if (!_Dirty)
            {
                return;
            }

            _Dirty = false;

            _Apply();
        }

        private void _Apply()
        {
            foreach (var avatarPart in _PartManager)
            {
                avatarPart.Apply(this);
            }
        }
        

        public void Destroy()
        {
            GameObject.Destroy(_GameObject);
            _Skeleton = null;
        }

        public IEnumerator<IAvatarPart<C>> GetEnumerator()
        {
            return _PartManager.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _PartManager.GetEnumerator();
        }
    }
}
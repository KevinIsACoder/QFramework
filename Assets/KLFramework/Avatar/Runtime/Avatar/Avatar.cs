using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KLFramework.Avatar
{

    //******************************************
    // Avatar
    //
    // @Author: Kakashi
    // @Email: john.cha@qq.com
    // @Date: 2022-07-30 16:47
    //******************************************
    public class Avatar<C> : IAvatar<C>
    {
        private IAvatar<C> _Avatar;
        private IAvatarLifeCycle<C> _AvatarLifeCycle;
        private IAvatarRule<C> _AvatarRule;
        
        public Avatar(IAvatar<C> avatar, IAvatarRule<C> avatarRule, IAvatarLifeCycle<C> avatarLifeCycle = null)
        {
            _Avatar = avatar;
            _AvatarRule = avatarRule;
            _AvatarLifeCycle = avatarLifeCycle;
        }

        public IEnumerator<IAvatarPart<C>> GetEnumerator()
        {
            return _Avatar.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Avatar.GetEnumerator();
        }

        public void AddPart(int partType, IAvatarPart<C> part)
        {
            int[] parts = _AvatarRule.GetMuteParts(partType, part);
            if (parts != null)
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    IAvatarPart<C> removePart = _Avatar.RemovePart(parts[i]);
                    if (removePart == null)
                    {
                        continue;
                    }
                    _AvatarLifeCycle?.OnRemovePart(this, removePart);
                }
            }
            _Avatar.AddPart(partType, part);
            _AvatarLifeCycle?.OnAddPart(this, part);
        }

        public IAvatarPart<C> RemovePart(int partType)
        {
            var removePart = _Avatar.RemovePart(partType);
            if (removePart != null)
            {
                _AvatarLifeCycle?.OnRemovePart(this, removePart);
            }
            return removePart;
        }

        public IAvatarPart<C> GetPart(int partType)
        {
            return _Avatar.GetPart(partType);
        }

        public void SetSkeleton(ISkeleton skeleton)
        {
            var oldSkeleton = _Avatar.GetSkeleton();
            _Avatar.SetSkeleton(skeleton);
            _AvatarLifeCycle?.OnChangeSkeleton(this, oldSkeleton, skeleton);
        }

        public ISkeleton GetSkeleton()
        {
            return _Avatar.GetSkeleton();
        }

        public GameObject GetGameObject()
        {
            return _Avatar.GetGameObject();
        }

        public Transform GetTransform()
        {
            return _Avatar.GetTransform();
        }

        public int GetAvatarId()
        {
            return _Avatar.GetAvatarId();
        }

        public Animator GetAnimator()
        {
            var skeleton = GetSkeleton();
            return skeleton.GetSkeleton().gameObject.GetComponent<Animator>();
        }
        public void ApplyChange()
        {
            _Avatar.ApplyChange();
        }

        public void Destroy()
        {
            _Avatar.Destroy();
        }
    }
}
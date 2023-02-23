
using UnityEngine;

namespace KLFramework.Avatar
{
    //******************************************
    // IAvatar
    //
    // @Author: Kakashi
    // @Email: john.cha@qq.com
    // @Date: 2022-07-25 16:04
    //******************************************
    public interface IAvatar<C> : IPartManager<C>
    {
        public void SetSkeleton(ISkeleton skeleton);
        
        public ISkeleton GetSkeleton();

        public GameObject GetGameObject();
        public Transform GetTransform();

        public int GetAvatarId();
        public Animator GetAnimator();
        void ApplyChange();

        void Destroy();
    }
}
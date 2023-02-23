using JetBrains.Annotations;
using UnityEngine;

namespace KLFramework.Avatar
{

    //******************************************
    // AttachRendererPart
    //
    // @Author: Kakashi
    // @Email: john.cha@qq.com
    // @Date: 2022-07-30 16:51
    //******************************************
    public class AttachRendererPart<C> : IAvatarPart<C>
    {
        public C Config { get; }
        
        private Transform _Transform;
        private string _AttachPath;
        
        public AttachRendererPart(C config, GameObject go, string attachPath) : this(config, go.transform, attachPath)
        {
        }

        public AttachRendererPart(C config, Transform transform, string attachPath)
        {
            Config = config;
            _Transform = transform;
            _AttachPath = attachPath;
        }

        public void Apply(IAvatar<C> avatar)
        {
            if (_Transform == null || avatar == null)
            {
                return;
            }

            var skeleton = avatar.GetSkeleton();
            if (skeleton == null)
            {
                return;
            }
            
            var transform = skeleton.Find(_AttachPath);
            _Transform.SetParent(transform, false);
        }

        public void Destroy()
        {
            if (_Transform != null)
            {
                GameObject.Destroy(_Transform.gameObject);
                _Transform = null;
            }
        }
    }
}
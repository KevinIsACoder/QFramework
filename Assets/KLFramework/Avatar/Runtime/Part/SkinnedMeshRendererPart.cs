using System.Collections.Generic;
using UnityEngine;

namespace KLFramework.Avatar
{

    //******************************************
    // SkinnedMeshRendererPart
    //
    // @Author: Kakashi
    // @Email: john.cha@qq.com
    // @Date: 2022-07-26 14:24
    //******************************************
    public class SkinnedMeshRendererPart<C> : IAvatarPart<C>
    {
        public C Config { get; }

        private GameObject _Go;
        private List<SkinnedMeshRenderer> _SkinnedMeshRenderers;
        private Transform[][] _Bones;
        
        public SkinnedMeshRendererPart(C config, GameObject gameObject)
        {
            _Go = gameObject;
            SkinnedMeshRenderer[] skinnedMeshRenderers = gameObject == null ? null : gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            Config = config;
            if (skinnedMeshRenderers == null)
            {
                return;
            }

            _SkinnedMeshRenderers = new List<SkinnedMeshRenderer>(skinnedMeshRenderers);
            _Bones = new Transform[_SkinnedMeshRenderers.Count][];
            for (int i = 0; i < _Bones.Length; i++)
            {
                var skinnedMeshRenderer = _SkinnedMeshRenderers[i];
                var bones = skinnedMeshRenderer.bones;
                _Bones[i] = new Transform[bones.Length];
            }
        }

        public void Apply(IAvatar<C> avatar)
        {
            if (_SkinnedMeshRenderers == null || avatar == null)
            {
                return;
            }

            var skeleton = avatar.GetSkeleton();
            if (skeleton == null)
            {
                return;
            }

            for (int i = 0; i < _SkinnedMeshRenderers.Count; i++)
            {
                var skinnedMeshRenderer = _SkinnedMeshRenderers[i];
                Transform[] bones = _Bones[i];
                bool hasBones = false;
                for (int j = 0; j < bones.Length; j++)
                {
                    bones[j] = skeleton.Find(skinnedMeshRenderer.bones[j].name);
                    if(bones[j] != null)
                    {
                        hasBones = true;
                    }
                    else
                    {
                        hasBones = false;
                        break;
                    }
                }
                if(hasBones)
                {
                    skinnedMeshRenderer.bones = bones;
                }
            }
        }

        public void Destroy()
        {
            if (_Go != null)
            {
                GameObject.Destroy(_Go);
                _Go = null;
            }
        }
    }
}
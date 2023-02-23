using System.Collections.Generic;
using UnityEngine;

namespace KLFramework.Avatar
{

    //******************************************
    // MultiSkinnedMeshRendererPart
    //
    // @Author: Kakashi
    // @Email: john.cha@qq.com
    // @Date: 2022-07-26 14:24
    //******************************************
    public class MultiSkinnedMeshRendererPart<C> : IAvatarPart<C>
    {
        public C Config { get; }
        
        private List<SkinnedMeshRenderer> _SkinnedMeshRenderers;
        private GameObject[] _GameObjects;
        
        private Transform[][] _Bones;
        
        public MultiSkinnedMeshRendererPart(C config, GameObject[] gameObjects)
        {
            Config = config;
            _GameObjects = gameObjects;
            if (gameObjects == null)
            {
                return;
            }
            _SkinnedMeshRenderers = _Find(gameObjects);          
            _Init();
        }

        private List<SkinnedMeshRenderer> _Find(GameObject[] gameObjects)
        {
            List<SkinnedMeshRenderer> list = new List<SkinnedMeshRenderer>();

            for (int i = 0; i < gameObjects.Length; i++)
            {
                var skinnedMeshRenderers = gameObjects[i].GetComponentsInChildren<SkinnedMeshRenderer>();
                if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0)
                {
                    continue;
                }
                list.AddRange(skinnedMeshRenderers);
            }

            return list;
        }

        private void _Init()
        {
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
                for (int j = 0; j < bones.Length; j++)
                {
                    bones[j] = skeleton.Find(skinnedMeshRenderer.bones[j].name);
                }
                skinnedMeshRenderer.bones = bones;
            }
        }

        public void Destroy()
        {
            if (_GameObjects != null)
            {
                for (int i = 0; i < _GameObjects.Length; i++)
                {
                    GameObject.Destroy(_GameObjects[i]);
                }

                _GameObjects = null;
            }
        }
    }
}
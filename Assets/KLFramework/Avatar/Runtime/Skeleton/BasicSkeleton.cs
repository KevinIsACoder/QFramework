using System.Collections.Generic;
using UnityEngine;

namespace KLFramework.Avatar
{

    //******************************************
    // BasicSkeleton
    //
    // @Author: Kakashi
    // @Email: john.cha@qq.com
    // @Date: 2022-07-25 17:10
    //******************************************
    public class BasicSkeleton : ISkeleton
    {
        private AvatarUtils.FlattenStrategy _FlattenStrategy;
        private Dictionary<string, Transform> _BoneName2Transforms = new Dictionary<string, Transform>();
        private Transform _Skeleton;
        
        public BasicSkeleton(Transform skeleton, AvatarUtils.FlattenStrategy flattenStrategy = AvatarUtils.FlattenStrategy.DoNotCheck)
        {
            _FlattenStrategy = flattenStrategy;
            if (skeleton == null)
            {
                return;
            }
            Set(skeleton);
        }

        public void Set(Transform skeleton)
        {
            _Skeleton = skeleton;
            _BoneName2Transforms.Clear();
            if(skeleton == null)
            {
                Debug.LogError("`skeleton` must not be null!");
                return;
            }

            AvatarUtils.Flatten(skeleton, _BoneName2Transforms, _FlattenStrategy);
        }

        public Transform Find(string boneName)
        {
            if (_BoneName2Transforms.TryGetValue(boneName, out Transform bone))
            {
                return bone;
            }
            Debug.Log($"Can not found bone {boneName}");
            return null;
        }

        public Transform GetSkeleton()
        {
            return _Skeleton;
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KLFramework.Avatar
{

    //******************************************
    // AvatarUtils
    //
    // @Author: Kakashi
    // @Email: john.cha@qq.com
    // @Date: 2022-07-25 16:50
    //******************************************
    public class AvatarUtils
    {
        public static void BindBone(Transform skeletonTransform, SkinnedMeshRenderer[] skinnedMeshRenderers)
        {
            if (skeletonTransform == null || skinnedMeshRenderers == null)
            {
                Debug.LogError($"`skeletonTransform` and `skinnedMeshRenderers` must not be null! skeletonTransform={skeletonTransform}, skinnedMeshRenderers={skinnedMeshRenderers}");
                return;
            }

            for (int rendererIndex = 0; rendererIndex < skinnedMeshRenderers.Length; rendererIndex++)
            {
                var skinnedMeshRenderer = skinnedMeshRenderers[rendererIndex];
                Transform[] bones = new Transform[skinnedMeshRenderer.bones.Length];
                for (int i = 0; i < bones.Length; i++)
                {
                    bones[i] = Find(skinnedMeshRenderer.bones[i], skeletonTransform);
                }
                skinnedMeshRenderer.bones = bones;
            }
        }
        
        public static Transform Find(Transform bone, Transform skeleton)
        {
            if (string.Equals(bone.name, skeleton.name))
            {
                return skeleton;
            }

            for (int i = 0; i < skeleton.childCount; i++)
            {
                var child = skeleton.GetChild(i);
                if (string.Equals(bone.name, child.name))
                {
                    return child;
                }

                var foundInChild = Find(bone, child);
                if (foundInChild != null)
                {
                    return foundInChild;
                }
            }
            return null;
        }

        public enum FlattenStrategy
        {
            DoNotCheck,
            LogErrorWhileSameBone,
            OverrideWhileSameBone,
            DoNothingWhileSameBone,
            ThrowExceptionWhileSameBone
        }

        public static void Flatten(Transform transform, Dictionary<string, Transform> childName2Transforms, FlattenStrategy flattenStrategy = FlattenStrategy.DoNotCheck)
        {
            switch (flattenStrategy)
            {
                case FlattenStrategy.DoNotCheck:
                case FlattenStrategy.ThrowExceptionWhileSameBone:
                    childName2Transforms.Add(transform.name, transform);
                    break;
                case FlattenStrategy.LogErrorWhileSameBone:
                    if (childName2Transforms.ContainsKey(transform.name))
                    {
                        Debug.LogError($"Duplicate bone name found! bone={transform.name}");
                    }
                    else
                    {
                        childName2Transforms.Add(transform.name, transform);
                    }
                    break;
                case FlattenStrategy.OverrideWhileSameBone:
                    if (childName2Transforms.ContainsKey(transform.name))
                    {
                        childName2Transforms[transform.name] = transform;
                    }
                    else
                    {
                        childName2Transforms.Add(transform.name, transform);
                    }
                    break;
                case FlattenStrategy.DoNothingWhileSameBone:
                    if (!childName2Transforms.ContainsKey(transform.name))
                    {
                        childName2Transforms.Add(transform.name, transform);
                    }
                    break;
                default:
                    throw new Exception($"Unsupported FlattenStrategy `{flattenStrategy}`");
            }
            
            for (int i = 0, childCount = transform.childCount; i < childCount; i++)
            {
                Flatten(transform.GetChild(i), childName2Transforms);
            }
        }
    }
}
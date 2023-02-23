using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;
[Serializable]
public class AvatarFaceData : MonoBehaviour
{
    [SerializeField]
    public Transform[] bones;
    [SerializeField]
    public SkinnedMeshRenderer blendSkinMesh;
    [SerializeField]
    public BlendFcData blendData;
    [SerializeField]
    public BoneFcData boneData;

    private Transform m_headTransform;
    private Transform m_skeletonTransform;
    public Transform FindBoneTransform(string path)
    {
        return m_skeletonTransform.Find(path);
    }

    public Transform FindBlendShapTransform(string path)
    {
        return m_headTransform.Find(path);
    }

    public void UpdateFaceData(string path)
    {
        var strArr = File.ReadAllLines(path);
        var boneData = JsonUtility.FromJson<BoneFcData>(strArr[0]);
        var blendData = JsonUtility.FromJson<BlendFcData>(strArr[1]);
        UpdateBoneData(boneData);
        UpdateBlendData(blendData);
    }

    public void UpdateBoneData(BoneFcData data)
    {
        boneData = data;
        if (boneData != null)
        {
            if (bones == null)
            {
                bones = new Transform[boneData.boneItemDatas.Length];
                for (int i = 0, count = boneData.boneItemDatas.Length; i < count; i++)
                {
                    if(!string.IsNullOrEmpty(boneData.boneItemDatas[i].path))
                    {
                        bones[i] = FindBoneTransform(boneData.boneItemDatas[i].path);
                    }
                }
            }

            for (int i = 0, count = boneData.boneItemDatas.Length; i < count; i++)
            {
                if(bones[i] != null)
                {
                    var itemData = boneData.boneItemDatas[i];
                    bones[i].localPosition = itemData.position;
                    bones[i].localRotation = Quaternion.Euler(itemData.rotation.x, itemData.rotation.y, itemData.rotation.z);
                    bones[i].localScale = itemData.scale;
                }
            }
        }
    }

    public void UpdateBlendData(BlendFcData data)
    {
        blendData = data;
        if (blendData != null)
        {
            if (blendSkinMesh == null)
            {
                blendSkinMesh = FindBlendShapTransform(blendData.path).gameObject.GetComponent<SkinnedMeshRenderer>();
            }

            var blendShapData = blendData.blendShapData;
            for (int i = 0; i < blendShapData.Length; i++)
            {
                var blendData = blendShapData[i];
                if (blendSkinMesh != null)
                {
                    if (!string.IsNullOrEmpty(blendData.blendName))
                    {
                        var blendIndex = blendSkinMesh.sharedMesh.GetBlendShapeIndex(blendData.blendName);
                        if (blendIndex >= 0)
                        {
                            blendSkinMesh.SetBlendShapeWeight(blendIndex, blendData.blendWeight);
                        }
                    }
                }
            }
        }
    }

    public void LoadFaceData(string data, Transform headTransform, Transform skeletonTrans)
    {
        m_headTransform = headTransform;
        m_skeletonTransform = skeletonTrans;
        if (!string.IsNullOrEmpty(data))
        {
            var datas = data.Split('|');
            var boneData = JsonUtility.FromJson<BoneFcData>(datas[0]);
            var blendData = JsonUtility.FromJson<BlendFcData>(datas[1]);
            UpdateBoneData(boneData);
            UpdateBlendData(blendData);
        }
    }
}

[Serializable]
public class BoneFcItem
{
    [SerializeField]
    public string path;
    [SerializeField]
    public Vector3 position;
    [SerializeField]
    public Vector3 rotation;
    [SerializeField]
    public Vector3 scale;
}

[Serializable]
public class BlendFcItemData
{
    [SerializeField]
    public string blendName;
    [SerializeField]
    public float blendWeight;
}

[Serializable]
public class BlendFcData
{
    [SerializeField]
    public string path;
    [SerializeField]
    public BlendFcItemData[] blendShapData;
}

[Serializable]
public class BoneFcData
{
    [SerializeField]
    public BoneFcItem[] boneItemDatas;
}

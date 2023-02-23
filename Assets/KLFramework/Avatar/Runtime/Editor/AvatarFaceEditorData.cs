#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
namespace KLFramework
{
    [Serializable]
    public class AvatarFaceEditorData : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField]
        public string editorDataPath = Environment.CurrentDirectory.Replace("\\", "/") + "/Assets/Raw/Game/AvatarX/FaceEditor/Face_edit_Data";
        [SerializeField]
        public Transform[] bones = new Transform[10];
        [SerializeField]
        public SkinnedMeshRenderer blendSkinMesh;
        [SerializeField]
        public BlendData blendData;
        [SerializeField]
        public BoneData boneData;
        [SerializeField]
        public string saveFaceDataPath = Environment.CurrentDirectory.Replace("\\", "/") + "/Assets/PersonalShow/AvatarX/";
        void Awake()
        {
            // UpdateBoneData();
        }

        public void OnBeforeSerialize() { }


        public void OnAfterDeserialize()
        {
        }

        public Transform FindTransform(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                var parent = FindParent(transform);
                if (parent != null && !string.IsNullOrEmpty(path))
                {
                    return parent.Find(path).transform;
                }
            }
            return null;
        }

        public Transform FindParent(Transform trans)
        {
            while (trans.parent != null)
            {
                trans = transform.parent;
            }
            return trans;
        }

        public void UpdateBoneData(BoneData data)
        {
            if (data != null) boneData = data;
            if (boneData != null)
            {
                var boneItems = boneData.boneItemDatas;
                for (int i = 0; i < boneItems.Length; i++)
                {
                    var item = boneItems[i];
                    var bone = bones[i];
                    var blendweight = item.blendWeight;
                    if (bone == null)
                    {
                        bone = FindTransform(item.path);
                        bones[i] = bone;
                    }
                    if (bone != null)
                    {
                        if (item.orginPosition == Vector3.zero)
                        {
                            item.orginPosition = bone.localPosition;
                        }
                        if (item.originRotation == Vector3.zero)
                        {
                            item.originRotation = bone.localRotation.eulerAngles;
                        }
                        if (item.originScale == Vector3.zero)
                        {
                            item.originScale = bone.localScale;
                        }


                        var newPosition = item.position;
                        var newRotation = item.rotation;
                        var newScale = item.scale;

                        bone.localPosition = item.orginPosition + new Vector3(newPosition.x * blendweight, newPosition.y * blendweight, newPosition.z * blendweight);
                        var rotation = item.originRotation;
                        bone.localRotation = Quaternion.Euler(rotation.x + newRotation.x * blendweight, rotation.y + newRotation.y * blendweight, rotation.z + newRotation.z * blendweight);
                        var scale = item.originScale;
                        bone.localScale = new Vector3(scale.x + newScale.x * blendweight, scale.y + newScale.y * blendweight, scale.z + newScale.z * blendweight);

                        item.path = GetTransformPath(bone);
                    }

                }
            }
        }

        public string GetTransformPath(Transform trans)
        {
            StringBuilder sb = new StringBuilder();
            while (trans.parent != null)
            {
                sb.Insert(0, trans.name);
                sb.Insert(0, "/");
                trans = trans.parent;
            }
            sb.Insert(0, trans.name);
            var path = sb.ToString();
            path = path.Substring(path.IndexOf("/") + 1);
            return path;
        }

        public void UpdateBlendData(BlendData data)
        {
            if (data != null) blendData = data;
            if (blendData != null)
            {
                if (blendSkinMesh == null && (!string.IsNullOrEmpty(blendData.path)))
                {
                    blendSkinMesh = FindTransform(blendData.path).gameObject.GetComponent<SkinnedMeshRenderer>();
                }
                var blendShapData = blendData.blendShapData;
                for (int i = 0; i < blendShapData.Length; i++)
                {
                    var blendItem = blendShapData[i];
                    if (blendSkinMesh != null)
                    {
                        if (!string.IsNullOrEmpty(blendItem.blendName))
                        {
                            var blendIndex = blendSkinMesh.sharedMesh.GetBlendShapeIndex(blendItem.blendName);
                            if (blendIndex >= 0)
                            {
                                blendSkinMesh.SetBlendShapeWeight(blendIndex, blendItem.blendWeight);
                            }
                        }
                    }
                }
                if (blendSkinMesh != null)
                {
                    blendData.path = GetTransformPath(blendSkinMesh.transform);
                }
            }
        }

        public void LoadFaceData(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                var datas = data.Split('|');
                var boneData = JsonUtility.FromJson<BoneData>(datas[0]);
                var blendData = JsonUtility.FromJson<BlendData>(datas[1]);
                UpdateBoneData(boneData);
                UpdateBlendData(blendData);
            }
        }

        private float fixed6(float value)
        {
            return (float)(Math.Floor(value * 1000000) / 1000000);
        }

        public void SaveFaceData()
        {
            var boneData = new BoneFcData();
            var boneFcItems = new List<BoneFcItem>();

            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i] != null)
                {
                    var item = new BoneFcItem();
                    var bonetrans = bones[i];
                    item.path = GetTransformPath(bonetrans);
                    item.position = bonetrans.localPosition;
                    item.rotation = bonetrans.localRotation.eulerAngles;
                    item.scale = bonetrans.localScale;
                    boneFcItems.Add(item);
                }
            }
            boneData.boneItemDatas = boneFcItems.ToArray();
            var jsonBoneData = JsonUtility.ToJson(boneData);

            ///变形器数据
            var blendFcData = new BlendFcData();
            if (blendSkinMesh != null)
            {
                blendFcData.path = GetTransformPath(blendSkinMesh.transform);
                blendFcData.blendShapData = new BlendFcItemData[blendData.blendShapData.Length];

                for (int i = 0; i < blendData.blendShapData.Length; i++)
                {
                    var data = blendData.blendShapData[i];
                    blendFcData.blendShapData[i] = new BlendFcItemData();
                    blendFcData.blendShapData[i].blendName = data.blendName;
                    blendFcData.blendShapData[i].blendWeight = data.blendWeight;
                }
            }
            var jsonBlendData = JsonUtility.ToJson(blendFcData);

            var str = new StringBuilder();
            str.Append(jsonBoneData).Append("|").Append(jsonBlendData);
            var dir = Path.GetDirectoryName(saveFaceDataPath);
            var fileName = Path.GetFileName(saveFaceDataPath);
            var path = EditorUtility.SaveFilePanel("保存捏脸数据", dir, fileName, "txt");
            File.WriteAllText(path, str.ToString());
        }
    }

    [Serializable]
    public class BoneItem
    {
        [SerializeField]
        public string path = "";
        [SerializeField]
        public Vector3 position;
        [SerializeField]
        public Vector3 rotation;
        [SerializeField]
        public Vector3 scale;
        [SerializeField]
        public Vector3 orginPosition;
        [SerializeField]
        public Vector3 originRotation;
        [SerializeField]
        public Vector3 originScale;
        [SerializeField]
        public float blendWeight;
    }

    [Serializable]
    public class BlendItemData
    {
        [SerializeField]
        public string blendName;
        [SerializeField]
        public float blendWeight;
    }

    [Serializable]
    public class BlendData
    {
        [SerializeField]
        public string path;
        [SerializeField]
        public BlendItemData[] blendShapData;
    }

    [Serializable]
    public class BoneData
    {
        [SerializeField]
        public BoneItem[] boneItemDatas;
    }
}
#endif
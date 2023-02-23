using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using XLua;

namespace AmongUs.Rendering
{
    /// <summary>
    /// 1.挂载到角色的根节点上
    /// 2.用角色的骨骼的根节点赋值boneRoot字段
    /// 3.调用Init()方法
    /// 4.调用Combine()方法
    /// </summary>
    [LuaCallCSharp]
    public class CombineAvatars : MonoBehaviour
    {
        public struct CombinedMeshData
        {
            public List<Vector3> vertices;
            public List<Vector2> uv;
            public List<Vector4> uvBounds;
            public List<int> indices;
            public List<Vector3> normals;
            public List<Vector4> tangents;
            public List<BoneWeight> boneWeights;
            public SkinnedMeshRenderer skinnedMeshRenderer;
            public List<SkinnedMeshRenderer> sourceSkinnedMeshRenderers;
            public Material material;
            public HashSet<Material> sourceMaterials;

            public List<Texture2D> mainTextures;
            public int totalCount;

            public Dictionary<string, List<float>> floats;
            public Dictionary<string, List<Cubemap>> cubemaps;
            public Dictionary<string, List<Vector4>> vectors;
            public Dictionary<string, List<Color>> colors;
            public Dictionary<string, List<Texture2D>> texture2Ds;
            public Dictionary<string, Texture2D> combinedTextures;
        }

        public Dictionary<Texture2D, int> indexInAtlas = new Dictionary<Texture2D, int>();
        public Dictionary<Texture2D, Rect> uvRects = new Dictionary<Texture2D, Rect>();
        public Dictionary<CombinedMeshData, Vector2Int> size = new Dictionary<CombinedMeshData, Vector2Int>();
        
        public Dictionary<Texture2D, Texture2D> sourceToCombined = new Dictionary<Texture2D, Texture2D>();
        public Dictionary<Texture2D, List<Material>> materialLut = new Dictionary<Texture2D, List<Material>>();

        public Transform boneRoot;
        public Dictionary<Shader, CombinedMeshData> combinedMeshDataLut = new Dictionary<Shader, CombinedMeshData>();
        public List<GameObject> blendShapeInstances = new List<GameObject>();

        private const string Combined = "Combined_";

        private Dictionary<Matrix4x4, Transform> bindPoseToBoneLut = new Dictionary<Matrix4x4, Transform>();
        private Dictionary<Matrix4x4, int> boneIndexLut = new Dictionary<Matrix4x4, int>();
        private Dictionary<string, Transform> allBonesLut = new Dictionary<string, Transform>();
        private Dictionary<Transform, Transform> childToBoneLut = new Dictionary<Transform, Transform>();
        
        private Dictionary<string, string> albedoPropertyNames = new Dictionary<string, string>
        {
            {"E3D/Actor/PBR-MaterialRGB-Normal", "_AlbedoMap"},
            {"E3D/Actor/E3D-ActorPBR-Rs-Face", "_AlbedoMap"},
            {"URP-Builtin/Character", "_BaseMap"},
        };
 
        [ContextMenu("Test")]
        private void Test()
        {
            Init();
            Combine(false, true);
        }

        private void ReqCacheBones(Transform bone)
        {
            for (int i = 0; i < bone.childCount; ++i)
            {
                var child = bone.GetChild(i);
                allBonesLut[child.name] = child;
                ReqCacheBones(child);
            }
        }

        public void Init()
        {
            allBonesLut.Clear();
            allBonesLut[boneRoot.name] = boneRoot;
            ReqCacheBones(boneRoot);
        }

        public void Combine(bool hideSkrs, bool resize = false, int width = 512, int height = 512)
        {
            var list = new List<SkinnedMeshRenderer>(GetComponentsInChildren<SkinnedMeshRenderer>());
            for (int i = 0; i < list.Count; ++i)
            {
                if (!list[i].enabled || !list[i].gameObject.activeSelf)
                {
                    list.RemoveAt(i--);
                }
            }

            Combine(list.ToArray(), hideSkrs, resize, width, height);
        }

        private void CombineMesh(CombinedMeshData combinedMeshData, Mesh sourceMesh, Rect uvRect)
        {
            List<Vector3> sourceVertices = new List<Vector3>();
            List<int> sourceIndices = new List<int>();
            List<Vector2> sourceUV = new List<Vector2>();
            List<Vector3> sourceNormals = new List<Vector3>();
            List<Vector4> sourceTangents = new List<Vector4>();
            List<BoneWeight> sourceBoneWeights = new List<BoneWeight>();

            sourceMesh.GetVertices(sourceVertices);
            sourceMesh.GetIndices(sourceIndices, 0);
            sourceMesh.GetUVs(0, sourceUV);
            sourceMesh.GetNormals(sourceNormals);
            sourceMesh.GetTangents(sourceTangents);
            sourceMesh.GetBoneWeights(sourceBoneWeights);
            //
            // sourceVertices = sourceVertices.GetRange(subMeshDescriptor.baseVertex + subMeshDescriptor.firstVertex,
            //     subMeshDescriptor.vertexCount);
            // sourceUV = sourceUV.GetRange(subMeshDescriptor.baseVertex + subMeshDescriptor.firstVertex,
            //     subMeshDescriptor.vertexCount);
            // sourceNormals = sourceNormals.GetRange(subMeshDescriptor.baseVertex + subMeshDescriptor.firstVertex,
            //     subMeshDescriptor.vertexCount);
            // sourceTangents = sourceTangents.GetRange(subMeshDescriptor.baseVertex + subMeshDescriptor.firstVertex,
            //     subMeshDescriptor.vertexCount);
            // sourceBoneWeights = sourceBoneWeights.GetRange(subMeshDescriptor.baseVertex + subMeshDescriptor.firstVertex,
            //     subMeshDescriptor.vertexCount);

            List<BoneWeight> bws = new List<BoneWeight>();

            for (int i = 0; i < sourceBoneWeights.Count; ++i)
            {
                var boneWeight = sourceBoneWeights[i];
                var bw = boneWeight;
                bw.boneIndex0 = boneIndexLut[sourceMesh.bindposes[boneWeight.boneIndex0]];
                bw.boneIndex1 = boneIndexLut[sourceMesh.bindposes[boneWeight.boneIndex1]];
                bw.boneIndex2 = boneIndexLut[sourceMesh.bindposes[boneWeight.boneIndex2]];
                bw.boneIndex3 = boneIndexLut[sourceMesh.bindposes[boneWeight.boneIndex3]];
                sourceBoneWeights[i] = bw;
            }

            bws.AddRange(sourceBoneWeights);

            for (int i = 0; i < sourceUV.Count; ++i)
            {
                var uv = sourceUV[i];
                uv = new Vector2(uv.x * uvRect.width + uvRect.x, uv.y * uvRect.height + uvRect.y);
                sourceUV[i] = uv;
            }

            var count = combinedMeshData.vertices.Count;
            for (int i = 0; i < sourceIndices.Count; ++i)
            {
                sourceIndices[i] = sourceIndices[i] + count;
            }

            combinedMeshData.vertices.AddRange(sourceVertices);
            combinedMeshData.indices.AddRange(sourceIndices);
            combinedMeshData.uv.AddRange(sourceUV);
            combinedMeshData.normals.AddRange(sourceNormals);
            combinedMeshData.tangents.AddRange(sourceTangents);
            combinedMeshData.boneWeights.AddRange(sourceBoneWeights);
        }

        [ContextMenu("Upload Parameters")]
        private void UploadParameters()
        {
            foreach (var combinedMeshData in combinedMeshDataLut)
            {
                if (combinedMeshData.Value.sourceSkinnedMeshRenderers.Count > 0)
                {
                    var material = combinedMeshData.Value.material;
                    material.EnableKeyword("COMBINED_AVATARS");
                    material.SetFloat("_TotalCombinedCount", combinedMeshData.Value.mainTextures.Count);
                    var s = size[combinedMeshData.Value];
                    material.SetVector("_CombinedUVSize", new Vector4(s.x, s.y));
                    material.SetVectorArray("_UVBounds", combinedMeshData.Value.uvBounds);
                    foreach (var color in combinedMeshData.Value.colors)
                    {
                        material.SetColorArray("_Array" + color.Key, color.Value);
                    }
                    foreach (var f in combinedMeshData.Value.floats)
                    {
                        material.SetFloatArray("_Array" + f.Key, f.Value);
                    }
                    foreach (var vector in combinedMeshData.Value.vectors)
                    {
                        material.SetVectorArray("_Array" + vector.Key, vector.Value);
                    }
                }
            }
        }
        
        /// <summary>
        /// 合并模型
        /// </summary>
        /// <param name="skinnedMeshRenderers">需要合并的SkinnedMeshRenderers</param>
        /// <param name="hideSkrs">隐藏合并前的SkinnedMeshRenderers</param>
        /// <param name="resize">是否重新设置合并后贴图的大小</param>
        /// <param name="newWidth">新的贴图的宽</param>
        /// <param name="newHeight">新的贴图的高</param>
        public void Combine(SkinnedMeshRenderer[] skinnedMeshRenderers, bool hideSkrs, bool resize, int newWidth, int newHeight)
        {
            blendShapeInstances.Clear();
            combinedMeshDataLut.Clear();
            uvRects.Clear();
            materialLut.Clear();
            sourceToCombined.Clear();

            foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
            {
                if (!skinnedMeshRenderer.enabled || !skinnedMeshRenderer.gameObject.activeInHierarchy ||
                    !skinnedMeshRenderer.gameObject.activeSelf)
                    continue;
                if (skinnedMeshRenderer.sharedMesh.blendShapeCount > 0) continue;
                var bindPoses = skinnedMeshRenderer.sharedMesh.bindposes;
                var bones = skinnedMeshRenderer.bones;
                for (int i = 0; i < bones.Length; ++i)
                {
                    var bindPose = bindPoses[i];
                    if (!bindPoseToBoneLut.ContainsKey(bindPose))
                    {
                        var bone = allBonesLut[bones[i].name];
                        bindPoseToBoneLut[bindPose] = bone;
                        boneIndexLut[bindPose] = boneIndexLut.Count;
                        childToBoneLut[bones[i]] = bone;
                    }
                }
            }

            Transform[] boneArray = new Transform[bindPoseToBoneLut.Count];
            Matrix4x4[] bindPoseArray = new Matrix4x4[bindPoseToBoneLut.Count];
            int index = 0;
            foreach (var bindPose in bindPoseToBoneLut)
            {
                boneArray[index] = bindPose.Value;
                bindPoseArray[index++] = bindPose.Key;
            }

            HashSet<Texture> uniqueTextures = new HashSet<Texture>();

            foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
            {
                if (skinnedMeshRenderer.sharedMesh.blendShapeCount > 0)
                {
                    var instance = Instantiate(skinnedMeshRenderer.gameObject);
                    var skrIns = instance.GetComponent<SkinnedMeshRenderer>();
                    skrIns.bones = new Transform[skinnedMeshRenderer.bones.Length];
                    for (int i = 0; i < skrIns.bones.Length; ++i)
                    {
                        skrIns.bones[i] = childToBoneLut[skinnedMeshRenderer.bones[i]];
                    }

                    blendShapeInstances.Add(instance);
                }

                var sharedMaterial = skinnedMeshRenderer.sharedMaterial;
                var shader = sharedMaterial.shader;
                CombinedMeshData combinedMeshData;

                if (!combinedMeshDataLut.TryGetValue(shader, out combinedMeshData))
                {
                    combinedMeshData = new CombinedMeshData();
                    var go = new GameObject(Combined + sharedMaterial.name);
                    go.transform.SetParent(transform);
                    var combinedSkr = go.AddComponent<SkinnedMeshRenderer>();
                    combinedSkr.sharedMaterial = sharedMaterial;

                    combinedMeshData.skinnedMeshRenderer = combinedSkr;
                    combinedMeshData.indices = new List<int>();
                    combinedMeshData.normals = new List<Vector3>();
                    combinedMeshData.vertices = new List<Vector3>();
                    combinedMeshData.uv = new List<Vector2>();
                    combinedMeshData.tangents = new List<Vector4>();
                    combinedMeshData.boneWeights = new List<BoneWeight>();
                    combinedMeshData.sourceSkinnedMeshRenderers = new List<SkinnedMeshRenderer>();
                    combinedMeshData.material = new Material(sharedMaterial);
                    combinedMeshData.floats = new Dictionary<string, List<float>>();
                    combinedMeshData.vectors = new Dictionary<string, List<Vector4>>();
                    combinedMeshData.colors = new Dictionary<string, List<Color>>();
                    combinedMeshData.cubemaps = new Dictionary<string, List<Cubemap>>();
                    combinedMeshData.sourceMaterials = new HashSet<Material>();
                    combinedMeshData.texture2Ds = new Dictionary<string, List<Texture2D>>();
                    combinedMeshData.combinedTextures = new Dictionary<string, Texture2D>();
                    combinedMeshData.mainTextures = new List<Texture2D>();
                    combinedMeshData.uvBounds = new List<Vector4>();

                    combinedMeshDataLut[shader] = combinedMeshData;
                }

                combinedMeshData.sourceSkinnedMeshRenderers.Add(skinnedMeshRenderer);
                if (combinedMeshData.sourceMaterials.Add(sharedMaterial))
                {
                    int texIndex = 0;
                    for (int j = 0; j < shader.GetPropertyCount(); ++j)
                    {
                        var propertyName = shader.GetPropertyName(j);
                        switch (shader.GetPropertyType(j))
                        {
                            case ShaderPropertyType.Texture:
                                var tex = sharedMaterial.GetTexture(propertyName);
                                if (propertyName.ToLower().Contains("matcap")) continue;;
                                
                                if (tex && uniqueTextures.Add(tex))
                                {
                                    switch (tex)
                                    {
                                        case Texture2D tex2D:
                                            List<Texture2D> texture2Ds;
                                            if (!combinedMeshData.texture2Ds.TryGetValue(propertyName, out texture2Ds))
                                            {
                                                texture2Ds = new List<Texture2D>();
                                                combinedMeshData.texture2Ds[propertyName] = texture2Ds;
                                            }

                                            var texName = albedoPropertyNames[shader.name];
                                            
                                            if (propertyName == texName)
                                            {
                                                combinedMeshData.mainTextures.Add(tex2D);
                                            }

                                            texture2Ds.Add(tex2D);
                                            List<Material> materials;
                                            if (!materialLut.TryGetValue(tex2D, out materials))
                                            {
                                                materials = new List<Material>();
                                                materialLut[tex2D] = materials;
                                            }

                                            materials.Add(sharedMaterial);
                                            break;
                                        case Cubemap cubemap:
                                            List<Cubemap> cubemaps;
                                            if (!combinedMeshData.cubemaps.TryGetValue(propertyName, out cubemaps))
                                            {
                                                cubemaps = new List<Cubemap>();
                                                combinedMeshData.cubemaps[propertyName] = cubemaps;
                                            }

                                            cubemaps.Add(cubemap);
                                            break;
                                    }
                                }

                                break;
                            case ShaderPropertyType.Float:
                            case ShaderPropertyType.Range:
                                List<float> floats;
                                if (!combinedMeshData.floats.TryGetValue(propertyName, out floats))
                                {
                                    floats = new List<float>();
                                    combinedMeshData.floats[propertyName] = floats;
                                }

                                floats.Add(sharedMaterial.GetFloat(propertyName));
                                break;
                            case ShaderPropertyType.Vector:
                                List<Vector4> vector4s;
                                if (!combinedMeshData.vectors.TryGetValue(propertyName, out vector4s))
                                {
                                    vector4s = new List<Vector4>();
                                    combinedMeshData.vectors[propertyName] = vector4s;
                                }

                                vector4s.Add(sharedMaterial.GetVector(propertyName));
                                break;
                            case ShaderPropertyType.Color:
                                List<Color> colors;
                                if (!combinedMeshData.colors.TryGetValue(propertyName, out colors))
                                {
                                    colors = new List<Color>();
                                    combinedMeshData.colors[propertyName] = colors;
                                }

                                colors.Add(sharedMaterial.GetColor(propertyName));
                                break;
                        }
                    }
                }
            }

            foreach (var combinedMeshData in combinedMeshDataLut)
            {
                foreach (var textures in combinedMeshData.Value.texture2Ds)
                {
                    var t = textures.Value[0];
                    textures.Value.RemoveAt(0);
                    textures.Value.Add(t);
                }
            }

            index = 0;
            foreach (var combinedMeshData in combinedMeshDataLut)
            {
                foreach (var texture2D in combinedMeshData.Value.texture2Ds)
                {
                    var propertyName = texture2D.Key;
                    var sourceTextures = texture2D.Value;

                    int width = 0;
                    var maxHeight = 0;
                    foreach (var sourceTexture in sourceTextures)
                    {
                        width += sourceTexture.width;
                        maxHeight = Mathf.Max(maxHeight, sourceTexture.height);
                        #if UNITY_EDITOR
                        UnityEditor.TextureImporter ti =
                            (UnityEditor.TextureImporter) UnityEditor.AssetImporter.GetAtPath(UnityEditor.AssetDatabase.GetAssetPath(sourceTexture));
                        ti.isReadable = true;
                        ti.SaveAndReimport();
                        #endif
                    }

                    while (width > 1024)
                    {
                        width /= 2;
                        maxHeight *= 2;
                    }

                    GraphicsFormat format = GraphicsFormat.R8G8B8A8_UNorm;
                    // switch (Application.platform)
                    // {
                    //     case RuntimePlatform.WindowsEditor:
                    //     case RuntimePlatform.WindowsPlayer:
                    //         format = GraphicsFormat.RGBA_DXT1_UNorm;
                    //         break;
                    //     case RuntimePlatform.Android:
                    //         format = GraphicsFormat.RGBA_ETC2_UNorm;
                    //         break;
                    //     case RuntimePlatform.IPhonePlayer:
                    //     case RuntimePlatform.OSXPlayer:
                    //     case RuntimePlatform.OSXEditor:
                    //         format = GraphicsFormat.RGBA_ASTC8X8_UNorm;
                    //         break;
                    // }
                    
                    var combined = new Texture2D(width, maxHeight, format, TextureCreationFlags.None);
                    combined.name = combinedMeshData.Value.material.shader.name + "___" + texture2D.Key;
                    combined.wrapMode = TextureWrapMode.Clamp;

                    var rects = combined.PackTextures(sourceTextures.ToArray(), 0);
                    if (combinedMeshData.Value.uvBounds.Count == 0)
                        foreach (var rect in rects)
                        {
                            combinedMeshData.Value.uvBounds.Add(new Vector4(rect.x, rect.y, rect.width, rect.height));
                        }
                    
                    var x = combined.width / sourceTextures[0].width;
                    var y = combined.height / sourceTextures[0].height;

                    if (resize)
                    {
                        var newCombined = new Texture2D(newWidth, newHeight, format, TextureCreationFlags.None);
                        Graphics.ConvertTexture(combined, newCombined);
                        combinedMeshData.Value.combinedTextures[propertyName] = newCombined;
                        combined.SafeRelease();
                        combined = newCombined;
                    }
                    else
                    {
                        combinedMeshData.Value.combinedTextures[propertyName] = combined;
                    }

                    for (int i = 0; i < sourceTextures.Count; ++i)
                    {
                        sourceToCombined[sourceTextures[i]] = combined;
                        List<Material> materials;
                        if (materialLut.TryGetValue(sourceTextures[i], out materials))
                        {
                            indexInAtlas[sourceTextures[i]] = i;
                            for (int j = 0; j < combinedMeshData.Value.mainTextures.Count; ++j)
                            {
                                if (combinedMeshData.Value.mainTextures[j] == sourceTextures[i])
                                {
                                    uvRects[sourceTextures[i]] = rects[i];
                                    size[combinedMeshData.Value] = new Vector2Int(x, y);
                                    break;
                                }
                            }
                        }
                    }

                    index++;
                    #if UNITY_EDITOR
                    foreach (var sourceTexture in sourceTextures)
                    {
                        UnityEditor.TextureImporter ti =
                            (UnityEditor.TextureImporter) UnityEditor.AssetImporter.GetAtPath(UnityEditor.AssetDatabase.GetAssetPath(sourceTexture));
                        ti.isReadable = false;
                        ti.SaveAndReimport();
                    }
                    #endif
                }
            }

            foreach (var meshRenderer in skinnedMeshRenderers)
            {
                var data = combinedMeshDataLut[meshRenderer.sharedMaterial.shader];
                var propName = albedoPropertyNames[meshRenderer.sharedMaterial.shader.name];
                Texture2D mainTex = meshRenderer.sharedMaterial.GetTexture(propName) as Texture2D;
                Rect uvRect;
                if (uvRects.TryGetValue(mainTex, out uvRect))
                {
                    CombineMesh(data, meshRenderer.sharedMesh, uvRect);
                }
            }


            foreach (var combinedMeshData in combinedMeshDataLut)
            {
                if (combinedMeshData.Value.sourceSkinnedMeshRenderers.Count > 0)
                {
                    if (hideSkrs)
                        foreach (var sourceSkinnedMeshRenderer in combinedMeshData.Value.sourceSkinnedMeshRenderers)
                        {
                            sourceSkinnedMeshRenderer.enabled = false;
                        }

                    var skr = combinedMeshData.Value.skinnedMeshRenderer;
                    var mesh = new Mesh();
                    var data = combinedMeshData.Value;
                    mesh.name = combinedMeshData.Value.skinnedMeshRenderer.name;
                    mesh.vertices = data.vertices.ToArray();
                    mesh.SetIndices(data.indices, MeshTopology.Triangles, 0);
                    mesh.uv = data.uv.ToArray();
                    mesh.normals = data.normals.ToArray();
                    mesh.tangents = data.tangents.ToArray();
                    mesh.boneWeights = data.boneWeights.ToArray();
                    mesh.bindposes = bindPoseArray;
                    skr.sharedMesh = mesh;
                    skr.bones = boneArray;
                    skr.rootBone = boneRoot;

                    var shader = combinedMeshData.Value.material.shader;
                    var material = combinedMeshData.Value.material;
                    for (int i = 0; i < shader.GetPropertyCount(); ++i)
                    {
                        var propName = shader.GetPropertyName(i);
                        var propType = shader.GetPropertyType(i);
                        switch (propType)
                        {
                            case ShaderPropertyType.Texture:
                                var tex = material.GetTexture(propName) as Texture2D;
                                int idx;
                                if (tex && indexInAtlas.TryGetValue(tex, out idx))
                                    material.SetFloat("_Index" + propName, idx);
                                
                                Texture2D texture2D;
                                if (combinedMeshData.Value.combinedTextures.TryGetValue(propName, out texture2D))
                                    material.SetTexture(propName, texture2D);
                                break;
                        }
                    }

                    material.name = skr.name;
                    skr.sharedMaterial = combinedMeshData.Value.material;
                    material.EnableKeyword("COMBINED_AVATARS");
                    material.SetFloat("_TotalCombinedCount", combinedMeshData.Value.mainTextures.Count);
                    var s = size[combinedMeshData.Value];
                    material.SetVector("_CombinedUVSize", new Vector4(s.x, s.y));
                    material.SetVectorArray("_UVBounds", combinedMeshData.Value.uvBounds);
                    foreach (var color in combinedMeshData.Value.colors)
                    {
                        material.SetColorArray("_Array" + color.Key, color.Value);
                    }
                    foreach (var f in combinedMeshData.Value.floats)
                    {
                        material.SetFloatArray("_Array" + f.Key, f.Value);
                    }
                    foreach (var vector in combinedMeshData.Value.vectors)
                    {
                        material.SetVectorArray("_Array" + vector.Key, vector.Value);
                    }
                }
            }
        }
    }
}
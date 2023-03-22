using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KLFramework.Avatar;
using Spark;
using System.IO;
using System.Text;
using System.Linq;
using XLua;
using AmongUs.Rendering;
public class ResourceType
{
    public const int Prefab = 1;
    public const int Texture = 2;
    public const int Color = 3;
}

public class GENDER
{
    public const int Boy = 1;
    public const int Girl = 2;
}

[LuaCallCSharp]
public class AvartaPlayer
{
    private IAvatar<int> m_avarta;
    public IAvatar<int> Avarta
    {
        get
        {
            return m_avarta;
        }
    }

    public IAvatarRule<int> m_avartaRule;
    private DelegateLifeCycle<int> m_lifeCycle;
    private Dictionary<int, MultiMaterialEffect> m_materialEffect = new Dictionary<int, MultiMaterialEffect>();
    private int m_gender = GENDER.Boy;
    public int Gender
    {
        get
        {
            return m_gender;
        }
    }
    private string skeletoName = "";

    private AvartaUserInfo m_avartaUserInfo;
    public AvartaUserInfo AvartaUserInfo
    {
        get
        {
            return m_avartaUserInfo;
        }
    }

    private Animator m_animator;
    public Animator Animator
    {
        get
        {
            return m_animator;
        }
    }

    private List<Color> skinColors = new List<Color>(); //肤色
    private bool m_isCombineAvatar = false; //是否需要合批
    
    public SpriteRenderer avatarBg;


    public AvartaPlayer(string data, bool isCombineAvatar = false, Transform parent = null)
    {
        m_isCombineAvatar = isCombineAvatar;
        m_lifeCycle = new DelegateLifeCycle<int>()
        .AddOnRemovePartHandler((avarta, part) =>
        {
            OnAvatarPartRemoved(avarta, part);
        })
        .AddOnAddPartHandler((avarta, part) => m_avarta.ApplyChange())
        .AddOnChangeSkeletonHandler((avarta, oldSkeleton, newSleleton) =>
        {
            m_avarta.ApplyChange();
        });

        m_avarta = new Avatar<int>(new BasicAvatar<int>(), AvatarConfig.GetBasicRule(), m_lifeCycle);
        if (parent != null)
        {
            m_avarta.GetTransform().SetParent(parent);
        }
        m_avarta.GetTransform().localScale = Vector3.one;
        m_avarta.GetTransform().localRotation = Quaternion.Euler(0, 180f, 0);

        //创建人物
        Init(data);
    }

    public void OnAvatarPartRemoved(IAvatar<int> avatar, IAvatarPart<int> part)
    {
        part.Destroy();
        m_avartaUserInfo.UpdateAvartaData(part.Config, null);
        m_materialEffect[part.Config].ClearMaterialArg();
        m_materialEffect.Remove(part.Config);
        Resources.UnloadUnusedAssets();
    }

    public GameObject GetAvatarObj()
    {
        if(m_avarta != null)
        {
            return m_avarta.GetGameObject();
        }
        return null;
    }
    
    //是否加载动画
    public Animator LoadAnimController(bool load, string path)
    {
        m_animator = m_avarta.GetAnimator();
        if(load)
        {
            if(!string.IsNullOrEmpty(path))
            {
                var animController = Spark.Assets.LoadAsset<RuntimeAnimatorController>(path);
                if(animController)
                {
                    m_animator.runtimeAnimatorController = animController;
                }
            }
            return m_animator;
        }
        else
        {
            return m_animator;
        }
    }

    public void Init(string data)
    {
        if (m_avartaUserInfo != null)
        {
            m_avartaUserInfo.InitData(data);
        }
        else
        {
            m_avartaUserInfo = new AvartaUserInfo(data);
        }        
        m_gender = m_avartaUserInfo.Gender;
        var avartaParent = m_avarta.GetTransform();
        var cfgData = m_avartaUserInfo.Gender == GENDER.Boy ? AvatarConfig.GetAvatarConstantCfg(AvatarConstantKey.m_SkeletonKey) : AvatarConfig.GetAvatarConstantCfg(AvatarConstantKey.f_SkeletonKey);
        var skeletonAsset = Spark.Assets.LoadAsset<GameObject>(cfgData.value);
        if (skeletonAsset != null)
        {
            var skeleton = GameObject.Instantiate(skeletonAsset);
            skeleton.name = m_avarta.GetAvatarId().ToString();
            skeleton.transform.SetParent(avartaParent, false);
            m_avarta.SetSkeleton(new BasicSkeleton((skeleton.transform)));
            AddHead();
            AddAvatarPart(m_avartaUserInfo);
            m_avarta.ApplyChange();

            //加载动画
            var animCfgData = m_avartaUserInfo.Gender == GENDER.Boy ? AvatarConfig.GetAvatarConstantCfg(AvatarConstantKey.m_animPathKey) : AvatarConfig.GetAvatarConstantCfg(AvatarConstantKey.f_animPathKey);
            if(animCfgData != null)
            {
                LoadAnimController(true, animCfgData.value);
            }

            if(m_isCombineAvatar)  //如果需要合并模型
            {
                CombineAvatar();
            }
        }
    }

    //只合并(衣服， 裤子， 鞋子)的
    public void CombineAvatar()
    {
        var skeletonObj = m_avarta.GetSkeleton().GetSkeleton();
        var avatarObj = m_avarta.GetGameObject();
        var combineAvatar = avatarObj.GetComponent<CombineAvatars>();
        if(combineAvatar == null) combineAvatar = avatarObj.AddComponent<CombineAvatars>();
        combineAvatar.boneRoot = skeletonObj.transform;
        combineAvatar.Init();
        var skinnedMeshRenderers = new List<SkinnedMeshRenderer>();
        
        GameObject tmpObj;
        if(m_materialEffect.ContainsKey(AvartaPartType.PART_TYPE_CLOTH))
        {
            tmpObj = m_materialEffect[AvartaPartType.PART_TYPE_CLOTH].GetMaterialObj();
            skinnedMeshRenderers.AddRange(tmpObj.GetComponentsInChildren<SkinnedMeshRenderer>());
            m_avarta.RemovePart(AvartaPartType.PART_TYPE_CLOTH);
        }

        if(m_materialEffect.ContainsKey(AvartaPartType.PART_TYPE_PANTS))
        {
            tmpObj = m_materialEffect[AvartaPartType.PART_TYPE_PANTS].GetMaterialObj();
            skinnedMeshRenderers.AddRange(tmpObj.GetComponentsInChildren<SkinnedMeshRenderer>());
            m_avarta.RemovePart(AvartaPartType.PART_TYPE_PANTS);
        }

        if(m_materialEffect.ContainsKey(AvartaPartType.PART_TYPE_SHOE))
        {
            tmpObj = m_materialEffect[AvartaPartType.PART_TYPE_SHOE].GetMaterialObj();
            skinnedMeshRenderers.AddRange(tmpObj.GetComponentsInChildren<SkinnedMeshRenderer>());
            m_avarta.RemovePart(AvartaPartType.PART_TYPE_SHOE);
        }

        combineAvatar.Combine(skinnedMeshRenderers.ToArray(), true, true, 512, 512);
    }

    public void AddAvatarPart(AvartaUserInfo m_avartaUserInfo)
    {
        for (int i = 0; i <  m_avartaUserInfo.AvartaData.Count; i++)
        {
            KeyValuePair<int, AvatarItem> item = m_avartaUserInfo.AvartaData.ElementAt(i);
            if (item.Value != null)
            {
                Render(item.Value);
            }
        }

        //检测有没有互相排斥的服装
        CheckMuteClothes();
       //设置肤色
       SetSkinColor();
    }

    public void Render(AvatarItem avartaData)
    {
        try
        {
            GameObject partObj = null;
            int partId = avartaData.id;
            var colors = new List<Color>();
            var texturesPath = new List<string>();
            foreach (var item in avartaData.resource_info)
            {
                var assetPath = item.value;
                if (!string.IsNullOrEmpty(item.value))
                {
                    assetPath = GetAssetPath(partId, assetPath);
                }
                else
                {
                    assetPath = GetDefaultAssetsPath(partId, m_gender);
                }

                if (item.type == ResourceType.Prefab)
                {
                    Transform parentTrans = null;
                    var hpName = AvatarConfig.GetAttributeName(partId, AvatarCfgAtrrType.HangPoint);
                    if (!string.IsNullOrEmpty(hpName))
                    {
                        string avatarName = m_avarta.GetGameObject().name;
                        var parentName = m_avarta.GetTransform().parent != null ? m_avarta.GetTransform().parent.name : "";
                        hpName = string.Format("{0}/{1}/{2}/{3}", parentName, avatarName, m_avarta.GetAvatarId().ToString(), hpName);
                        parentTrans = GameObject.Find(hpName).transform;
                    }
                    else
                    {
                        parentTrans = m_avarta.GetTransform();
                    }
                    assetPath = Path.ChangeExtension(assetPath, ".prefab");
                    var asset = Spark.Assets.LoadAsset<GameObject>(assetPath);
                    if (asset != null)
                    {
                        partObj = GameObject.Instantiate(asset);
                        partObj.transform.SetParent(parentTrans, false);
                        m_avarta.AddPart(partId, new SkinnedMeshRendererPart<int>(partId, partObj));
                        m_materialEffect[partId] = new MultiMaterialEffect(partObj);
                    }                          
                }
                else if (item.type == ResourceType.Color)
                {
                    Color color;
                    if (ColorUtility.TryParseHtmlString(item.value, out color))
                    {
                        colors.Add(color);
                    }
                }
                else if (item.type == ResourceType.Texture)
                {
                    assetPath = Path.ChangeExtension(assetPath, ".png");
                    texturesPath.Add(assetPath);
                }
            }

            //设置颜色
            if (colors.Count > 0)
            {
                if (partId == AvartaPartType.PART_TYPE_BACKGROUND)
                {
                    AvartaScene.SetColorBg(colors[0]);
                }
                else if (partId == AvartaPartType.PART_TYPE_SKIN)
                {
                    skinColors = colors;
                    SetSkinColor();
                }
                else
                {
                    SetMaterialColor(partId, colors);
                }
            }

            //设置图片
            if (texturesPath.Count > 0)
            {
                if (partId == AvartaPartType.PART_TYPE_BACKGROUND)
                {
                    var sprite = Spark.Assets.LoadAsset<Sprite>(texturesPath[0]);
                    AvartaScene.SetPictureBg(sprite);
                }
                else
                {
                    SetMaterialTexture(partId, texturesPath);
                }
            }

            //设置脸部数据
            if (avartaData.face_param != null)
            {
                var index = avartaData.face_param.LastIndexOf("/");
                var fileName = "";
                if (index > 0)
                {
                    fileName = avartaData.face_param.Substring(index + 1);
                }

                var cfgData = m_gender == GENDER.Boy ? AvatarConfig.GetAvatarConstantCfg(AvatarConstantKey.m_faceDataKey) : AvatarConfig.GetAvatarConstantCfg(AvatarConstantKey.f_faceDataKey);
                if (cfgData != null)
                {
                    fileName = Path.ChangeExtension((cfgData.value + fileName), ".txt");
                    var json_data = Spark.Assets.LoadAsset<TextAsset>(avartaData.face_param);
                    SetFaceData(partId, json_data.text);
                }
            }

            UpdateAvartaData(avartaData);
        }
        catch (System.Exception ex)
        {
            OnCreateAvatarFailed();
            Debug.LogErrorFormat("Avatar Renderer Error+++++++++++++: {0}", ex.ToString());
        }
    }

    //创建人物失败
    public void OnCreateAvatarFailed()
    {
        DestroyAvatar();
    }

    public bool isFacePart(int partType)
    {
        return partType == AvartaPartType.PART_TYPE_EYEBROW || partType == AvartaPartType.PART_TYPE_LASH || partType == AvartaPartType.PART_TYPE_SHADOW
        || partType == AvartaPartType.PART_TYPE_BEARD || partType == AvartaPartType.PART_TYPE_BLUSH || partType == AvartaPartType.PART_TYPE_FRECKLE
        || partType == AvartaPartType.PART_TYPE_MOUTHSHAP;
    }

    MultiMaterialEffect GetMaterialEffect(int partId)
    {
        if (isFacePart(partId))
        {
            return m_materialEffect[AvartaPartType.PART_TYPE_HEAD];
        }
        return m_materialEffect[partId];
    }

    public void SetFaceData(int partId, string data)
    {
        if (!string.IsNullOrEmpty(data))
        {
            var headTransform = m_materialEffect[AvartaPartType.PART_TYPE_HEAD].GetMaterialObj().transform;
            var shapeName = AvatarConfig.GetFaceShapName(partId);
            Transform partObj = headTransform.Find(shapeName);
            if(partObj == null)
            {
                partObj = new GameObject(shapeName).transform;
                partObj.SetParent(headTransform, false);
            }
            //设置blendShap值
            AvatarFaceData faceCom = partObj.gameObject.GetComponent<AvatarFaceData>();
            if (faceCom == null)
            {
                faceCom = partObj.gameObject.AddComponent<AvatarFaceData>();
            }
            faceCom.LoadFaceData(data, headTransform, m_avarta.GetSkeleton().GetSkeleton());
        }
    }

    public void SetMaterialTexture(int partId, List<string> texurePaths)
    {
        string[] materialNames = AvatarConfig.GetMateriaNames(partId, m_gender);
        string[] colorNames = AvatarConfig.GetTexPropertyNames(partId, m_gender);
        var mEffect = GetMaterialEffect(partId);
        for (int i = 0; i < texurePaths.Count; i++)
        {
            string materialName = null;
            if (materialNames != null && materialNames.Length > 0 && !string.IsNullOrEmpty(materialNames[i]))
            {
                materialName = materialNames[i];
            }

            if (colorNames != null && colorNames.Length > 0 && !string.IsNullOrEmpty(colorNames[i]))
            {
                var materialArg = new MaterialTextureArgs(colorNames[i]);
                var texture = Spark.Assets.LoadAsset<Texture>(texurePaths[i]);
                materialArg.Value = texture;
                mEffect.AddMaterialArg(partId, materialArg, materialName);
            }
        }
    }

    public void SetMaterialColor(int partId, List<Color> colors)
    {
        string[] materialNames = AvatarConfig.GetMateriaNames(partId, m_gender);
        string[] colorNames = AvatarConfig.GetColorPropertyNames(partId, m_gender);
        var mEffect = GetMaterialEffect(partId);
        for (int i = 0; i < colors.Count; i++)
        {
            string materialName = null;
            if (materialNames != null && materialNames.Length > 0 && !string.IsNullOrEmpty(materialNames[i]))
            {
                materialName = materialNames[i];
            }

            if (colorNames != null && colorNames.Length > 0 && !string.IsNullOrEmpty(colorNames[i]))
            {
                var materialArg = new MaterialColorArg(colorNames[i]);
                materialArg.Value = colors[i];
                mEffect.AddMaterialArg(partId, materialArg, materialName);
            }
        }
    }

    //合并mesh和贴图,只合并了身子的
    public void CombineMeshAndTexture()
    {
        var materials = new List<Material>();
        var skeleton = m_avarta.GetSkeleton();
        var avatarTrans = m_avarta.GetTransform();
        var meshes = avatarTrans.GetComponentsInChildren<SkinnedMeshRenderer>();
        List<CombineInstance> combineInstances = new List<CombineInstance>();
        var bones = new List<Transform>();
        var matrix = m_avarta.GetSkeleton().GetSkeleton().localToWorldMatrix;
        for (int i = 0; i < meshes.Length; i++)
        {
            SkinnedMeshRenderer mesh = meshes[i];
            if(mesh.gameObject.name == "base_face" || mesh.gameObject.name == "base_eye_l" || mesh.gameObject.name == "base_eye_r")
            {
                continue;
            }

            materials.AddRange(mesh.sharedMaterials);
            //collect
            for (int j = 0; j < mesh.sharedMesh.subMeshCount; j++)
            {
                CombineInstance cb = new CombineInstance();
                cb.mesh = mesh.sharedMesh;
                cb.transform = mesh.localToWorldMatrix * matrix;
                cb.subMeshIndex = j;
                cb.mesh.triangles = mesh.sharedMesh.triangles;
                combineInstances.Add(cb);
            }

            for (int boneIndex = 0; boneIndex < mesh.bones.Length; ++boneIndex)
            {
                var transforms = m_avarta.GetSkeleton().GetSkeleton().GetComponentsInChildren<Transform>();
                for (int transformIndex = 0; transformIndex < transforms.Length; transformIndex++)
                {
                    if(transforms[transformIndex].name.Equals(mesh.bones[boneIndex].name))
                    {
                        bones.Add(transforms[transformIndex]);
                        break;
                    }
                }
            }
        }

        var skeletonObj = skeleton.GetSkeleton().gameObject;
        var skinMesh = skeletonObj.GetComponent<SkinnedMeshRenderer>();
        if (skinMesh)
        {
            GameObject.Destroy(skinMesh);
        }
        var newMesh = skeletonObj.AddComponent<SkinnedMeshRenderer>();
        newMesh.sharedMesh = new Mesh();
        newMesh.sharedMesh.CombineMeshes(combineInstances.ToArray(), true, false);
        newMesh.bones = bones.ToArray();
        newMesh.materials = materials.ToArray();
    }

    //设置肤色
    public void SetSkinColor()
    {
        List<int> partIds = new List<int>();
        List<Color> colors = skinColors;
        if (colors != null)
        {
            partIds.Add(AvartaPartType.PART_TYPE_HEAD);
            if (m_materialEffect.ContainsKey(AvartaPartType.PART_TYPE_SUIT))
            {
                partIds.Add(AvartaPartType.PART_TYPE_SUIT);
            }
            else
            {
                partIds.Add(AvartaPartType.PART_TYPE_CLOTH);
                partIds.Add(AvartaPartType.PART_TYPE_PANTS);
            }

            var cfgData = AvatarConfig.GetAvatarMaterialCfg(AvartaPartType.PART_TYPE_SKIN);
            string[] colorArgs = m_gender == GENDER.Boy ? cfgData.m_colornames : cfgData.f_colornames;
            for (int i = 0; i < partIds.Count; i++)
            {
                for (int j = 0; j < colors.Count; ++j)
                {
                    if(m_materialEffect.ContainsKey(partIds[i]))
                    {
                        var propertyName = colorArgs[j];
                        var materialColorArg = new MaterialColorArg(propertyName);
                        materialColorArg.Value = colors[j];
                        m_materialEffect[partIds[i]].AddMaterialArg(partIds[i], materialColorArg);
                    }
                }
            }
        }
    }

    public void CheckMuteClothes()
    {
        bool hasMuteCloth = false;
        List<int> mutePartIds = new List<int>();
        int[] partIds = null;
        if(m_avartaUserInfo != null && m_avartaUserInfo.GetAvatarData(AvartaPartType.PART_TYPE_SUIT) == null)
        {
            var partCfg = AvatarConfig.GetAvatarPartCfg(AvartaPartType.PART_TYPE_SUIT);
            partIds = partCfg.muteparts;
        }

        if(partIds != null)
        {
            for (int i = 0; i < partIds.Length; i++)
            {
                if (m_avartaUserInfo.GetAvatarData(partIds[i]) == null)
                {
                    hasMuteCloth = true;
                    mutePartIds.Add(partIds[i]);
                }
            }
        }

        if (hasMuteCloth)
        {
            for (int i = 0; i < mutePartIds.Count; ++i)
            {
                var item = GetDefaultAvatarItem(mutePartIds[i]);
                if (item != null)
                {
                    Render(item);
                }
            }
        }
    }

    ///获取初始形象数据
    private AvatarItem GetDefaultAvatarItem(int partId)
    {
        var avatarData = AvartaScene.GetDefaultData(m_gender);
        if (avatarData != null)
        {
            foreach (var item in avatarData)
            {
                if (item.id == partId)
                {
                    foreach (var avatarItem in item.part_list)
                    {
                        if (avatarItem.id == partId)
                        {
                            return avatarItem;
                        }
                    }
                }
            }
        }
        return null;
    }

    public void UpdateAvartaData(AvatarItem avartaData)
    {
        m_avartaUserInfo.UpdateAvartaData(avartaData.id, avartaData);
    }

    //加载不到资源，加载默认资源
    public string GetDefaultAssetsPath(int partType, int gender)
    {
        var cfgDataData = AvatarConfig.GetAvatarPartCfg(partType);
        if (cfgDataData != null)
        {
            return gender == GENDER.Boy ? cfgDataData.m_defaultres : cfgDataData.f_defaultres;
        }
        return "";
    }
    //添加头
    public void AddHead()
    {
        var partType = AvartaPartType.PART_TYPE_HEAD;
        string path = m_gender == GENDER.Boy ? "Game/PersonalShow/M_Head/AMHD01_L1.prefab" : "Game/PersonalShow/F_Head/AFHD01_L1.prefab";
        var asset = Spark.Assets.LoadAsset<GameObject>(path);
        var partObj = GameObject.Instantiate(asset);
        partObj.transform.SetParent(m_avarta.GetTransform(), false);
        m_avarta.AddPart(partType, new SkinnedMeshRendererPart<int>(partType, partObj));
        m_materialEffect[partType] = new MultiMaterialEffect(partObj);
    }

    public string GetAssetPath(int partType, string name)
    {
        var cfgData = AvatarConfig.GetAvatarPartCfg(partType);
        StringBuilder path = m_gender == GENDER.Boy ? new StringBuilder(cfgData.m_respath) : new StringBuilder(cfgData.f_respath);
        var index = name.LastIndexOf("/");
        if (index > 0)
        {
            name = Path.GetFileNameWithoutExtension(name.Substring(index + 1));
        }

        if (name.Contains("_L1") || name.Contains("_L2"))  //是不是有精度
        {
            var tmpName = name.Split('_')[0];
            path = path.Append(tmpName).Append("/");
            path = path.Append(name).Append(".awb");
        }
        else if(partType == AvartaPartType.PART_TYPE_BACKGROUND)
        {
            path = path.Append(name).Append(".awb");
        }
        else
        {
            path = path.Append(name).Append("/").Append(name).Append(".awb");
        }
        return path.ToString();
    }

    public void DestroyAvatar()
    {
        if(m_avarta != null)
        {
            m_avarta.Destroy();
            m_avarta = null;
        }
        m_materialEffect.Clear();
        m_avartaUserInfo = null;
    }
}

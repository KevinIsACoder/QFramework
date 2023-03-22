using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KLFramework.Avatar;
using XLua;
public static class AvatarConfig
{
    public static BasicAvatarRule<int> basicRule;
    private static List<AvatarPartCfg> avatarPartCfgs;
    private static List<AvatarItemCfg> avatarItemCfgs;
    private static List<AvatarConstantCfg> avatarConstantCfgs;
    private static List<AvatarMaterialCfg> avatarMaterialCfgs;
    private static List<AvatarCameraCfg> avatarCameraCfgs;

    public const int design_ResolutionX = 720;
    public const int design_ResolutionY = 1280;
    static AvatarConfig()
    {
        var objects = SparkLua.Env.DoString("return require('Game/Common/Config/AvatarPartCfg'), require('Game/Common/Config/AvatarItemCfg'), require('Game/Common/Config/AvatarConstantCfg'), require('Game/Common/Config/AvatarMaterialCfg'), require('Game/Common/Config/AvatarCameraCfg')");
        avatarPartCfgs = ((LuaTable)objects[0]).Cast<List<AvatarPartCfg>>();
        avatarItemCfgs = ((LuaTable)objects[1]).Cast<List<AvatarItemCfg>>();
        avatarConstantCfgs = ((LuaTable)objects[2]).Cast<List<AvatarConstantCfg>>();
        avatarMaterialCfgs = ((LuaTable)objects[3]).Cast<List<AvatarMaterialCfg>>();
        avatarCameraCfgs = ((LuaTable)objects[4]).Cast<List<AvatarCameraCfg>>();
    }

    public static AvatarPartCfg GetAvatarPartCfg(int partId)
    {
        if(avatarPartCfgs == null) return null;
        return avatarPartCfgs.Find((AvatarPartCfg cfg) => {return cfg.id == partId;});
    }

    public static List<AvatarPartCfg> GetAllAvatarPartCfgs()
    {
        return avatarPartCfgs;
    }
    
    public static AvatarItemCfg GetAvatarItemCfg(int partId, int attrType)
    {
        if(avatarItemCfgs == null) return null;
        return avatarItemCfgs.Find((AvatarItemCfg cfg) => {return cfg.part_id == partId && cfg.atrr_type == attrType;});
    }

    public static AvatarConstantCfg GetAvatarConstantCfg(string key)
    {
        if(avatarConstantCfgs == null) return null;
        return avatarConstantCfgs.Find((AvatarConstantCfg cfg) => {return cfg.key == key;});
    }

    public static AvatarMaterialCfg GetAvatarMaterialCfg(int partId)
    {
        if(avatarMaterialCfgs == null) return null;
        return avatarMaterialCfgs.Find((AvatarMaterialCfg cfg) => {return cfg.part_id == partId; });
    }

    public static AvatarCameraCfg GetAvatarCameraCfg(int cameraId)
    {
        if(avatarCameraCfgs == null) return null;
        return avatarCameraCfgs.Find((AvatarCameraCfg cfg) => {return cfg.cameraId == cameraId; });
    }

    public static BasicAvatarRule<int> GetBasicRule()
    {
        if (basicRule == null)
        {
            basicRule = new BasicAvatarRule<int>();
            var avatarCfgs = GetAllAvatarPartCfgs();
            foreach (var data in avatarCfgs)
            {
                basicRule.AddMute(data.id, data.muteparts);
            }
            return basicRule;
        }
        return basicRule;
    }

    public static string GetAttributeName(int partId, int attrType)
    {
        var data = AvatarConfig.GetAvatarItemCfg(partId, attrType);
        if(data != null)
        {
            return (string)data.atrr_value;
        }
        return "";
    }

    public static string[] GetMateriaNames(int partId, int gender)
    {
        var cfgData = GetAvatarMaterialCfg(partId);
        if(cfgData != null)
        {
            return gender == GENDER.Boy ? cfgData.m_materialnames : cfgData.f_materialnames;
        }
        return null;
    }

    public static string GetFaceShapName(int partId)
    {
        if (partId == AvartaPartType.PART_TYPE_HEAD)
        {
            return "Face_Shape";
        }
        else if (partId == AvartaPartType.PART_TYPE_EYE)
        {
            return "Eye_Shape";
        }
        else if (partId == AvartaPartType.PART_TYPE_NOSE)
        {
            return "Nose_Shape";
        }
        else if (partId == AvartaPartType.PART_TYPE_MOUTHSHAP)
        {
            return "Mouth_Shape";
        }
        return "";
    }

    public static string[] GetTexPropertyNames(int partId, int gender)
    {
        var cfgData = GetAvatarMaterialCfg(partId);
        if(cfgData != null)
        {
            return gender == GENDER.Boy ? cfgData.m_texturenames : cfgData.f_texturenames;
        }
        return null;
    }

    public static string[] GetColorPropertyNames(int partId, int gender)
    {
        var cfgData = GetAvatarMaterialCfg(partId);
        if(cfgData != null)
        {
            return gender == GENDER.Boy ? cfgData.m_colornames : cfgData.f_colornames;
        }
        return null;
    }
}

public class ModelQuality
{
    public const int High = 1;
    public const int Low = 2;
}

public class AvatarCfgAtrrType
{
    public const int HangPoint = 1; //部位挂点
    public const int AnimName = 2; //换装后动画
}

public class AvatarConstantKey
{
    public const string m_SkeletonKey = "male_skeleton";
    public const string f_SkeletonKey = "female_skeleton";
    public const string m_faceDataKey = "m_facedatapath";
    public const string f_faceDataKey= "f_facedatapath";
    public const string m_animPathKey = "m_animpath";
    public const string f_animPathKey = "f_animpath";
}

public class AvatarCameraId
{
    public const int DefaultCameraId = 1; //默认全身
    public const int HalfBodyCameraId = 2; //半身
    public const int FaceCameraId = 3; //脸部
    public const int DressCameraId = 4; //半屏全身
}

[CSharpCallLua]
public class AvatarPartCfg
{
    public int id; //部件Id
    public string name; //部位名字
    public int[] muteparts; //互斥部位(不能同时存在)
    public string m_respath; //男资源路径
    public string f_respath; //女资源路径
    public string m_defaultres; //男默认资源
    public string f_defaultres; //女默认资源
}

[CSharpCallLua]
public class AvatarItemCfg
{
    public int id;
    public int part_id; //部位Id
    public int atrr_type; //参数类型
    public object atrr_value; //参数值
}

[CSharpCallLua]
public class AvatarConstantCfg
{
    public int id;
    public string key;
    public string value;
}

[CSharpCallLua]
public class AvatarMaterialCfg
{
    public int id;
    public int part_id;
    public string name;
    public string[] m_materialnames;
    public string[] f_materialnames;
    public string[] m_colornames;
    public string[] f_colornames;
    public string[] m_texturenames;
    public string[] f_texturenames;
}

[CSharpCallLua]
public class AvatarCameraCfg
{
    public int cameraId;
    public float[] position;
    public float[] rotation;
    public float fov;
    public int enablemove; //是否可移动 0 不可 1 可以
    public int enablescale; //是否可缩放 0 不可 1 可以
    public float[] movebound; //相机视角下人物移动范围
    public float[] scalebound; //相机视角下人物缩放范围
}

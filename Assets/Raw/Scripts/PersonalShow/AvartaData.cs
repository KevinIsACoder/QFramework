using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class AvartaData
{
    public List<AvatarItem> part_list;
    public int id; //换装类型Id
    
}

[Serializable]
public class AvatarItemParam
{
}

[Serializable]
public class AvartaFaceData
{
    public string faceData;
}

[Serializable]
public class AvatarItem
{
    public int id; //二级换装类型Id(如脸部下面的子部位，眼睛，眉毛等)
    public List<AvatarItemParam> res_param; //各部分换装的影响参数
    public string face_param;
    public List<ResourceInfo> resource_info; //资源类型 key：1 资源包 2 图片 3 色值  value:对应的值
}

[Serializable]
public class ResourceInfo
{
    public int type;
    public string value;
}

[Serializable]
public class AvartaItemInfo
{
    public int id;
    public List<AvatarItemParam> res_param;

    public bool is_selected;
}

[Serializable]
public class AvartaUserDatas
{
    public int gender = 1; //性别
    public string show_page; //展示页面
    public List<AvartaData> init_params;
    public float showheight; //弹窗高度
    public List<AvartaData> origin_male; //男性初始数据
    public List<AvartaData> origin_female; //女性初始数据
}

[Serializable]
public class CameraParam
{
    public float pos;
    public float fileofview;
    public int cameraId;
}

[Serializable]
public class AvartaCameraData
{
    public CameraParam camera_params;
    public int full_screen;
}

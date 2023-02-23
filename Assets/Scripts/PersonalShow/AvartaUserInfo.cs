using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using XLua;
[LuaCallCSharp]
public class AvartaUserInfo
{
    //private List<AvartaData> m_avartaDatas;

    private Dictionary<int, AvatarItem> m_avartaDatas = new Dictionary<int, AvatarItem>();
    public Dictionary<int, AvatarItem> AvartaData
    {
        get
        {
            return m_avartaDatas;
        }
    }
    private int m_gender = 1; //性别
    public int Gender
    {
        get
        {
            return m_gender;
        }
    }

    private string m_showpage = AvartaShowPage.PROFILE; //展示界面
    public string ShowPage
    {
        get
        {
            return m_showpage;
        }
    }
    
    public AvartaUserInfo(string data)
    {
        InitData(data);
    }

    public void InitData(string data)
    {
        m_avartaDatas.Clear();
        if (data != null)
        {
            var usrdata = JsonUtility.FromJson<AvartaUserDatas>(data);
            m_gender = usrdata.gender;
            m_showpage = usrdata.show_page;
            foreach (var item in usrdata.init_params)
            {
                foreach (var subItem in item.part_list)
                {
                    m_avartaDatas[subItem.id] = subItem;   
                }
            }
        }
    }

    public void UpdateAvartaData(int res_id, AvatarItem data)
    {
        m_avartaDatas[res_id] = data;
    }
    
    public AvatarItem GetAvatarData(int res_id)
    {
        if(m_avartaDatas.ContainsKey(res_id))
        {
           return m_avartaDatas[res_id];
        }
        return null;
    }

    public void ResetAvartaData(string data)
    {
        InitData(data);
    }
}

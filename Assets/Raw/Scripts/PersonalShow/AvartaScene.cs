using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using XLua;
using Spark;
using System;
[LuaCallCSharp]
public class AvartaScene
{
    private static string m_SceneName;

    private static AvartaSceneController sceneController;
    public static AvartaSceneController SceneController
    {
        get
        {
            return sceneController;
        }
    }
    private static List<AvartaData> m_originMaleData;  //客户端下发的男性初始数据
    private static List<AvartaData> m_originFamaleData; //客户端下发的女性初始数据
    private static bool isSceneLoad = false;

    public static void Enter(string sceneName, string data, Action callback = null)
    {

        var usrdata = JsonUtility.FromJson<AvartaUserDatas>(data);
        if(usrdata.show_page == AvartaShowPage.VIRTUAL_CHARACTOR_EDIT)
        {
            SetDefaultAvatarData(usrdata);
        }
        if(sceneController != null)
        {
            sceneController.Init(data);
        }
        else
        {
            if(isSceneLoad == false)
            {
                isSceneLoad = true;
                LoadAvatarScene(sceneName, data, callback);
            }
        }
    }

    //设置初始形象数据（默认数据用客户端下发的）
    private static void SetDefaultAvatarData(AvartaUserDatas data)
    {
        if(m_originMaleData == null)
        {
            m_originMaleData = data.origin_male;
        }
        if(m_originFamaleData == null)
        {
            m_originFamaleData = data.origin_female;
        }
    }

    public static List<AvartaData> GetDefaultData(int gender)
    {
        if(gender == GENDER.Boy)
        {
            return m_originMaleData;
        }
        else
        {
            return m_originFamaleData;
        }
    }

    private static void LoadAvatarScene(string sceneName, string data, Action callback = null)
    {
        m_SceneName = sceneName;
        SceneHelper.LoadSceneAsync(m_SceneName, true , () =>
        {   
            var obj = GameObject.Find("SceneController");
            sceneController = obj.GetComponent<AvartaSceneController>();
            sceneController.Init(data);
            if(callback != null)
            {
                Debug.Log("LoadAvatarScene callBack++++++++++");
                callback();
            }
        });
    }

    public static void Exit(Action callback = null)
    {
        if (SceneHelper.IsSceneLoaded("PersonalShow"))
        {
            //退出场景,清除数据
            Clear();
            SceneHelper.UnloadScene("PersonalShow");
            Debug.Log("UnLoadScene CallBack++++++++++++++++ ");
            if(callback != null)
            {
                callback();
            }
        }
    }

    public static void SetColorBg(Color color)
    {
        if(sceneController)
        {
            sceneController.SetColorBg(color);
        }
    }

    public static void SetPictureBg(Sprite sprite)
    {
        if(sceneController)
        {
            sceneController.SetPictureBg(sprite);
        }
    }
    public static void Clear()
    {
        Debug.Log("Exit Avatar Scene++++++++++ " + m_SceneName);
        isSceneLoad = false;
        if(sceneController != null)
        {
            sceneController.Exit();
            sceneController = null;
        }
    }
}

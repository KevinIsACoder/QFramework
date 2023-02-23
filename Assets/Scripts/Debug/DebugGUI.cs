using System.Collections;
using UnityEngine;

public class DebugGUI
{
#if UNITY_EDITOR
    static string ip = "http://129.226.37.173:9200";
    static string gameName = "DrawGuess;200";
    static string uid = "111111";
    static string room = "12345";
    static string userList = "111111,222222";
    static string gameConfig = "[{\"field\":\"subType\",\"value\":102}]";

    static GUIStyle style_label = new GUIStyle();
    static GUIStyle style_input = new GUIStyle();
    static float base_y = 10f;
    static int fontSize = 20;
    static bool isHost = false;

    static bool isShow = true;

    static DebugGUI() {
        style_label.fontSize = fontSize;
        style_label.normal.textColor = new Color(255f, 255f, 255f);
        GUI.skin.textField.fontSize= fontSize;
        GUI.skin.textArea.fontSize = fontSize;
        GUI.skin.button.fontSize = fontSize;
        GUI.skin.toggle.fontSize = fontSize;

        //style_input.fontSize = fontSize;
        ReadOld();

    }
    public static void ShowGUI()
    {
        if (!isShow) {
            // 显示按钮
            if (GUI.Button(new Rect(10, 10, 80, 30), "Show"))
            {
                isShow = true;
                Debug.Log("Show DebugWindow " + isShow);
            }
            return;
        }
        GUI.Window(0, new Rect(10, 10, Screen.width - 20, 350), (int i)=>{ }, "Debug Window");
        ip = NewInput("服务器", ip, 20, 30, 30);
        gameName = NewInput("游戏 ",gameName, 20, 80, 20);
        uid = NewInput("UID",uid,20,130,6);
        room = NewInput("房间号", room, 20, 180, 8);
        userList = NewInput("用户列表", userList,300,130,20,70);

        gameConfig = NewInput("玩法类型", gameConfig,300,200,20,70);

        // 初始化按钮
        if (GUI.Button(new Rect(10, 300, 100, 30), "InitGame"))
        {
            Debug.Log("InitGame！");
            FakeSocket.Get().SetConf(ip,room,uid,gameName,userList,gameConfig, isHost);
            FakeSocket.Get().Start();
            FakeSocket.Get().InitGame();
            Save();
        }
        // 设置语音麦席位
        if (GUI.Button(new Rect(150, 300, 100, 30), "SetSeat"))
        {
            Debug.Log("SetSeat！");
            FakeSocket.Get().SetSeat();
        }
        // 清空页面
        if (GUI.Button(new Rect(300, 300, 100, 30), "CloseAll"))
        {
            Debug.Log("CloseAll！");
            SparkLua.Env.DoString("GameCenter.CloseAll()");
            //SparkLua.Env.DoString("App.uiRoot:CloseAll(true)");
            SparkLua.Env.DoString("for key,_ in pairs(package.preload) do package.preload[key] = nil end");
            SparkLua.Env.DoString("for key,_ in pairs(package.loaded) do package.loaded[key] = nil end");
            //Spark.Assets.UnloadUnusedAssets();
        }
        // 重置房间
        if(GUI.Button(new Rect(450, 300, 120, 30), "ClearRoom"))
        {
            Debug.Log("ClearRoom！");
            FakeSocket.Get().ClearRoom();
        }
        // 隐藏按钮
        if (GUI.Button(new Rect(Screen.width - 100, 300, 80, 30), "Hide"))
        {
            isShow = false;
            Debug.Log("Show DebugWindow " + isShow);
        }
        isHost= GUI.Toggle(new Rect(20, 250, 100, 30), isHost, "是否房主");


    }

    public static Rect RectInWindow(float x, float y, float width, float height)
    {
        return new Rect(x, y + base_y, width, height);
    }

    public static string NewInput(string name,string value,float x,float y,float length) {
        return NewInput(name, value, x, y, length, 30);
    }

    public static string NewInput(string name, string value, float x, float y, float length,float height)
    {
        GUI.Label(RectInWindow(x, y, fontSize * name.Length, 30), name, style_label);
        return GUI.TextArea(RectInWindow(x + fontSize * name.Length + 20, y - 5, length * 15, height), value);
    }

    private static void Save() {
        PlayerPrefs.SetString("Debug_ip",ip);
        PlayerPrefs.SetString("Debug_room", room);
        PlayerPrefs.SetString("Debug_uid", uid);
        PlayerPrefs.SetString("Debug_gameName", gameName);
        PlayerPrefs.SetString("Debug_userList", userList);
        PlayerPrefs.SetString("Debug_gameConfig", gameConfig);
        
        PlayerPrefs.SetInt("Debug_isHost", isHost?1:0);
    }

    private static void ReadOld() {
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("Debug_ip"))) {
            ip = PlayerPrefs.GetString("Debug_ip");
        }
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("Debug_room")))
        {
            room = PlayerPrefs.GetString("Debug_room");
        }
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("Debug_uid")))
        {
            uid = PlayerPrefs.GetString("Debug_uid");
        }
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("Debug_gameName")))
        {
            gameName = PlayerPrefs.GetString("Debug_gameName");
        }
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("Debug_userList")))
        {
            userList = PlayerPrefs.GetString("Debug_userList");
        }
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("Debug_gameConfig")))
        {
            gameConfig = PlayerPrefs.GetString("Debug_gameConfig");
        }

        isHost = PlayerPrefs.GetInt("Debug_isHost") > 0;
    }
#endif
}
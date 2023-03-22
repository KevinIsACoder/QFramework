using System;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDebugLogin : UIBehaviour
{
    [SerializeField] private InputField _user;
    [SerializeField] private InputField _room;
    [SerializeField] private InputField _robot;
    [SerializeField] private Dropdown _games;
    [SerializeField] private Dropdown _nodes;
    [SerializeField] private Button _login;
    [SerializeField] private Toggle _reset;
    [SerializeField] private Dropdown _gateways;
    [SerializeField] private Dropdown _role;

#if GAME_SOCKET
    private enum GameTypes
    {
        [Description("2")]
        KTV = 2,
        [Description("3")]
        PersonalShow = 3,
        [Description("10")]
        Ludo = 100,
        [Description("20")]
        DrawGuess = 200,
        [Description("30")]
        Dominoes = 300,
        [Description("40")]
        Ball = 400,
        [Description("50")]
        Billiards = 500,
        [Description("60")]
        UnderSpy = 600,
        [Description("70")]
        AmongUS = 700,
    }
    
    private enum Roles
    {
        [Description("平民")]
        Civilian = 1,
        [Description("狼人")]
        Wolf = 2,
    }

    private enum Gateways
    {
        [Description("测试服:119.28.109.92:58887")]
        Develop = 0,
        [Description("马健:10.100.6.2:58887")]
        Node1 = 1,
        [Description("田亚斌(PC):10.41.3.176:58887")]
        Node2 = 2,
        [Description("田亚斌(MAC):10.100.5.54:58887")]
        Node3 = 3,
    }
    
    private enum NodeTypes
    {
        [Description("测试服")]
        Develop = 0,
        [Description("马健")]
        Node1 = 1,
        [Description("田亚斌")]
        Node2 = 2,
    }
    private static string ToDescription(Enum val)
    {
        var type = val.GetType();
        var memberInfo = type.GetMember(val.ToString());
        var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
        //如果没有定义描述，就把当前枚举值的对应名称返回
        return attributes.Length != 1 ? val.ToString() : (attributes.Single() as DescriptionAttribute)?.Description;
    }

    public Action<int, int, int, int, int, int> OnLogin;
    public static UIDebugLogin Instance { get; private set; }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_IOS
    [RuntimeInitializeOnLoadMethod]
    private static void Init()
    {
        #if UNITY_EDITOR
        var asset = Spark.Assets.LoadAsset<GameObject>("Assets/Raw/GUI/Prefabs/UIDebugLogin.prefab");
        #else
        var asset = Spark.Assets.LoadAsset<GameObject>("UIDebugLogin.prefab");
        #endif
        var parent = GameObject.FindWithTag("UIView");
        if (parent != null)
        {
            Instantiate(asset, parent.transform);    
        }
    }
#endif

    protected override void Awake()
    {
        Instance = this;
        _login.onClick.AddListener(() =>
        {
            if (!Enum.TryParse(_games.options[_games.value].text, true, out GameTypes result)) return;
            _room.text = _room.text.PadLeft(_room.characterLimit, '0');
            
            PlayerPrefs.SetInt("debugGameVal", _games.value);
            PlayerPrefs.SetInt(_games.value + "gateway", _gateways.value);
            PlayerPrefs.SetInt(_games.value + "node", _nodes.value);
            PlayerPrefs.SetString(_games.value + "rid", _room.text);
            PlayerPrefs.SetString(_games.value + "uid", _user.text);  
            PlayerPrefs.SetString(_games.value + "robot", _robot.text);
            PlayerPrefs.SetInt(_games.value + "role", _role.value);

            if (result == GameTypes.KTV)
            {
                const string msg = "{\"method_name\":\"initGameInfo\",\"unity_object_name\":\"KTV\",\"data\":{\"room\":{\"room_id\":20012},\"game_status\":{\"room_status\":0,\"game_type\":2,\"game_name\":\"KTV\",\"config_type_data\":[]},\"current_user\":{\"user_id\":12},\"start_point_y\":120,\"is_debug\":true}}";
                GameObject.Find("PlatformResponse").GetComponent<PlatformResponse>().OnMessage(msg);
            }
            // else if (result == GameTypes.AmongUS)
            // {
            //     const string msg = "{\"method_name\":\"initGameInfo\",\"unity_object_name\":\"AmongUS\",\"data\":{\"room\":{\"room_id\":20012},\"game_status\":{\"room_status\":0,\"game_type\":700,\"game_name\":\"AmongUS\",\"config_type_data\":[]},\"current_user\":{\"user_id\":12},\"start_point_y\":120,\"is_debug\":true}}";
            //     GameObject.Find("PlatformResponse").GetComponent<PlatformResponse>().OnMessage(msg);
            // }
            else if(result == GameTypes.PersonalShow)
            {
                // const string msg = "{\"method_name\":\"initGameInfo\",\"unity_object_name\":\"PersonalShow\",\"data\":{\"room\":{\"room_id\":20012},\"game_status\":{\"room_status\":0,\"game_type\":700,\"game_name\":\"PersonalShow\",\"config_type_data\":[]},\"current_user\":{\"user_id\":12},\"start_point_y\":120,\"is_debug\":true}}";
                // GameObject.Find("PlatformResponse").GetComponent<PlatformResponse>().OnMessage(msg);
                var avatarData = "{\"gender\":2,\"init_params\":[{\"id\":17,\"part_list\":[{\"id\":17,\"res_id\":\"bg3\",\"resource_info\":[{\"res_id\":\"bg3\",\"type\":2,\"value\":\"\",\"value_url\":\"https://static.starmakerstudios.com/production/statics/virtual/android/bg3.awb\",\"verify_md5\":\"dc2ac2001c8584f0c21215860f825965\"}]}],\"res_id\":\"bg3\"},{\"id\":1,\"part_list\":[{\"id\":1,\"res_id\":\"AMHR03_L1\",\"resource_info\":[{\"res_id\":\"-1\",\"type\":3,\"value\":\"#fb4021\",\"value_url\":\"#fb4021\"},{\"res_id\":\"AMHR03_L1\",\"type\":1,\"value\":\"\",\"value_url\":\"https://static.starmakerstudios.com/production/statics/virtual/android/AMHR03_L1.awb\",\"verify_md5\":\"7ba9c8a80c313adab82ed267967f2a40\"}]}],\"res_id\":\"AMHR03_L1\"},{\"id\":13,\"part_list\":[{\"id\":13,\"res_id\":\"AMST12B_L1\",\"resource_info\":[{\"res_id\":\"AMST12B_L1\",\"type\":1,\"value\":\"\",\"value_url\":\"https://static.starmakerstudios.com/production/statics/virtual/android/AMST12B_L1.awb\",\"verify_md5\":\"30493578c309a3b9e21f2023274ba831\"}]}],\"res_id\":\"AMST12B_L1\"},{\"id\":20,\"part_list\":[{\"id\":20,\"res_param\":\"{}\",\"resource_info\":[{\"type\":3,\"value\":\"#FFFFFF\",\"value_url\":\"#FFFFFF\"},{\"type\":3,\"value\":\"#FACCCA\",\"value_url\":\"#FACCCA\"}]}]},{\"id\":15,\"part_list\":[{\"id\":15,\"res_id\":\"AMSE03D_L1\",\"resource_info\":[{\"res_id\":\"AMSE03D_L1\",\"type\":1,\"value\":\"\",\"value_url\":\"https://static.starmakerstudios.com/production/statics/virtual/android/AMSE03D_L1.awb\",\"verify_md5\":\"e1b51a7f0e441a13df10d120cd953db1\"}]}],\"res_id\":\"AMSE03D_L1\"},{\"id\":14,\"part_list\":[{\"id\":14,\"res_id\":\"AMPT06A_L1\",\"resource_info\":[{\"res_id\":\"AMPT06A_L1\",\"type\":1,\"value\":\"\",\"value_url\":\"https://static.starmakerstudios.com/production/statics/virtual/android/AMPT06A_L1.awb\",\"verify_md5\":\"6b8a2c13f8346a1e96fd75dd71d51cf3\"}]}],\"res_id\":\"AMPT06A_L1\"}],\"showheight\":640,\"show_page\":\"\"}";
                AvartaScene.Enter("Game/PersonalShow/Scenes/PersonalShow", avatarData);
            }
            else
            {
                var gateways = (Gateways)_gateways.value;
                var gatewaysDescriptions = ToDescription(gateways).Split(':');
                GameObject.Find("PlatformResponse").GetComponent<PlatformResponse>().OpenSocket(gatewaysDescriptions[1], int.Parse(gatewaysDescriptions[2]));
                    
                var rid = $"{ToDescription(result)}{_nodes.value}{_room.text}";
                Debug.Log("Login " + rid);
                var uid = int.Parse(_user.text);
                OnLogin?.Invoke(uid, int.Parse(rid), (int)result, _reset.isOn ? 1 : 0, int.Parse(_robot.text), GetRoleId((int)_role.value));
            }
        });

        foreach (var eName in Enum.GetNames(typeof(NodeTypes)))
        {
            if (Enum.TryParse(eName, true, out NodeTypes result))
            {
                _nodes.options.Add(new Dropdown.OptionData(ToDescription(result)));     
            }
        }

        foreach (var eName in Enum.GetNames(typeof(Gateways)))
        {
            if (Enum.TryParse(eName, true, out Gateways result))
            {
                _gateways.options.Add(new Dropdown.OptionData(ToDescription(result)));     
            }
        }

        foreach (var eName in Enum.GetNames(typeof(Roles)))
        {
            if (Enum.TryParse(eName, true, out Roles result))
            {
                _role.options.Add(new Dropdown.OptionData(ToDescription(result)));
            }
        }

        _games.options = Enum.GetNames(typeof(GameTypes)).Select(eName => new Dropdown.OptionData(eName)).ToList();
        _games.onValueChanged.AddListener(UpdateTextByPrefs);
        _games.value = PlayerPrefs.GetInt("debugGameVal", 0);
        UpdateTextByPrefs(_games.value);
    }

    private void UpdateTextByPrefs(int index)
    {
        _gateways.value = PlayerPrefs.GetInt(index + "gateway", 0);
        _role.value = PlayerPrefs.GetInt(index + "role", 0);
        _nodes.value = PlayerPrefs.GetInt(index + "node", 0);
        _room.text = PlayerPrefs.GetString(index + "rid", "");
        _user.text = PlayerPrefs.GetString(index + "uid", "");
        _robot.text = PlayerPrefs.GetString(index + "robot", "");
    }

    private int GetRoleId(int index)
    {
        switch (index)
        {
            case 0:
                return 1;
            case 1:
                return 2;
            default:
                return 0;
        }
    }

#endif
}
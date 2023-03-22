using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Spark;

public class FakeSocket : FakeMessageModel
{

    private static FakeSocket onlyOne;
    private string server_ip = "http://129.226.37.173:9200";
    private static string APINAME_SOCKET = "/sm/game/v1/api/query_response";
    private static string APINAME_CREATE = "/sm/game/v1/internal/desk/create";
    private static string APINAME_MSG = "/sm/game/v1/internal/user/msg";
    private static string APINAME_CLEAR = "/sm/game/v1/internal/desk/reset";
    private string roomId = "12345";
    private string userId = "111111";
    private string gameType = "200";
    private string gameName = "DrawGuess";
    private string gameConfStr = "[{\"field\":\"subType\",\"value\":102}]";
    private string[] userList;
    private bool isAdmin;
    //private int[] tempList = new int[8];
    private int[] gameSeatIdList = new int[8];
    private List<int> voiceSeatIdList = new List<int>(8);
    //private string[,] testList = new string[8, 2];
    //private Hashtable[] testList2 = new Hashtable[8];
    private Dictionary<string, int> seatInfo = new Dictionary<string, int>();

    private Queue<string> msgQueue = new Queue<string>();
    private Action<byte[]> m_onSocketMessage;
    private Action<string> m_onMessage;

    private string _sessionId = "";
    private bool _needNewPost = false;
    public static FakeSocket Get()
    {
        if (onlyOne == null)
        {
            onlyOne = new FakeSocket();
        }
        return onlyOne;
    }

    FakeSocket()
    {
        for (int i = 1; i <= 8; i++)
        {
            gameSeatIdList[i - 1] = 9 - i;
        }
        for (int i = 1; i <= 8; i++)
        {
            //voiceSeatIdList[i - 1] = 300 + i;
            voiceSeatIdList.Add(300 + i);
        }
        //for (int i = 1; i <= 8; i++) {
        //    testList[i-1,0] = (300 + i).ToString();
        //    testList[i-1, 1] = "111111";
        //}
        //for (int i = 1; i <= 8; i++) {
        //    Hashtable ht = new Hashtable();
        //    ht["seatId"] = 300 + i;
        //    ht["uid"] = "111111";
        //    testList2[i] = ht;
        //}
    }

    public void Post()
    {
        _needNewPost = false;
        try
        {
            var headers = new Dictionary<string, string>()
            {
                {"RoomId", roomId },
                {"UserId", userId },
            };
            if (_sessionId != "")
            {
                headers.Add("SessionId", _sessionId);
            }
            HttpHelper.Post(server_ip + APINAME_SOCKET, new byte[0], headers, OnRequestFinished_FakeSocket);
            //HTTPRequest request = new HTTPRequest(new Uri(server_ip + APINAME_SOCKET), HTTPMethods.Post, OnRequestFinished_FakeSocket);

            //request.AddHeader("RoomId", roomId);
            //request.AddHeader("UserId", userId);
            //if (_sessionId != "")
            //{
            //    Debug.Log("yush _sessionId = " + _sessionId);
            //    request.AddHeader("SessionId", _sessionId);
            //}
            //request.Send();
        }
        catch (Exception e)
        {
            Debug.LogError("FakeSocket Post Error:" + e.Message);
            _needNewPost = true;

        }

    }

    void OnRequestFinished_FakeSocket(HttpResponse response)
    {
        try
        {
            //Debug.Log("Request Finished! Text received: " + response.text);

            if (response.isError) {
                _needNewPost = true;
                 Debug.Log("yush HTTP query_response response.isError: " + response.isError);
                return;
            }
             _sessionId = "1";
            // if (response.HasHeader("SessionId"))
            // {
            //     _sessionId = response.GetHeaderValue("SessionId");
            //     Debug.Log("yush HTTP query_response _sessionId: " + _sessionId);
            // }
            if (response.HasHeader("MsgType"))
            {
                string MsgType = response.GetHeaderValue("MsgType");
                Debug.Log("yush HTTP query_response MsgType: " + MsgType);
            }
            if (response.HasHeader("GameType"))
            {
                string GameType = response.GetHeaderValue("GameType");
                Debug.Log("yush HTTP query_response GameType: " + GameType);
            }
            if (response.data.Length > 0)
            {
                m_onSocketMessage?.Invoke(response.data);
                //Debug.Log("yush HTTP query_response response.data: " + response.data);
                //Debug.Log("yush HTTP query_response response: " + response);
                if (!_updateGameInfo)
                {
                    _updateGameInfo = true;
                    UpdateGame(1);
                }
            }
            //_needNewPost = true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            //throw;
        }
        finally
        {
            _needNewPost = true;
            //Debug.Log("yush HTTP query_response finally: ");
        }

    }

    public void SetConf(string ip, string _roomId, string _userId)
    {
        server_ip = ip;
        roomId = _roomId;
        userId = _userId;
        Debug.Log("yush SetConf SERVER_IP=" + server_ip + " roomId=" + roomId + " userId=" + userId);
    }

    public void SetConf(string ip, string _roomId, string _userId, string _gameName, string userListStr,string gameConfig, bool _isAdmin)
    {
        seatInfo.Clear();
        string[] gameSet = _gameName.Split(';');
        gameName = gameSet[0];
        gameType = gameSet[1];
        server_ip = ip;
        roomId = _roomId;
        userId = _userId;
        userList = userListStr.Split(',');
        gameConfStr = gameConfig;
        //for (int i = 0; i < userList.Length; i++)
        //{
        //    seatInfo.Add(userId, voiceSeatIdList[i]);
        //}
        isAdmin = _isAdmin;
        Debug.Log("yush SetConf SERVER_IP=" + server_ip + " roomId=" + roomId + " userId=" + userId);
        Debug.Log("yush SetConf userList = " + userList + " " + isAdmin);
    }

    public void Init(Action<byte[]> _onSocketMessage, Action<string> _onMessage)
    {
        m_onSocketMessage = _onSocketMessage;
        m_onMessage = _onMessage;
        Spark.Scheduler.Interval(CheckPost, 0.01f);
    }

    private void CheckMsg()
    {
        //if (msgQueue.Count > 0) {
        //    m_onMessage?.Invoke(DecodeBase64("utf-8", msgQueue.Dequeue()));
        //}
    }

    private void CheckPost()
    {
        if (_needNewPost)
        {
            Post();
        }
    }

    public void SendTest(byte[] msg)
    {

    }

    public void Start()
    {
        _needNewPost = true;
    }

    public void SendCreate()
    {
        var headers = new Dictionary<string, string>()
        {
            //{"Content-Type", "application/json; charset=utf-8" },
            //{"Connection", "keep-alive" },
            {"RoomId", roomId },
            {"UserId", userId },
            {"GameType", gameType },
        };
        //HTTPRequest request = new HTTPRequest(new Uri(server_ip + APINAME_CREATE), HTTPMethods.Post, OnRequestFinished);
        //request.SetHeader("Content-Type", "application/json; charset=utf-8");
        //request.SetHeader("Connection", "keep-alive");
        //request.AddHeader("RoomId", roomId);
        //request.AddHeader("UserId", userId);
        //request.AddHeader("GameType", gameType);
        JsonCreate jsonObj = new JsonCreate();
        jsonObj.room_id = int.Parse(roomId);
        //jsonObj.game_mode = 1;
        //jsonObj.bet = 100;

        jsonObj.config_type_data = gameConfStr;
        jsonObj.users = new User[userList.Length];
        for (int i = 0; i < userList.Length; i++)
        {
            User player = new User();
            player.user_id = int.Parse(userList[i]);
            player.nick = "nick_" + i;
            player.sex = 1;
            //player.head_url = "";
            player.seat = i;
            if (isAdmin && player.user_id.ToString() == userId)
            {
                player.seat = 2;
            }
            jsonObj.users[i] = player;
        }
        string jsonStr2 = JsonUtility.ToJson(jsonObj);

        //request.RawData = Encoding.UTF8.GetBytes(jsonStr2);
        //request.Send();
        HttpHelper.Post(server_ip + APINAME_CREATE, Encoding.UTF8.GetBytes(jsonStr2), headers, OnRequestFinished);
    }

    public void SendMsgToMessage(string msgStr)
    {
        Hashtable map = Spark.JSON.Parse(msgStr) as Hashtable;
            Debug.Log("yush map " + map.GetType());

            string msgName = map["messageName"] as String;

            if (!string.IsNullOrEmpty(msgName))
            {
                UnityToNative messageType = (UnityToNative)Enum.Parse(typeof(UnityToNative), msgName);
                switch (messageType)
                {
                    case UnityToNative.startGame:
                        SendCreate();
                        break;
                    case UnityToNative.sendGameSocketMessage:
                        SendPlayerMessage((map["data"]) as Hashtable);
                        break;
                    case UnityToNative.operateVoiceChatSeat:
                        break;
                    case UnityToNative.operateGameChatSeat:
                        string newSeatId = ((map["data"]) as Hashtable)["seat_id"].ToString();
                        OperateChatSeat(int.Parse(newSeatId));
                        break;
                    case UnityToNative.smGameSeatLocationUpdate:
                        break;
                    case UnityToNative.gameReadyOk:
                        //InitGame();
                        break;
                    case UnityToNative.callChatInput:
                        break;
                    case UnityToNative.callNativeToast:
                        string msg = ((map["data"]) as Hashtable)["msg"] as string;
                        CallNativeToast(msg);
                        break;
                    default:
                        break;
                }
            }
            
        try
        {


            
        }
        catch (Exception e)
        {
            Debug.LogError("yush SendMsgToMessage "+ e.Message);
            Debug.LogError(e.StackTrace);
        }
    }


    public void SendPlayerMessage(Hashtable dataTable)
    {
        object type = dataTable["msg_type"];
        if (type == null)
        {
            return;
        }
        int msg_type = (int)type;

        Debug.Log("yush type " + type);
        // switch (msg_type)
        // {
        //     case 2000:
        //     case 2004:
        //     case 2005:
        //     case 2009:
        //     case 2010:
        //     case 2011:
        //     case 2012:
        //     case 2013:
        //     case 2014:
        //     case 2015:
        //     case 2016:
        //         Debug.Log("yush msgType " + msg_type);

        //         break;
        //     default:
        //         Debug.Log("yush msgType unknow " + dataTable.ToString());
        //         return;
        // }


        string MsgType = msg_type.ToString();
        string contentStr = dataTable["content"].ToString();
        Debug.Log("yush SendPlayerMsg content = " + contentStr);

        byte[] msg = Convert.FromBase64String(contentStr);

        HttpHelper.Post(server_ip + APINAME_MSG, msg, new Dictionary<string, string> {
            {"RoomId", roomId },
            {"UserId", userId},
            {"GameType", gameType},
            {"MsgType", MsgType},
        }, OnRequestFinished);

        //HTTPRequest request = new HTTPRequest(new Uri(server_ip + APINAME_MSG), HTTPMethods.Post, OnRequestFinished);
        //request.RawData = msg;
        //request.AddHeader("RoomId", roomId);
        //request.AddHeader("UserId", userId);
        //request.AddHeader("GameType", gameType);
        //request.AddHeader("MsgType", MsgType);
        //request.Send();
    }

    void OnRequestFinished(HttpResponse response)
    {
        try
        {
            Debug.Log("Http Text received: " + response.text);

        }
        catch (Exception)
        {

            throw;
        }
        finally
        {

        }

    }

    private bool _updateGameInfo = false;

    public void UpdateGame(int status)
    {
        UpdateGameJson json = new UpdateGameJson()
        {
            data = new UpdateGameData()
            {
                game_status = new GameStatusData()
                {
                    room_status = status,
                    game_type = int.Parse(gameType),
                    game_name = gameName,
                    config_type_data = gameConfStr
                },
            },
            method_name = "updateGameInfo",
            unity_object_name = "Ludo",
        };

        m_onMessage?.Invoke(JsonUtility.ToJson(json));
    }

    public void InitGame()
    {
        _updateGameInfo = false;

        InitGameJson initGameJson = new InitGameJson();
        InitGameData gameData = new InitGameData();
        gameData.start_point_y = 120;
        gameData.is_debug = true;
        initGameJson.method_name = "initGameInfo";
        initGameJson.unity_object_name = gameName;
        RoomData room = new RoomData();
        room.room_id = int.Parse(roomId);
        gameData.room = room;
        GameStatusData statusData = new GameStatusData();
        statusData.room_status = 0;
        statusData.game_type = int.Parse(gameType);
        statusData.game_name = gameName;
        statusData.config_type_data = gameConfStr;
        gameData.game_status = statusData;
        CurrentUserData currentUser = new CurrentUserData();
        currentUser.user_id = int.Parse(userId);
        gameData.current_user = currentUser;
        initGameJson.data = gameData;
        string jsonStr = JsonUtility.ToJson(initGameJson);
        m_onMessage?.Invoke(jsonStr);
    }

    public void SetSeat()
    {
        GameSeatData gameSeatData = new GameSeatData();
        gameSeatData.method_name = "updateGameSeat";
        gameSeatData.unity_object_name = gameName;
        SeatItemsDataVoice[] temp_seatItemsDataVoice = new SeatItemsDataVoice[8];
        int playerNum = userList.Length > 2 ? userList.Length : 2;
        SeatItemsDataGame[] temp_seatItemsDataGame = new SeatItemsDataGame[playerNum];
        for (int i = 0; i < 8; i++)
        {

            SeatItemsDataVoice seatItemsDataVoice = new SeatItemsDataVoice();
            SeatItemsDataGame seatItemsDataGame = new SeatItemsDataGame();
            seatItemsDataGame.is_host = false;
            seatItemsDataVoice.seat_id = voiceSeatIdList[i];
            seatItemsDataGame.seat_id = gameSeatIdList[i];
            seatItemsDataVoice.seat_status = 0;
            seatItemsDataVoice.star_light = 0;
            seatItemsDataVoice.extra = "";
            if (i < userList.Length)
            {
                string userIdStr = userList[i];
                if (isAdmin && userIdStr == userId)
                {
                    seatItemsDataGame.is_host = true;
                }


                UserInfoData userInfoData = new UserInfoData();
                userInfoData.nick = "nick" + i;
                userInfoData.verified_icon = "";
                userInfoData.profile_image = "";
                userInfoData.uid = int.Parse(userIdStr);
                userInfoData.roles = new int[] { 0, 0 };
                PortraitPendantInfoData portraitPendantInfoData = new PortraitPendantInfoData();
                portraitPendantInfoData.url = "";
                userInfoData.portrait_pendant_info = portraitPendantInfoData;
                ExtraBeanData extraBeanData = new ExtraBeanData();
                VerifiedInfo verifiedInfo = new VerifiedInfo();
                verifiedInfo.icon = "";
                extraBeanData.verified_info = verifiedInfo;
                userInfoData.extra_bean = extraBeanData;
                seatItemsDataVoice.user_info = userInfoData;
                seatItemsDataGame.user_info = userInfoData;
                seatItemsDataGame.status = 1;
                //seatItemsDataGame.is_host = false;
                temp_seatItemsDataGame[i] = seatItemsDataGame;
            }

            temp_seatItemsDataVoice[i] = seatItemsDataVoice;



        }
        SeatItemsData seatItemsData = new SeatItemsData();
        seatItemsData.seatItemsDataGame = temp_seatItemsDataGame;
        seatItemsData.seatItemsDataVoice = temp_seatItemsDataVoice;

        gameSeatData.data = seatItemsData;

        string jsonStr = JsonUtility.ToJson(gameSeatData);
        m_onMessage?.Invoke(jsonStr);
    }

    public void ClearRoom()
    {
       try
        {
            var headers = new Dictionary<string, string>()
            {
                {"RoomId", roomId },
                {"UserId", userId },
            };
            if (_sessionId != "")
            {
                Debug.Log("yush _sessionId = " + _sessionId);
                headers.Add("SessionId", _sessionId);
            }

            HttpHelper.Post(server_ip + APINAME_CLEAR, new byte[0], headers, OnRequestFinished);
        }
        catch (Exception)
        {
            Debug.LogError("FakeSocket Post Error");
            _needNewPost = true;

        } 
    }

    public void OperateChatSeat(int seat_id)
    {
        //if (!seatInfo.ContainsValue(seat_id)) {
        //    seatInfo.Add(userId, seat_id);
        //}
        voiceSeatIdList.Reverse();
        SetSeat();
    }

    public void CallChatInput()
    { 
      // TO-DO yush 显示模拟输入框   
    }

    public void CallNativeToast(string msg)
    {
        Debug.LogWarning(msg);
    }

    ///解码
    public byte[] DecodeBase64(string code_type, string code)
    {
        byte[] bytes = Convert.FromBase64String(code);
        return bytes;
    }

}
using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public class FakeMessageModel
{
    public enum UnityToNative {
        sendGameSocketMessage = 0,
        operateVoiceChatSeat = 1,
        operateGameChatSeat = 2,
        startGame = 3,
        continueGame = 4,
        gameReadyOk = 5,
        clickGameRoomBtn = 6,
        smGameConfigUpdate = 7,
        smGameSeatLocationUpdate = 8,
        requestOnLineUserList = 9,
        callChatInput = 10,
        updateChatConfig = 11,
        leaveGameSeat = 12,
        setRoomGatherStatus = 13,
        inviteUser = 14,
        getHttpInfo = 15,
        openNativeRoute = 16,
        followUserSuccess = 17,
        callNativeToast = 18,
        saveImage = 19,
        countContent = 20,
        getAudioVolume = 21,
        setAudioVolume = 22,
    }

    [Serializable]
    public struct JsonCreate
    {
        public int room_id;
        public User[] users;

        public string config_type_data;
    }
    [Serializable]
    public struct User
    {
        public int user_id;
        public string nick;
        public int sex;
        public int seat;
    }
    [Serializable]
    public struct Message
    {
        public string messageName;
        public MessageData data;
    }

    [Serializable]
    public struct MessageData
    {
        public string content;
        public int msg_type;
        public int game_type;
    }

    [Serializable]
    public struct InitGameJson
    {
        public InitGameData data;
        public string method_name;
        public string unity_object_name;
    }

    [Serializable]
    public struct InitGameData
    {
        public RoomData room;
        public GameStatusData game_status;
        public CurrentUserData current_user;
        public int start_point_y;
        public bool is_debug;

    }

    [Serializable]
    public struct UpdateGameJson
    {
        public UpdateGameData data;
        public string method_name;
        public string unity_object_name;
    }

    [Serializable]
    public struct UpdateGameData
    {
        public GameStatusData game_status;
    }

    [Serializable]
    public struct RoomData
    {
        public int room_id;
    }

    [Serializable]
    public struct GameStatusData
    {
        public int room_status;
        public int game_type;
        public string game_name;
        public string config_type_data;
    }

    [Serializable]
    public struct CurrentUserData
    {
        public int user_id;
    }

    [Serializable]
    public struct GameSeatData
    {
        public SeatItemsData data;
        public string method_name;
        public string unity_object_name;
    }

    [Serializable]
    public struct SeatItemsData
    {
        public SeatItemsDataVoice[] seatItemsDataVoice;
        public SeatItemsDataGame[] seatItemsDataGame;
    }

    [Serializable]
    public struct SeatItemsDataVoice
    {
        public int seat_id;
        public int seat_status;
        public int star_light;
        public string extra;
        public UserInfoData user_info;
    }

    [Serializable]
    public struct UserInfoData
    {
        public PortraitPendantInfoData portrait_pendant_info;
        public ExtraBeanData extra_bean;
        public string nick;
        public string verified_icon;
        public string profile_image;
        public int uid;
        public int[] roles;
    }

    [Serializable]
    public struct PortraitPendantInfoData
    {
        public string url;
    }

    [Serializable]
    public struct ExtraBeanData
    {
        public VerifiedInfo verified_info;
    }

    [Serializable]
    public struct VerifiedInfo
    {
        public string icon;
    }

    [Serializable]
    public struct SeatItemsDataGame
    {
        public int seat_id;
        public int status;
        public bool is_host;
        public UserInfoData user_info;
    }
}
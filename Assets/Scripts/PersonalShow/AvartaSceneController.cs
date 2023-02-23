using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using XLua;
using KLFramework.Avatar;
using System;
using Spark.Tweening;
using UnityEngine.UI;
using System.IO;

public enum MOVEDIRECTION
{
    IDENTITY = 0,
    UP = 1,
    DOWN = 2
}

[LuaCallCSharp]
public class AvartaSceneController : MonoBehaviour
{
    public Camera CameraPlayer;
    public GameObject _BackGround;
    public Transform _AvartaParent;

    public Image _ColorBg;
    public Image _PictrueBg;
    public float DragIntensity = 0.1f;
    private float _Rotate = 0;

    private bool _Dragging;
    private Vector3 _LastPosition;
    public Vector3 DeltaPosition => Input.mousePosition - _LastPosition;
    private BasicAvatarRule<int> avartaRule;

    private Spark.ObjectPool<AvartaPlayer> avartaPool;

    private AvartaPlayer m_avartaPlayer;

    public RectTransform m_avatarTrans;
    private Vector2 m_originSizeDelta;
    private float m_scalefactor;
    private float m_showheight = 0;

    private float m_canvasHeight = 0;

    ///双指缩放
    private bool isInit = false;
    Vector3 touch1, touch2;
    float touchScale;
    public float scaleSpeed = 1f;

    public float moveSpeed = 1f;
    private float min = 0, max = 5f;
    public GameObject m_ColorBgParent;

    private RenderTexture m_renderTex;

    private float m_originPosY = 0f;
    private float m_scaleMin, m_scaleMax;
    private float m_moveMin, m_moveMax;

    private MOVEDIRECTION m_movDirectionY = MOVEDIRECTION.IDENTITY;
    private bool m_isTouchMoved = false;
    private bool m_enableMove = false;
    private bool m_enableScale = false;
    private Vector3 m_startTouchPos = Vector3.zero;

    void Awake()
    {
        CameraPlayer.transform.localRotation = Quaternion.identity;
        SetRenderTexture(AvatarConfig.design_ResolutionX, AvatarConfig.design_ResolutionY);
    }

    // Update is called once per frame
    void Update()
    {
        if (_CheckDragging())
        {
            _Rotate -= DeltaPosition.x * DragIntensity;
        }
        _AvartaParent.rotation = Quaternion.Euler(0, _Rotate, 0);

        //手指缩放
        //不是双指就关闭
        if (Input.touchCount != 2)
        {
            isInit = false;
        }

        //初始化
        if (Input.touchCount == 2 && !isInit)
        {
            //两指点位
            touch1 = Input.GetTouch(0).position;
            touch2 = Input.GetTouch(1).position;
            isInit = true;
        }

        if (Input.touchCount == 1)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                m_startTouchPos = Input.GetTouch(0).position;
                m_isTouchMoved = false;
            }

            if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                m_isTouchMoved = true;
                if (Input.GetTouch(0).position.y > m_startTouchPos.y)
                {
                    m_movDirectionY = MOVEDIRECTION.UP;
                }
                else if (Input.GetTouch(0).position.y < m_startTouchPos.y)
                {
                    m_movDirectionY = MOVEDIRECTION.DOWN;
                }
                m_startTouchPos = Input.GetTouch(0).position;
            }

            if(Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Stationary)
            {
                m_isTouchMoved = false;
            }
        }

        if (Input.touchCount == 2)
        {
            //两指缩放比例
            touchScale = Vector3.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position) / Vector3.Distance(touch1, touch2);
            //利用scaleSpeed控制缩放速度
            var newScale = _AvartaParent.localScale.y + (touchScale - 1) * scaleSpeed;
            //缩放模型大小
            if (newScale > m_scaleMin && newScale < m_scaleMax && m_enableScale)
            {
                _AvartaParent.localScale = new Vector3(newScale, newScale, newScale);
            }

            //设置位置, touchScale - 1 < 0  说明是双指在缩小状态
            if (touchScale - 1 < 0)
            {
                if (Mathf.Abs(_AvartaParent.position.y) > 1e-2)
                {
                    var posY = _AvartaParent.position.y + (m_originPosY - _AvartaParent.position.y) * touchScale;
                    _AvartaParent.position = new Vector3(0, posY, 0);
                }
            }
        }
        else if (Input.touchCount == 1 && m_isTouchMoved && m_enableMove) //移动模型
        {
            var moveDelta = Time.deltaTime * moveSpeed;
            if (m_movDirectionY == MOVEDIRECTION.DOWN) moveDelta = -moveDelta;
            var nowPos = _AvartaParent.position;
            var nowPosY = nowPos.y + moveDelta;
            nowPosY = Mathf.Clamp(nowPosY, _AvartaParent.localScale.y * m_moveMin, _AvartaParent.localScale.y * m_moveMax);
            nowPos.y = nowPosY;
            _AvartaParent.position = nowPos;
        }
    }

    string FaceId = "";
    string EyeId = "";
    string NoseId = "";
    string MouthId = "";
    void OnGUI()
    {
        FaceId = GUI.TextField(new Rect(0, 0, 100, 50), FaceId);
        EyeId = GUI.TextField(new Rect(0, 100, 100, 50), EyeId);
        NoseId = GUI.TextField(new Rect(0, 200, 100, 50), NoseId);
        MouthId = GUI.TextField(new Rect(0, 300, 100, 50), MouthId);
        if (GUI.Button(new Rect(200, 100, 100, 50), "脸型"))
        {
            // var screenpos = RectTransformUtility.WorldToScreenPoint(m_uiCamera, m_line.transform.position);
            // var screenpos_1 = new Vector3(screenpos.x, screenpos.y, 9);
            // var worldPos = Camera.main.ScreenToWorldPoint(screenpos_1);
            // var dis = Vector3.Distance(CameraPlayer.transform.position, m_avartaPlayer.GetAvatarObj().transform.position);
            // Debug.Log("3333333333333333333 " + dis);
            // SetCameraParam("{\"full_screen\":\"0\", \"camera_param\":{\"pos\":-288}}");
            // GetCameraView(720f, 640f);
            // string data = "{\"gender\":2,\"init_params\":[{\"id\":17,\"part_list\":[{\"id\":17,\"res_param\":\"Assets/PersonalShow/AvatarX/F_Shape_Data/F_Face_00.txt\",\"resource_info\":[{\"type\":3,\"value\":\"#FFBCB4\",\"value_url\":\"#FFBCB4\"}]}]},{\"id\":1,\"part_list\":[{\"id\":1,\"res_id\":\"AMHR05_L1\",\"resource_info\":[{\"res_id\":\"-1\",\"type\":3,\"value\":\"#f5b3d5\",\"value_url\":\"#f5b3d5\"},{\"res_id\":\"AMHR05_L1\",\"type\":1,\"value\":\"\",\"value_url\":\"https://static.starmakerstudios.com/production/statics/virtual/ios/AMHR05_L1.awb\",\"verify_md5\":\"2bfd7dbdca6aec3457fc2f838d39086c\"}]}],\"res_id\":\"AMHR05_L1\"},{\"id\":13,\"part_list\":[{\"id\":13,\"res_id\":\"AMST12B_L1\",\"resource_info\":[{\"res_id\":\"AMST12B_L1\",\"type\":1,\"value\":\"\",\"value_url\":\"https://static.starmakerstudios.com/production/statics/virtual/ios/AMST12B_L1.awb\",\"verify_md5\":\"dd475438183a633fbed3654d68100cee\"}]}],\"res_id\":\"AMST12B_L1\"}, {\"id\":15,\"part_list\":[{\"id\":15,\"res_param\":\"{}\",\"res_id\":\"AMSE03A_L1\",\"resource_info\":[{\"res_id\":\"AMSE03A_L1\",\"type\":1,\"value\":\"\",\"value_url\":\"https://static.starmakerstudios.com/production/statics/virtual/ios/AMSE03A_L1.awb\",\"verify_md5\":\"aa2df40da9d46a6bed875f39ed108f9d\"}]}],\"res_id\":\"AMSE03A_L1\"},{\"id\":14,\"part_list\":[{\"id\":14,\"res_param\":\"{}\",\"res_id\":\"AMPT07A_L1\",\"resource_info\":[{\"res_id\":\"AMPT07A_L1\",\"type\":1,\"value_url\":\"https://static.starmakerstudios.com/production/statics/virtual/ios/AMPT07A_L1.awb\",\"verify_md5\":\"1dca2922e0a2ec0ba7aec0145ba0d697\"}]}],\"res_id\":\"AMPT07A_L1\"}],\"showheight\":672,\"show_page\":\"profile\"}";
            //var avatar = new AvartaPlayer(data, false);
            //TestFaceData(2, int.Parse(FaceId));
            //UpdateAvartaData(1, "{\"id\":13,\"part_list\":[{\"id\":13,\"res_id\":\"AFST05A_L1\",\"face_param\":\"Assets/PersonalShow/AvatarX/F_Shape_Data/F_Face_04.txt\", \"resource_info\":[{\"res_id\":\"AFST05A_L1\",\"type\":1,\"value\":\"1/AFST10A_L1.awb\",\"value_url\":\"https://static.starmakerstudios.com/production/statics/virtual/v2/awb/AFST05A_L1.awb\",\"verify_md5\":\"aa71203ee10de4247c88697973f56317\"}]}],\"res_id\":\"AFST05A_L1\"}");
            string cameradata = "{\"full_screen\":\"1\", \"camera_params\":{\"cameraId\":\"1\"}}";
            SetCameraParam(cameradata);
        }

        if (GUI.Button(new Rect(200, 200, 100, 50), "眼睛"))
        {
            string cameradata = "{\"full_screen\":\"0\", \"camera_params\":{\"cameraId\":\"2\"}}";
            SetCameraParam(cameradata);
            //TestFaceData(4, int.Parse(EyeId));
            //UpdateAvartaData(1, "{\"id\":11,\"part_list\":[{\"id\":11,\"res_id\":\"AFST05A_L1\",\"face_param\":\"Assets/PersonalShow/AvatarX/F_Shape_Data/F_Eye_00.txt\", \"resource_info\":[{\"res_id\":\"AFST05A_L1\",\"type\":2,\"value\":\"Assets/PersonalShow/AvatarX/TC_Lip/Lip06.png\",\"value_url\":\"https://static.starmakerstudios.com/production/statics/virtual/v2/awb/AFST05A_L1.awb\",\"verify_md5\":\"aa71203ee10de4247c88697973f56317\"}]}],\"res_id\":\"AFST05A_L1\"}");
        }

        if (GUI.Button(new Rect(200, 300, 100, 50), "鼻子"))
        {
            string cameradata = "{\"full_screen\":\"0\", \"camera_params\":{\"cameraId\":\"3\"}}";
            SetCameraParam(cameradata);
            //TestFaceData(8, int.Parse(NoseId));
            // UpdateAvartaData(1, "{\"id\":8,\"part_list\":[{\"id\":8,\"res_id\":\"AFST05A_L1\",\"face_param\":\"Assets/PersonalShow/AvatarX/F_Shape_Data/F_Nose_00.txt\", \"resource_info\":[{\"res_id\":\"AFST05A_L1\",\"type\":1,\"value\":\"\",\"value_url\":\"https://static.starmakerstudios.com/production/statics/virtual/v2/awb/AFST05A_L1.awb\",\"verify_md5\":\"aa71203ee10de4247c88697973f56317\"}]}],\"res_id\":\"AFST05A_L1\"}");
        }

        if (GUI.Button(new Rect(200, 400, 100, 50), "嘴巴"))
        {
            string cameradata = "{\"full_screen\":\"0\", \"camera_params\":{\"cameraId\":\"4\"}}";
            SetCameraParam(cameradata);
            //TestFaceData(11, int.Parse(MouthId));
            //UpdateAvartaData(1, "{\"id\":11,\"part_list\":[{\"id\":11,\"res_id\":\"AFST05A_L1\",\"face_param\":\"Assets/PersonalShow/AvatarX/F_Shape_Data/F_Mouth_00.txt\", \"resource_info\":[{\"res_id\":\"AFST05A_L1\",\"type\":1,\"value\":\"\",\"value_url\":\"https://static.starmakerstudios.com/production/statics/virtual/v2/awb/AFST05A_L1.awb\",\"verify_md5\":\"aa71203ee10de4247c88697973f56317\"}]}],\"res_id\":\"AFST05A_L1\"}");
        }
    }

    private bool _CheckDragging()
    {
        if (Input.touchCount == 1)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                _Dragging = true;
            }
            else
            {
                _Dragging = false;
            }
        }
        return _Dragging;
    }

    private void LateUpdate()
    {
        _LastPosition = Input.mousePosition;
    }

    public void Init(string data)
    {
        var usrdata = JsonUtility.FromJson<AvartaUserDatas>(data);
        m_scalefactor = Screen.width * 1f / AvatarConfig.design_ResolutionX;
        if (usrdata.show_page == AvartaShowPage.PROFILE)
        {
            float screenRatio = Screen.width * 1f / Screen.height;
            var sceneWidth = usrdata.showheight * screenRatio;
            m_scalefactor = sceneWidth / AvatarConfig.design_ResolutionX;
            m_canvasHeight = usrdata.showheight / m_scalefactor;
        }
        else
        {
            m_canvasHeight = Screen.height / m_scalefactor;
        }

        if (usrdata.showheight > 0)
        {
            m_showheight = usrdata.showheight / m_scalefactor;
        }


        if (m_avartaPlayer != null)
        {
            if (m_avartaPlayer.Gender != usrdata.gender)
            {
                Clear();
            }
        }


        if (m_avartaPlayer == null)
        {
            avartaRule = AvatarConfig.GetBasicRule();
            m_avartaPlayer = new AvartaPlayer(data, false, _AvartaParent);
        }

        _Rotate = 0;
        _BackGround.SetActive(m_avartaPlayer.AvartaUserInfo.ShowPage != AvartaShowPage.PROFILE);
        m_avartaPlayer.GetAvatarObj().transform.position = new Vector3(0, -1f, 0);
        string cameradata = "{\"full_screen\":\"1\", \"camera_params\":{\"cameraId\":\"1\"}}";
        SetCameraParam(cameradata);
    }

    public void Clear()
    {
        m_avartaPlayer.DestroyAvatar();
        m_avartaPlayer = null;
    }

    public void TestFaceData(int type, int id)
    {
        var str_id = string.Format("{0:d2}", id);
        var fileName = "F_Face_" + str_id + ".txt";
        if (type == 4)
        {
            fileName = "F_Eye_" + str_id + ".txt";
        }
        else if (type == 8)
        {
            fileName = "F_Nose_" + str_id + ".txt";
        }
        else if (type == 11)
        {
            fileName = "F_Mouth_" + str_id + ".txt";
        }
        fileName = "Assets/PersonalShow/AvatarX/F_Shape_Data/" + fileName;
        var avartaData = new AvatarItem()
        {
            id = type,
            face_param = fileName,
            resource_info = new List<ResourceInfo>(),
        };
        m_avartaPlayer.Render(avartaData);
    }

    public void UpdateAvartaData(int avartaType, string data)
    {
        var avartaData = JsonUtility.FromJson<AvartaData>(data);

        for (int i = 0, count = avartaData.part_list.Count; i < count; i++)
        {
            var itemData = avartaData.part_list[i];
            m_avartaPlayer.Render(itemData);
        }
        m_avartaPlayer.CheckMuteClothes();

        //人物角度回到初始角度
        _Rotate = 0;
        //todo 人物动画
        var animName = AvatarConfig.GetAttributeName(avartaData.id, AvatarCfgAtrrType.AnimName);
        if (!string.IsNullOrEmpty(animName))
        {
            m_avartaPlayer.Animator.Play(animName);
        }

        m_avartaPlayer.SetSkinColor();
    }

    public void ResetAvartaData(string data)
    {
        Clear();
        Init(data);
    }

    // public void SetCameraParam(string data)
    // {
    //     var cameraData = JsonUtility.FromJson<AvartaCameraData>(data);
    //     bool isFullScreen = cameraData.full_screen == 1;
    //     float height = isFullScreen ? m_canvasHeight : m_showheight;

    //     float charactorScale = 1;
    //     float charactorPos = 0;
    //     float pictureBgPos = 0;
    //     float fieldOfView = m_fieldofView;

    //     Vector3 cameraPos = Vector3.zero;

    //     if (isFullScreen)
    //     {
    //         pictureBgPos = 0;
    //         m_avatarTrans.sizeDelta = new Vector2(height, height);
    //     }
    //     else
    //     {
    //         if (cameraData.camera_params.pos == -288)
    //         {
    //             m_avatarTrans.sizeDelta = new Vector2(height, height);
    //             cameraPos = new Vector3(0, 3.5f, -4f);
    //             fieldOfView = m_fieldofView;
    //             charactorScale = m_avatarScale;
    //             charactorPos = height * 0.5f;
    //         }
    //         else
    //         {
    //             charactorScale = m_avatarScale;
    //             m_avatarTrans.sizeDelta = new Vector2(height, height);
    //             charactorPos = height * 0.5f;
    //             pictureBgPos = height * 0.4f + (m_canvasHeight - height) * 0.5f;
    //         }
    //     }

    //     m_oriScale = charactorScale;
    //     // ///人物缩放
    //     SparkTween.DOScale(m_avatarTrans, charactorScale, charactorScale, charactorScale, 0.5f);
    //     //人物位置
    //     SparkTween.DOAnchorPosY(m_avatarTrans, charactorPos, 0.5f);
    //     //背景位置
    //     var rectTrans = _PictrueBg.gameObject.GetComponent<RectTransform>();
    //     SparkTween.DOAnchorPosY(rectTrans, pictureBgPos, 0.5f);
    //     //相机移动
    //     SparkTween.DOMove(CameraPlayer.transform, cameraPos.x, cameraPos.y, cameraPos.z, 0.5f);
    //     SparkTween.DOFieldOfView(CameraPlayer, fieldOfView, 0.5f);
    // }

    public void SetCameraParam(string data)
    {
        var cameraData = JsonUtility.FromJson<AvartaCameraData>(data);
        bool isFullScreen = cameraData.full_screen == 1;
        float rt_width = AvatarConfig.design_ResolutionX;
        float rt_height = AvatarConfig.design_ResolutionY;

        var cameraId = cameraData.camera_params.cameraId;
        var camerCfg = AvatarConfig.GetAvatarCameraCfg(cameraId);
        var avatarPosY = 0f;
        var avatarScale = 1f;
        var pictureBgPos = 0f;
        if (camerCfg != null)
        {
            var camPos = new Vector3(camerCfg.position[0], camerCfg.position[1], camerCfg.position[2]);
            var camRotation = new Vector3(camerCfg.rotation[0], camerCfg.rotation[1], camerCfg.rotation[2]);
            var fov = camerCfg.fov;
            m_enableMove = camerCfg.enablemove > 0;
            m_enableScale = camerCfg.enablescale > 0;
            m_moveMin = camerCfg.movebound[0];
            m_moveMax = camerCfg.movebound[1];
            m_scaleMin = camerCfg.scalebound[0];
            m_scaleMax = camerCfg.scalebound[1];
            avatarScale = m_scaleMin;

            if (!isFullScreen)
            {
                rt_height = Mathf.Max(m_showheight, Screen.width);
                rt_width = rt_height;
                avatarPosY = rt_height * 0.5f;
                pictureBgPos = cameraId == AvatarCameraId.DressCameraId ? rt_height * 0.5f : 0;
            }

            if(rt_width != m_renderTex.width || rt_height != m_renderTex.height)
            {
               SetRenderTexture((int)rt_width, (int)rt_height);
            }
            
            var deltaTime = 0f;
            //相机位置
            SparkTween.DOMove(CameraPlayer.transform, camPos.x, camPos.y, camPos.z, deltaTime);
            SparkTween.DOFieldOfView(CameraPlayer, fov, deltaTime);
            SparkTween.DORotate(CameraPlayer.transform, camRotation.x, camRotation.y, camRotation.z, deltaTime);
            //人物位置和缩放
            SparkTween.DOScale(_AvartaParent, avatarScale, avatarScale, avatarScale, deltaTime);
            SparkTween.DOMove(_AvartaParent, 0, m_originPosY, 0, deltaTime);
            SparkTween.DOAnchorPosY(m_avatarTrans, avatarPosY, deltaTime);
            SparkTween.DOSizeDelta(m_avatarTrans, rt_width, rt_height, deltaTime);
            //背景位置
            var rectTrans = _PictrueBg.gameObject.GetComponent<RectTransform>();
            SparkTween.DOAnchorPosY(rectTrans, pictureBgPos, deltaTime);
        }
    }

    void SetRenderTexture(int rt_width, int rt_height)
    {
        RenderTexture.ReleaseTemporary(m_renderTex);
        m_renderTex = RenderTexture.GetTemporary(rt_width, rt_height, 24);
        CameraPlayer.targetTexture = m_renderTex;
        m_avatarTrans.GetComponent<RawImage>().texture = m_renderTex;
    }

    public void SetColorBg(Color color)
    {
        if (m_ColorBgParent != null) m_ColorBgParent.SetActive(true);
        _PictrueBg.gameObject.SetActive(false);
        if (_ColorBg != null)
        {
            var gradientCom = _ColorBg.gameObject.GetComponent<Gradient>();
            gradientCom.topColor = new Color(color.r, color.g, color.b, 153 / 255.0f);
            gradientCom.bottomColor = new Color(color.r, color.g, color.b, 76 / 255.0f);
            gradientCom.Refresh();
        }
    }

    public void SetPictureBg(Sprite sprite)
    {
        if (m_ColorBgParent != null) m_ColorBgParent.SetActive(false);
        _PictrueBg.gameObject.SetActive(true);
        if (_PictrueBg != null)
        {
            _PictrueBg.sprite = sprite;
        }
    }

    public void Exit()
    {
        _BackGround.SetActive(false);
        m_avatarTrans.gameObject.SetActive(false);
        CameraPlayer.targetTexture = null;
        m_avatarTrans.GetComponent<RawImage>().texture = null;
        RenderTexture.ReleaseTemporary(m_renderTex);
        if (m_avartaPlayer != null)
        {
            m_avartaPlayer.DestroyAvatar();
            m_avartaPlayer = null;
        }
    }
}

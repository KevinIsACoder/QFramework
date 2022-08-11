/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System.Collections.Generic;
using System;
using UnityEngine;
using XLua;
using System.Linq;
using System.Reflection;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using System.IO;
//配置的详细介绍请看Doc下《XLua的配置.doc》
public static class XLuaGenConfig
{
    [LuaCallCSharp]
    public static List<Type> UGUITypes {
        get{
            HashSet<Type> excludes = new HashSet<Type>(){
                // 忽略的类型
                typeof(UnityEngine.UI.DefaultControls),
#if UNITY_EDITOR
                typeof(UnityEngine.UI.GraphicRebuildTracker),
#endif
            };
            List<Type> list = Assembly.Load("UnityEngine.UI").GetExportedTypes()
                .Where(type => !type.IsGenericTypeDefinition)
                // .Where(type => !type.IsAbstract)
                .Where(type => !type.IsInterface)
                // .Where(type => !type.IsNested)
                // .Where(type => !type.IsValueType)
                .Where(type => !excludes.Contains(type))
                .ToList();
            return list;
        }
    }

    // 第三方
    [LuaCallCSharp]
    public static List<Type> ThirdPartTypes = new List<Type>()
    {
        // //自扩展====================================
        // typeof(CCM.UIExtent),
        // typeof(CCM.UIEventListener),
        // typeof(RuntimeAnimatorController),
        // typeof(Common),
        // typeof(CCM.AudioManager),
        // typeof(CCM.UIDrap),
        // typeof(WavUtility),
        // typeof(I2.Loc.LocalizationManager),
        // typeof(I2.Loc.Localize),
        // typeof(I2.Loc.TermData),
        // typeof(PhysicsCustom),
        // typeof(Ludo.UIUtils),

		// // spine
		// typeof(Spine.Unity.SkeletonAnimation),
        // typeof(Spine.Unity.SkeletonGraphic),
        // typeof(Spine.AnimationState),

        // typeof(PathologicalGames.PoolManager),

        // typeof(Ludo.SpineUtils),
        // typeof(EnhancedUI.EnhancedScroller.EnhancedList),

        // //插件=======================================
        //DOTween
        typeof(DG.Tweening.AutoPlay),
        typeof(DG.Tweening.AxisConstraint),
        typeof(DG.Tweening.Ease),
        typeof(DG.Tweening.LogBehaviour),
        typeof(DG.Tweening.LoopType),
        typeof(DG.Tweening.PathMode),
        typeof(DG.Tweening.PathType),
        typeof(DG.Tweening.RotateMode),
        typeof(DG.Tweening.ScrambleMode),
        typeof(DG.Tweening.TweenType),
        typeof(DG.Tweening.UpdateType),
        typeof(DG.Tweening.DOTween),
        typeof(DG.Tweening.DOVirtual),
        typeof(DG.Tweening.EaseFactory),
        typeof(DG.Tweening.Tweener),
        typeof(DG.Tweening.Tween),
        typeof(DG.Tweening.Sequence),
        typeof(DG.Tweening.TweenParams),
        typeof(DG.Tweening.Core.ABSSequentiable),
        typeof(DG.Tweening.Core.TweenerCore<Vector3, Vector3, DG.Tweening.Plugins.Options.VectorOptions>),
        typeof(DG.Tweening.TweenCallback),
        typeof(DG.Tweening.TweenExtensions),
        typeof(DG.Tweening.TweenSettingsExtensions),
        typeof(DG.Tweening.ShortcutExtensions),
        
        // System
        typeof(System.GC),
        typeof(System.IO.Directory),
        typeof(System.Array),
        typeof(System.Object),
        //typeof(System.Convert),
        typeof(System.DateTime),
        typeof(System.TimeSpan),
        typeof(System.DateTimeKind),
    };

    //lua中要使用到C#库的配置，比如C#标准库，或者Unity API，第三方库等。
    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp = new List<Type>()
    {
        typeof(UnityEngine.CircleCollider2D),
        typeof(UnityEngine.TextEditor),
        typeof(UnityEngine.LogType),        
        typeof(UnityEngine.LocationInfo),        
        typeof(UnityEngine.LocationService),
        typeof(UnityEngine.LocationServiceStatus),        
        typeof(UnityEngine.TouchPhase),
        typeof(UnityEngine.Collision),
        typeof(UnityEngine.ImageConversion),
        typeof(UnityEngine.RuntimePlatform),
        typeof(UnityEngine.Bounds),
        typeof(UnityEngine.Ray2D),
        typeof(UnityEngine.Keyframe),
        typeof(UnityEngine.FontStyle),        
        typeof(UnityEngine.AnimationCurve),
        typeof(UnityEngine.AnimationCullingType),
        typeof(UnityEngine.ParticleSystem),
        typeof(UnityEngine.EventSystems.PointerEventData),
        typeof(UnityEngine.Gyroscope),
        typeof(UnityEngine.Light),
        typeof(UnityEngine.U2D.SpriteAtlas),
        typeof(UnityEngine.U2D.SpriteAtlasManager),
        typeof(UnityEngine.Events.UnityAction),
        typeof(UnityEngine.Events.UnityEvent),
        typeof(UnityEngine.Events.UnityEventBase),

        // 
        typeof(UnityEngine.HorizontalWrapMode),
        typeof(UnityEngine.WaitForSeconds),
        typeof(UnityEngine.WaitForFixedUpdate),
        typeof(UnityEngine.WaitForEndOfFrame),
        typeof(UnityEngine.Shader),
        typeof(UnityEngine.Screen),
        typeof(UnityEngine.GameObject),
        typeof(UnityEngine.Behaviour),
        typeof(UnityEngine.MonoBehaviour),
        typeof(UnityEngine.MeshRenderer),
        typeof(UnityEngine.Animator),
        typeof(UnityEngine.SkinnedMeshRenderer),
        typeof(UnityEngine.MeshCollider),
        typeof(UnityEngine.Camera),
        typeof(UnityEngine.Canvas),
        typeof(UnityEngine.CanvasGroup),
        typeof(UnityEngine.AudioListener),
        typeof(UnityEngine.AudioSource),
        typeof(UnityEngine.BoxCollider),
        typeof(UnityEngine.AsyncOperation),
        typeof(UnityEngine.Application),
        typeof(UnityEngine.AudioClip),
        typeof(UnityEngine.Animation),
        typeof(UnityEngine.AnimationClip),
        typeof(UnityEngine.AnimationState),
        typeof(UnityEngine.AnimatorStateInfo),
        typeof(UnityEngine.BoxCollider2D),
        typeof(UnityEngine.Color),
        typeof(UnityEngine.Time),
        typeof(UnityEngine.Mathf),
        typeof(UnityEngine.Ray),
        typeof(UnityEngine.RaycastHit),
        typeof(UnityEngine.RaycastHit2D),
        typeof(UnityEngine.Input),
        typeof(UnityEngine.Touch),
        typeof(UnityEngine.Quaternion),
        typeof(UnityEngine.Physics),
        typeof(UnityEngine.Vector2),
        typeof(UnityEngine.Vector3),
        typeof(UnityEngine.Vector4),
        typeof(UnityEngine.Material),
        typeof(UnityEngine.PlayerPrefs),
        typeof(UnityEngine.SystemLanguage),
        typeof(UnityEngine.Transform),
        typeof(UnityEngine.Object),
        typeof(UnityEngine.Debug),
        typeof(UnityEngine.Resources),
        typeof(UnityEngine.Font),
        typeof(UnityEngine.LayerMask),
        typeof(UnityEngine.Sprite),
        typeof(UnityEngine.Texture),
        typeof(UnityEngine.Texture2D),
        typeof(UnityEngine.RectTransform),
        typeof(UnityEngine.RectTransformUtility),
        typeof(UnityEngine.Random),
        typeof(UnityEngine.Rigidbody),
        typeof(UnityEngine.CapsuleCollider),
        typeof(UnityEngine.Component),
        typeof(UnityEngine.EventSystems.EventSystem),
        typeof(UnityEngine.Rect),
        typeof(UnityEngine.TextAsset),
        typeof(UnityEngine.MaterialPropertyBlock),
        typeof(UnityEngine.KeyCode),
        typeof(UnityEngine.SceneManagement.Scene),
        typeof(UnityEngine.SceneManagement.SceneManager),
        typeof(UnityEngine.SceneManagement.LoadSceneMode),
        typeof(UnityEngine.QualitySettings),
        typeof(UnityEngine.RenderSettings),
        typeof(UnityEngine.SystemInfo),
        typeof(UnityEngine.Rigidbody2D),
        typeof(UnityEngine.Joint2D),
        typeof(UnityEngine.AnchoredJoint2D),
        typeof(UnityEngine.SpringJoint2D),
        typeof(UnityEngine.DistanceJoint2D),
        typeof(UnityEngine.TargetJoint2D),
        typeof(UnityEngine.RelativeJoint2D),
        typeof(UnityEngine.FixedJoint2D),
        typeof(UnityEngine.Mesh),
        typeof(UnityEngine.MeshFilter),
        typeof(UnityEngine.Collider),
        typeof(UnityEngine.Cloth),
        typeof(UnityEngine.BoneWeight),
        typeof(UnityEngine.HumanBone),
        typeof(UnityEngine.SkeletonBone),
        typeof(UnityEngine.Renderer),
        typeof(UnityEngine.SpriteRenderer),
        typeof(UnityEngine.FogMode),
        typeof(UnityEngine.Color32),
        typeof(UnityEngine.ColorSpace),
        typeof(UnityEngine.ColorUtility),
        typeof(UnityEngine.TextGenerator),

        typeof(UnityEngine.TextMesh),
        typeof(UnityEngine.LineRenderer),
        typeof(UnityEngine.TrailRenderer),
        typeof(UnityEngine.RenderTexture),
        typeof(UnityEngine.RenderTextureFormat),
        typeof(UnityEngine.Rendering.GraphicsDeviceType),
        typeof(UnityEngine.SleepTimeout),
        typeof(UnityEngine.AssetBundle),
        typeof(UnityEngine.Networking.UnityWebRequest),
        typeof(UnityEngine.Networking.DownloadHandler),
        typeof(UnityEngine.WWWForm),
        typeof(UnityEngine.ScreenCapture),
        typeof(UnityEngine.Video.VideoClip),
        typeof(UnityEngine.Video.VideoPlayer),
        typeof(UnityEngine.Video.Video3DLayout),
        typeof(UnityEngine.Video.VideoAspectRatio),
        typeof(UnityEngine.Video.VideoAudioOutputMode),
        typeof(UnityEngine.Video.VideoRenderMode),
        typeof(UnityEngine.Video.VideoSource),
        typeof(UnityEngine.Video.VideoTimeReference),
        typeof(UnityEngine.Video.VideoTimeSource),
        typeof(UnityEngine.SphereCollider),
        typeof(UnityEngine.TextAnchor),
    };

    //C#静态调用Lua的配置（包括事件的原型），仅可以配delegate，interface
    [CSharpCallLua]
    public static List<Type> CSharpCallLua = new List<Type>()
    {
        typeof(Action),
        typeof(Func<double, double, double>),
        typeof(Action<int, uint, string, string>),
        typeof(Action<string>),
        typeof(Action<double>),
        typeof(Action<int>),
        typeof(Action<bool>),
        typeof(Action<float>),
        typeof(Action<PointerEventData,UnityEngine.RectTransform>),
        typeof(UnityEngine.Events.UnityAction),
        typeof(UnityEngine.Events.UnityAction<UnityEngine.Vector2>),
        typeof(System.Collections.IEnumerator),
        typeof(UnityEngine.Events.UnityEvent),
        typeof(UnityEngine.Events.UnityEventBase),
    };

    //黑名单
    [BlackList]
    public static List<List<string>> BlackList = new List<List<string>>()
    {
        new List<string>(){"UnityEngine.TextureFormat", "DXT1Crunched"},
        new List<string>(){"UnityEngine.TextureFormat", "DXT5Crunched"},
        new List<string>(){"UnityEngine.WWW", "movie"},
        new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
        new List<string>(){"UnityEngine.Security", "GetChainOfTrustValue"},
        new List<string>(){"UnityEngine.CanvasRenderer", "onRequestRebuild"},
        new List<string>(){"UnityEngine.Light", "areaSize"},
        new List<string>(){"UnityEngine.Light", "SetLightDirty"},
        new List<string>(){"UnityEngine.Light", "shadowRadius"},
        new List<string>(){"UnityEngine.Light", "shadowAngle"},
        new List<string>(){"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
#if !UNITY_WEBPLAYER 
        new List<string>(){"UnityEngine.Application", "ExternalEval"},
#endif
        new List<string>(){"UnityEngine.GameObject", "networkView"}, //4.6.2 not support
        new List<string>(){"UnityEngine.Component", "networkView"},  //4.6.2 not support
        new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
        new List<string>(){"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
        
        new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
        new List<string>(){"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
        new List<string>(){"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
        new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},
        new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},
        new List<string>(){"UnityEngine.MonoBehaviour", "useGUILayout"},
        new List<string>(){"UnityEngine.Input", "IsJoystickPreconfigured","System.String"},
        new List<string>(){"UnityEngine.UI.Graphic", "OnRebuildRequested"},
        new List<string>(){"UnityEngine.UI.Text", "OnRebuildRequested"},
        new List<string>(){"UnityEngine.MeshRenderer", "receiveGI"},
        new List<string>(){"UnityEngine.MeshRenderer", "scaleInLightmap"},
        new List<string>(){"UnityEngine.MeshRenderer", "stitchLightmapSeams"},
        new List<string>(){"UnityEngine.MeshRenderer", "scaleInLightmap"},
        new List<string>(){"UnityEngine.QualitySettings", "streamingMipmapsRenderersPerFrame"},
        new List<string>(){"UnityEngine.Texture", "imageContentsHash"},
        
#if !PLATFORM_SUPPORTS_GAMEPAD_AUDIO
        new List<string>(){"UnityEngine.AudioSource", "GamepadSpeakerSupportsOutputType","UnityEngine.GamepadSpeakerOutputType"},
        new List<string>(){"UnityEngine.AudioSource", "gamepadSpeakerOutputType"},
        new List<string>(){"UnityEngine.AudioSource", "PlayOnGamepad","System.Int32"},
        new List<string>(){"UnityEngine.AudioSource", "DisableGamepadOutput"},
        new List<string>(){"UnityEngine.AudioSource", "SetGamepadSpeakerMixLevel","System.Int32","System.Int32"},
        new List<string>(){"UnityEngine.AudioSource", "SetGamepadSpeakerMixLevelDefault", "System.Int32"},
        new List<string>(){"UnityEngine.AudioSource", "SetGamepadSpeakerMixLevel","System.Int32","System.Boolean"},
        new List<string>(){"UnityEngine.AudioSource", "SetGamepadSpeakerRestrictedAudio", "System.Int32","System.Boolean"},

#endif
        
        new List<string>(){ "UnityEngine.AnimatorControllerParameter", "set_name"},
        new List<string>(){"System.Type", "IsSZArray"},
        // I2.Loc.Localize: UNITY_EDITOR
        new List<string>(){ "I2.Loc.Localize", "Source"},
        new List<string>(){ "I2.Loc.Localize", "Source"},
        // I2.Loc.LanguageSourceData: UNITY_EDITOR
        new List<string>(){ "I2.Loc.LanguageSourceData", "Spreadsheet_LocalFileName"},
        new List<string>(){ "I2.Loc.LanguageSourceData", "Spreadsheet_LocalCSVSeparator"},
        new List<string>(){ "I2.Loc.LanguageSourceData", "Spreadsheet_LocalCSVEncoding"},
        new List<string>(){ "I2.Loc.LanguageSourceData", "Spreadsheet_SpecializationAsRows"},
        new List<string>(){ "I2.Loc.LanguageSourceData", "Google_Password"},
        new List<string>(){ "I2.Loc.LanguageSourceData", "Editor_SetDirty"},
        // I2.Loc.LanguageSource: UNITY_EDITOR
        new List<string>(){ "I2.Loc.LanguageSource", "Spreadsheet_LocalFileName"},
        new List<string>(){ "I2.Loc.LanguageSource", "Spreadsheet_LocalCSVSeparator"},
        new List<string>(){ "I2.Loc.LanguageSource", "Spreadsheet_LocalCSVEncoding"},
        new List<string>(){ "I2.Loc.LanguageSource", "Spreadsheet_SpecializationAsRows"},
        new List<string>(){ "I2.Loc.LanguageSource", "Google_Password"},
        new List<string>(){ "I2.Loc.LanguageSource", "GoogleInEditorCheckFrequency"},
        // Particle2DUGUI: UNITY_EDITOR
        new List<string>(){ "Particle2DUGUI", "ReadConfig"},

        new List<string>(){ "UnityEngine.GUIStyleState", "scaledBackgrounds"},
        //new List<string>(){ "UnityEngine.GUIStyleState", "scaledBackgrounds"},
        new List<string>(){ "UnityEngine.ParticleSystemForceField", "FindAll"},
        new List<string>(){ "UnityEngine.Playables.PlayableGraph", "GetEditorName"},
        new List<string>(){ "UnityEngine.Rendering.RenderPipelineAsset", "terrainBrushPassIndex"},
        
    };
    #if UNITY_2018_1_OR_NEWER
    [BlackList]
    public static Func<MemberInfo, bool> MethodFilter = (memberInfo) =>
    {
        var type = memberInfo.DeclaringType;
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
        {
            if (memberInfo.Name == "GetEnumerator") return true;
        }
        if (typeof(System.Collections.ICollection).IsAssignableFrom(type))
        {
            if (type.Name == "IsSynchronized") return true;
            if (type.Name == "CopyTo") return true;
            if (type.Name == "SyncRoot") return true;
        }

        if (memberInfo.DeclaringType.IsGenericType)
        {
            if (memberInfo.MemberType == MemberTypes.Constructor)
            {
                return true;
            }

            if (memberInfo.DeclaringType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                if (memberInfo.MemberType == MemberTypes.Constructor)
                {
                    return true;
                }else if (memberInfo.MemberType == MemberTypes.Method)
                {
                    var methodInfo = memberInfo as MethodInfo;
                    if (memberInfo.Name == "OnDeserialization" || methodInfo.Name == "TryAdd" || methodInfo.Name == "Remove" && methodInfo.GetParameters().Length == 2)
                    {
                        return true;
                    }
                }
                else
                {
                    if (memberInfo.Name == "Comparer" || memberInfo.Name == "Keys" || memberInfo.Name == "Values")
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    };
#endif
}

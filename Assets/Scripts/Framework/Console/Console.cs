#define USE_CONSOLE
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
//using TSDKMessage;
using UnityEngine;
using XLua;

namespace CCM
{
    [LuaCallCSharp]
    public class Console : MonoBehaviour
    {
        public static bool IsOn = true;

        #region Inspector Settings  

        /// <summary>  
        /// The hotkey to show and hide the console window.  
        /// </summary>  
        public KeyCode toggleKey = KeyCode.BackQuote;
        public KeyCode toggleKey2 = KeyCode.Escape;

        /// <summary>  
        /// Whether to open the window by shaking the device (mobile-only).  
        /// </summary>  
        public bool shakeToOpen = true;

        /// <summary>  
        /// The (squared) acceleration above which the window should open.  
        /// </summary>  
        public float shakeAcceleration = 4f;

        /// <summary>  
        /// Whether to only keep a certain number of logs.  
        ///  
        /// Setting this can be helpful if memory usage is a concern.  
        /// </summary>  
        public bool restrictLogCount = true;

        /// <summary>  
        /// Number of logs to keep before removing old ones.  
        /// </summary>  
        public static int maxLogs = 1000;

        /// <summary>
        /// Allow the window to be dragged by its title bar.  
        /// </summary>
        public bool enableDrag = false;
        #endregion

        [StructLayout(LayoutKind.Sequential)]
        [LuaCallCSharp]
        public struct Log
        {
            public string message;
            public string stackTrace;
            public LogType type;
        }

        static readonly public List<Log> s_logs = new List<Log>();
        Vector2 m_scrollPosition = new Vector2();
        bool m_isCollapse;
        static bool m_isVisible;

        // Visual elements:  
        static readonly Dictionary<LogType, Color> s_logTypeColors = new Dictionary<LogType, Color>
        {
            {LogType.Assert,Color.white},
            {LogType.Error,Color.red},
            {LogType.Exception,Color.red},
            {LogType.Log,Color.white},
            {LogType.Warning,Color.yellow},
        };

        const string m_windowTitle = "Console";
        const int m_margin = 5;
        static readonly GUIContent s_clearLabel = new GUIContent("Clear", "Clear the contents of the console.");
        static readonly GUIContent s_collapseLabel = new GUIContent("Collapse", "Hide repeated messages.");

        readonly Rect m_titleBarRect = new Rect(0, 0, 10000, 20);
        Rect m_windowRect = new Rect(m_margin, m_margin, Screen.width - (m_margin * 2), Screen.height - (m_margin * 2));

        static Queue<string> s_cachedLog = new Queue<string>();
        static StringBuilder s_cachedLogStringBuilder = new StringBuilder(1000);

#if UNITY_EDITOR_WIN && !UNITY_ANDROID && !UNITY_IPHONE
        //private static ServerConsole s_windowsConsole = null;
#endif
        private static Console s_instance = null;
        public static Console GetInstance()
        {
            if (s_instance == null)
            {
                s_instance = Common.AddTSRGameObject("Console", "CCM.Console") as Console;

#if UNITY_EDITOR_WIN && !UNITY_ANDROID && !UNITY_IPHONE
                //s_windowsConsole = s_instance.gameObject.AddComponent<ServerConsole>();
#endif
            }

            return s_instance;
        }

#if USE_CONSOLE

        void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        void Update()
        {
            if (!IsOn)
            {
                return;
            }

            if (Input.GetKeyDown(toggleKey) || Input.GetKeyDown(toggleKey2))
            { 
                m_isVisible = !m_isVisible;
            }

            if (shakeToOpen && Input.acceleration.sqrMagnitude > shakeAcceleration)
            {
                m_isVisible = !m_isVisible;
            }
        }

        void OnGUI()
        {
            /*if (AppData.GetInstance().platformConfig != null && !AppData.GetInstance().platformConfig.IsDebug)
            {
                return;
            }*/

            if (!m_isVisible)
            {
                return;
            }

            m_windowRect = GUILayout.Window(3927, m_windowRect, DrawConsoleWindow, m_windowTitle);
        }

        /// <summary>  
        /// Displays a window that lists the recorded logs.  
        /// </summary>  
        /// <param name="windowID">Window ID.</param>  
        void DrawConsoleWindow(int windowID)
        {
            DrawLogsList();
            DrawToolbar();

            // Allow the window to be dragged by its title bar.  
            GUI.DragWindow(enableDrag ? m_titleBarRect : new Rect(0, 0, 0, 0));
        }


        /// <summary>  
        /// Displays a scrollable list of logs.  
        /// </summary>  
        void DrawLogsList()
        {
            GUI.skin.verticalScrollbar.fixedWidth = Screen.width * 0.05f;
            GUI.skin.verticalScrollbarThumb.fixedWidth = Screen.width * 0.05f;
            GUI.skin.verticalScrollbarThumb.fixedHeight = Screen.width * 0.05f;
            m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition);

            // Iterate through the recorded logs.  
            for (var i = 0; i < s_logs.Count; i++)
            {
                var log = s_logs[i];

                // Combine identical messages if collapse option is chosen.  
                if (m_isCollapse && i > 0)
                {
                    var previousMessage = s_logs[i - 1].message;

                    if (log.message == previousMessage)
                    {
                        continue;
                    }
                }

                GUI.skin.label.fontSize = 30;
                GUI.contentColor = s_logTypeColors[log.type];
                if ((log.type == LogType.Error) || (log.type == LogType.Exception))
                {
                    GUILayout.Label(log.message + "\n" + log.stackTrace, new GUILayoutOption[0]);
                }
                else
                {
                    GUILayout.Label(log.message, new GUILayoutOption[0]);
                }
            }

            GUILayout.EndScrollView();

            // Ensure GUI colour is reset before drawing other components.  
            GUI.contentColor = Color.white;
        }

        /// <summary>  
        /// Displays options for filtering and changing the logs list.  
        /// </summary>  
        void DrawToolbar()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(s_clearLabel))
            {
                s_logs.Clear();
            }

            m_isCollapse = GUILayout.Toggle(m_isCollapse, s_collapseLabel, GUILayout.ExpandWidth(false));

            GUILayout.EndHorizontal();
        }
#endif

        /// <summary>  
        /// Records a log from the log callback.  
        /// </summary>  
        /// <param name="message">Message.</param>  
        /// <param name="stackTrace">Trace of where the message came from.</param>  
        /// <param name="type">Type of message (log, error, exception, warning, assert).</param>  
        void HandleLog(string message, string stackTrace, LogType type)
        {
#if UNITY_EDITOR_WIN && !UNITY_ANDROID && !UNITY_IPHONE
           // if (s_windowsConsole != null)
           // {
            //    s_windowsConsole.HandleLog(message, stackTrace, type);
            //}
#endif

            //显示状态不加log
            if (m_isVisible)
            {
                return;
            }

            s_logs.Add(new Log
            {
                message = message,
                stackTrace = stackTrace,
                type = type,
            });

            m_scrollPosition.y = float.MaxValue;

            TrimExcessLogs();
        }

        /// <summary>  
        /// Removes old logs that exceed the maximum number allowed.  
        /// </summary>  
        void TrimExcessLogs()
        {
            if (!restrictLogCount)
            {
                return;
            }

            var amountToRemove = Mathf.Max(s_logs.Count - maxLogs, 0);

            if (amountToRemove == 0)
            {
                return;
            }

            s_logs.RemoveRange(0, amountToRemove);
        }

        public static void AddLog(string message, string stackTrace, LogType type, string moduleType = "")
        {
            //显示状态不加log
            if (m_isVisible)
            {
                return;
            }
            WriteToTSDK(message, stackTrace, type, moduleType);

            Console.GetInstance().HandleLog(message, stackTrace, type);
        }

        private static void WriteToTSDK(string message, string stackTrace, LogType type, string moduleType = "")
        {
            
            if ((message != null) && (message != string.Empty))
            {
                message = message.TrimEnd(new char[1]);
                int num = 2;
                switch (type)
                {
                    case LogType.Error:
                    case LogType.Assert:
                    case LogType.Warning:
                    case LogType.Exception:
                        num = 1;
                        message = message + stackTrace;
                        break;

                    case LogType.Log:
                        num = 2;
                        break;
                }
//                if (mCmd.logMessage == null)
//                {
//                    mCmd.logMessage = new byte[0x400];
//                    mCmd.moduleType = new byte[0x100];
//                }
//                mCmd.cmdId = 400;
//                mCmd.logLevel = num;
//                string item = message;
//                while (s_cachedLog.Count >= 150)
//                {
//                    s_cachedLog.Dequeue();
//                }
//                s_cachedLog.Enqueue(item);
//                byte[] bytes = Encoding.ASCII.GetBytes(item);
//                if (bytes.Length < 0x400)
//                {
//                    Buffer.BlockCopy(bytes, 0, mCmd.logMessage, 0, bytes.Length);
//                    mCmd.logMessage[bytes.Length] = 0;
//                }
//                else
//                {
//                    return;
//                }
//                byte[] src = Encoding.ASCII.GetBytes(moduleType);
//                if (src.Length < 0x100)
//                {
//                    Buffer.BlockCopy(src, 0, mCmd.moduleType, 0, src.Length);
//                    mCmd.moduleType[src.Length] = 0;
//                }
//                else
//                {
//                    return;
//                }
//                TSDKService.TSDKSend(mCmd);
            }
            
        }

        public static string CachedLogString
        {
            get
            {
                if (s_cachedLog.Count == 0)
                {
                    return string.Empty;
                }
                s_cachedLogStringBuilder.Length = 0;
                try
                {
                    s_cachedLogStringBuilder.Append("#### GameUin:");
                    //s_cachedLogStringBuilder.Append(PlayerLoginDataMgr.GetInstance().GetLoginDataUin().ToString());
                    s_cachedLogStringBuilder.Append("####\n");
                }
                catch (Exception)
                {
                }
                foreach (string str in s_cachedLog)
                {
                    string str2 = str.Trim(new char[1]);
                    s_cachedLogStringBuilder.Append(str2);
                    s_cachedLogStringBuilder.Append("\n");
                }
                return s_cachedLogStringBuilder.ToString();
            }
        }
    }
}
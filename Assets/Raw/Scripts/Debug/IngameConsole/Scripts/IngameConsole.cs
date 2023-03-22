//#if GAME_DEBUG

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IngameConsole : MonoBehaviour
{
	struct LogMessage : IEquatable<LogMessage>
	{
		public string Message
		{
			get; private set;
		}
		public string StackTrace
		{
			get; private set;
		}
		public LogType Type
		{
			get; private set;
		}

		public LogMessage(string message, string stackTrace, LogType type) : this()
		{
			Message = message;
			StackTrace = stackTrace;
			Type = type;
		}

		public override bool Equals(object obj)
		{
			if (obj == null) {
				return false;
			}

			if (obj.GetType() != typeof(LogMessage)) {
				return false;
			}

			return Equals((LogMessage)obj);
		}

		public override int GetHashCode()
		{
			return (Message == null ? 0 : Message.GetHashCode()) ^
				(StackTrace == null ? 0 : StackTrace.GetHashCode()) ^
					Type.GetHashCode();
		}

		public bool Equals(LogMessage other)
		{
			return Message == other.Message && StackTrace == other.StackTrace && Type == other.Type;
		}
	}

	private Vector2 m_scrollPositionText;
	private Vector2 m_scrollPositionStackTrace;
	private GUIContent m_clearLabel = new GUIContent("Clear", "Clear console");
	private GUIContent m_hideLabel = new GUIContent("Hide", "Hide console");
	private GUIContent m_sizeLabel = new GUIContent("Half", "Resize console");
	private GUIContent m_hideStackTraceLabel = new GUIContent("Hide StackTrace", "Hide stack trace");

	private const float Margin = 42.0f;
	private const float NativeWidth = 1920.0f;
	private const float NativeHeight = 1080.0f;

	public GUISkin m_skin;
	public KeyCode m_activateKey = KeyCode.Backspace;
	public bool m_autoShowOnError = true;

	private static Color[] LogColors = new Color[5];

	private List<LogMessage> m_logEntries = new List<LogMessage>();
	private string m_selectedStackTrace = null;
	private bool m_fullSize = true;
	private bool m_stackTraceVisible;
	private Vector2 m_startPoint = Vector2.zero;
	private bool m_doubleSwipeDrag;
	private bool m_isDraggingContent;

	private static bool s_initialized;
	private static GameObject s_instance;

	public bool IsVisible
	{
		get; set;
	}

	private LogMessage m_exceptionMessage;

	public void Awake()
	{
		if (!s_initialized) {
			Application.logMessageReceived += LogHandler;
			s_instance = this.gameObject;
			DontDestroyOnLoad(s_instance);
			s_initialized = true;
		} else {
			if (s_instance != this.gameObject) {
				Destroy(this.gameObject);
			}
		}

		LogColors[0] = Color.red; // Error
		LogColors[1] = Color.white; // Assert
		LogColors[2] = Color.yellow; // Warning
		LogColors[3] = Color.white; // Log
		LogColors[4] = Color.red; // Exception
	}

	private void LogHandler(string condition, string stackTrace, LogType type)
	{
		if (AddLogEntry(condition, stackTrace, type)) {
			if (m_autoShowOnError && (type == LogType.Exception || type == LogType.Error)) {
				if (!IsVisible) {
					m_scrollPositionText.y = float.MaxValue;
					IsVisible = true;
				}
			}
		}
	}

	public void Update()
	{
		if (Input.GetKeyDown(m_activateKey)) {
			IsVisible = !IsVisible;
		}

		if (Input.touchCount == 1) {
			var t0 = Input.GetTouch(0);
			var windowRect = GetWindowRect();
			var pos = new Vector2(t0.position.x, Screen.height - t0.position.y);

			if (!windowRect.Contains(pos)) {
				return;
			}

			if (t0.phase == TouchPhase.Moved) {
				m_scrollPositionText.y += t0.deltaPosition.y * (Time.deltaTime / t0.deltaTime);
				m_isDraggingContent = true;
			}
			if (t0.phase == TouchPhase.Ended) {
				m_isDraggingContent = false;
			}
		} else if (Input.touchCount == 2) // Handle double swipe show/hide
		  {
			var t0 = Input.GetTouch(0);
			var t1 = Input.GetTouch(1);
			var point = Vector2.Lerp(t0.position, t1.position, 0.5f);

			if (TouchBegan(t0, t1)) {
				m_startPoint = point;
				m_doubleSwipeDrag = true;
			}

			var diff = m_startPoint - point;

			if (diff.y > 300 && m_doubleSwipeDrag) {
				IsVisible = !IsVisible;
				m_doubleSwipeDrag = false;
			}
		}
	}

	private bool TouchBegan(Touch t0, Touch t1)
	{
		return (t0.phase == TouchPhase.Began && t1.phase == TouchPhase.Began) ||
				(t0.phase == TouchPhase.Stationary && t1.phase == TouchPhase.Began) ||
				(t0.phase == TouchPhase.Moved && t1.phase == TouchPhase.Began);
	}

	public bool AddLogEntry(string message, string stackTrace, LogType type)
	{
		if (type == LogType.Exception || type == LogType.Error) {
			if (message.IndexOf("Live2D") < 0) {
				m_logEntries.Add(new LogMessage(message, stackTrace, type));
				return true;
			}
		}
		return false;
	}

	public void OnGUI()
	{
		if (!IsVisible) {
			return;
		}

		DrawErrorConsole();
	}

	private void DrawErrorConsole()
	{
		GUI.skin = m_skin;

		float sx = Screen.width / NativeWidth;
		float sy = Screen.height / NativeHeight;

		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(sx, sy, 1));

		Rect windowRect = GetWindowRect();

		GUILayout.Window(12, windowRect, ConsoleWindowFunc, "Console");
	}

	private Rect GetWindowRect()
	{
		if (m_fullSize) {
			return new Rect(Margin, Margin, NativeWidth - Margin * 2, NativeHeight - Margin * 2);
		}

		// Half screen size
		return new Rect(Margin, NativeHeight * 0.5f, NativeWidth - Margin * 2, NativeHeight * 0.5f - Margin);
	}

	private void ConsoleWindowFunc(int windowId)
	{
		GUILayout.BeginHorizontal();

		if (GUILayout.Button(m_clearLabel, GUILayout.MaxWidth(200))) {
			m_logEntries.Clear();
			m_selectedStackTrace = null;
		}

		if (GUILayout.Button(m_hideLabel, GUILayout.MaxWidth(200))) {
			IsVisible = false;
		}

		if (GUILayout.Button(m_sizeLabel, GUILayout.MaxWidth(200))) {
			m_fullSize = !m_fullSize;
			if (m_fullSize) {
				m_sizeLabel.text = "Half";
			} else {
				m_sizeLabel.text = "Full";
			}
		}

		if (GUILayout.Button(m_hideStackTraceLabel, GUILayout.MaxWidth(380))) {
			m_stackTraceVisible = false;
		}

		GUILayout.EndHorizontal();

		m_scrollPositionText = GUILayout.BeginScrollView(m_scrollPositionText);

		foreach (var entry in m_logEntries) {
			Color currentColor = GUI.contentColor;
			GUI.contentColor = LogColors[(int)entry.Type];
			if (GUILayout.Button(entry.Message, m_skin.customStyles[0]) && !m_isDraggingContent) {
				m_selectedStackTrace = entry.StackTrace;
				m_stackTraceVisible = true;
			}
			GUI.contentColor = currentColor;
		}

		GUILayout.EndScrollView();
		if (m_stackTraceVisible && !string.IsNullOrEmpty(m_selectedStackTrace)) {
			m_scrollPositionStackTrace = GUILayout.BeginScrollView(m_scrollPositionStackTrace, GUILayout.Height(250));
			GUILayout.TextArea(m_selectedStackTrace);

			GUILayout.EndScrollView();
		}
	}
}

//#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[XLua.LuaCallCSharp]
public static partial class SparkHelper
{
	public const string version = "3.3.1";

	public delegate void OnScreenSizeChanged(int width, int height);

	/// <summary>
	/// 屏幕发生变化的代理，只在UnityEditor下生效。
	/// </summary>
	public static OnScreenSizeChanged onScreenSizeChanged;
	/// <summary>
	/// 改变编辑器分辨率，只在UnityEditor下生效。
	/// </summary>
	public static Action<int, int> onSwitchGameView;

	public static Action onReload;
	internal static Action onInternalReload;

	public static Action onAppQuit;
	public static Action<bool> onAppPause;
	public static Action<bool> onAppFocus;

	/// <summary>
	/// 设备返回键事件，比如Android、WP8等支持返回键的设备
	/// 参数指定该事件是否被UI截获了
	/// </summary>
	public static Action<bool> onBackButtonPressed;

	private static GameObject m_SparkObject;
	private static SparkBehaviour m_SparkBehaviour;

	public const string SparkAssetExtension = ".awb";
	public const string SparkAssetIndexName = "SparkAssetIndex" + SparkAssetExtension;
	//public const string SparkUpdateIndexName = "SparkUpdateIndex";
	public const string SparkUnityShadersName = "SparkUnityShaders" + SparkAssetExtension;

	static SparkHelper()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
			return;
#endif

		System.IO.Directory.CreateDirectory(dataPath);
		System.IO.Directory.CreateDirectory(assetsPath);

		// init #obj#
		GameObject prefab = Resources.Load("__SPARK__", typeof(GameObject)) as GameObject;
		if (prefab != null) {
			m_SparkObject = GameObject.Instantiate(prefab) as GameObject;
			m_SparkObject.name = "__SPARK__";
		} else {
			m_SparkObject = new GameObject("__SPARK__");
		}
		GameObject.DontDestroyOnLoad(m_SparkObject);

		// Add Module
		m_SparkBehaviour = CreateModule<SparkBehaviour>();
	}

	//static private string m_PersistentDataPath = string.Empty;
	public static string persistentDataPath = Application.persistentDataPath;

	public static string dataPath = persistentDataPath + "/data";

	public static string assetsPath = persistentDataPath + "/upd";

	public static string staticAssetsPath = Application.streamingAssetsPath + "/StaticAssets";

	public static string staticAssetsIndexPath = staticAssetsPath + "/PkgAssets.dat";

	/// <summary>
	/// 触发reload事件，外部可根据此事件清除相关状态
	/// </summary>
	public static void Reload()
	{
		Spark.Scheduler.Timeout(() => {
			onInternalReload?.Invoke();
			onReload?.Invoke();
		});
	}

	internal static T CreateModule<T>() where T : MonoBehaviour
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
			return null;
#endif
		return m_SparkObject.AddComponent<T>();
	}

	/// <summary>
	/// 设置所有子节点的Layer
	/// </summary>
	public static void SetLayer(GameObject go, int layer)
	{
		foreach (Transform t in go.GetComponentsInChildren<Transform>(true)) {
			t.gameObject.layer = layer;
		}
	}

	private static List<Graphic> s_TempComponents;
	private static Material m_UIDefaultGrayMaterial;
	private static Material m_UIDefaultGrayETC1Material;

	public static void SetGray(GameObject root, bool gray)
	{
		if (s_TempComponents == null) {
			s_TempComponents = new List<Graphic>();
		}
		root.GetComponentsInChildren<Graphic>(true, s_TempComponents);
		if (gray) {
			if (m_UIDefaultGrayMaterial == null) {
				m_UIDefaultGrayMaterial = Resources.Load<Material>("Materials/UIDefaultGray");
				m_UIDefaultGrayETC1Material = Resources.Load<Material>("Materials/UIDefaultGrayETC1");
			}
			foreach (var graphic in s_TempComponents) {
				var storage = graphic.GetComponent<MaterialStorage>();
				if (storage != null)
					continue;
				var mat = graphic.material;
				if (mat != null && mat.shader != null) {
					var flag = false;
					var shaderName = mat.shader.name;
					if (shaderName == "UI/Default") {
						flag = true;
						graphic.material = m_UIDefaultGrayMaterial;
					} else if (shaderName == "UI/DefaultETC1") {
						flag = true;
						graphic.material = m_UIDefaultGrayETC1Material;
					}
					if (flag) {
						storage = graphic.gameObject.AddComponent<MaterialStorage>();
						storage.material = mat;
					}
				}
			}
		} else {
			foreach (var graphic in s_TempComponents) {
				var storage = graphic.GetComponent<MaterialStorage>();
				if (storage != null) {
					graphic.material = storage.material;
					// 必须用DestroyImmediate，否则Instantiate该对象，MaterialStorage对象还存在。
					Component.DestroyImmediate(storage);
				}
			}
		}
		s_TempComponents.Clear();
	}

	public static void CaptureScreenshot(string filename)
	{
		ScreenCapture.CaptureScreenshot(filename);
	}
	
	/// <summary>
	/// 启动协程
	/// </summary>
	public static Coroutine StartCoroutine(IEnumerator routine)
	{
		if (m_SparkBehaviour != null) {
			return m_SparkBehaviour.StartCoroutine(routine);
		}
		return null;
	}

	/// <summary>
	/// 终止协程
	/// </summary>
	public static void StopCoroutine(IEnumerator routine)
	{
		if (m_SparkBehaviour != null) {
			m_SparkBehaviour.StopCoroutine(routine);
		}
	}
	
	public static bool ObjectValidCheck(object obj)
	{
		return (!(obj is UnityEngine.Object)) ||  ((obj as UnityEngine.Object) != null);
	}

	public static void SwitchGameView(int width, int height)
	{
		onSwitchGameView?.Invoke(width, height);
	}

#if UNITY_EDITOR
	private static Dictionary<Material, Material> s_SharedMaterials = new Dictionary<Material, Material>();
	[XLua.BlackList]
	public static Material GetStoredSharedMaterial(Material material)
	{
		Material mat = null;
		if (material != null) {
			if (!s_SharedMaterials.TryGetValue(material, out mat)) {
				mat = new Material(material);
				s_SharedMaterials[material] = mat;
			}
		}
		return mat;
	}
#endif

	/// <summary>
	/// MaterialStorage
	/// </summary>
	class MaterialStorage : MonoBehaviour
	{
		public Material material;

		void Awake()
		{
			hideFlags = HideFlags.HideInInspector;
		}
	}

	/// <summary>
	/// SparkBehaviour
	/// </summary>
	class SparkBehaviour : MonoBehaviour
	{
#if UNITY_EDITOR
		private int m_ScreenWidth;
		private int m_ScreenHeight;

		void Awake()
		{
			m_ScreenWidth = Screen.width;
			m_ScreenHeight = Screen.height;
		}
#endif

#if UNITY_EDITOR || UNITY_ANDROID
		void Update()
		{
#if UNITY_EDITOR
			int width = Screen.width;
			int height = Screen.height;
			if (width != m_ScreenWidth || height != m_ScreenHeight) {
				m_ScreenWidth = width;
				m_ScreenHeight = height;
				if (onScreenSizeChanged != null) {
					onScreenSizeChanged(width, height);
				}
			}
#endif
			if (Input.GetKeyUp(KeyCode.Escape)) {
				var blocked = Spark.UIViewRoot.OnBackButtonPressed();
				if (onBackButtonPressed != null)
					onBackButtonPressed(blocked);
			}
		}
#endif

		void OnApplicationQuit()
		{
			if (onAppQuit != null)
				onAppQuit();
		}

		void OnApplicationPause(bool isPaused)
		{
			if (onAppPause != null)
				onAppPause(isPaused);
		}
		void OnApplicationFocus(bool isFocused)
		{
			if (onAppFocus != null)
				onAppFocus(isFocused);
		}
	}
}

namespace Spark
{
	public static partial class ExtensionMethod
	{
		public static string ToUpperFirst(this string str)
		{
			if (str == string.Empty)
				return str;
			return str.Substring(0, 1).ToUpper() + str.Substring(1);
		}
		public static string ToLowerFirst(this string str)
		{
			if (str == string.Empty)
				return str;
			return str.Substring(0, 1).ToLower() + str.Substring(1);
		}
	}
}
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spark
{
	[XLua.LuaCallCSharp]
	public static class SceneHelper
	{
		private static Action m_onComplete;
		static SceneHelper()
		{
			SceneManager.sceneLoaded += (Scene scene, LoadSceneMode sceneMod) => {
				SparkHelper.StartCoroutine(SetActiveSceneAsync(scene.name, m_onComplete));
			};
		}
		public static Scene GetActiveScene()
		{
			return SceneManager.GetActiveScene();
		}

		public static Scene CreateScene(string sceneName)
		{
			return SceneManager.CreateScene(sceneName);
		}

		[Obsolete("Use SceneHelper.LoadSceneAsync")]
		public static void LoadScene(string scenePath, Action onComplete)
		{
			LoadScene(scenePath, false, onComplete);
		}

		[Obsolete("Use SceneHelper.LoadSceneAsync")]
		public static void LoadScene(string scenePath, bool additive, Action onComplete)
		{
			var sceneName = Path.GetFileNameWithoutExtension(scenePath);
			if (!IsSceneLoaded(sceneName)) {
				CheckSceneOrLoadAsset(scenePath, sceneName);
				SceneManager.LoadScene(sceneName, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
			}
			SparkHelper.StartCoroutine(SetActiveSceneAsync(sceneName, onComplete));
		}

		private static IEnumerator SetActiveSceneAsync(string sceneName, Action onComplete)
		{
			// TODO: Because of lua, don't use WaitForEndOfFrame
			yield return null;
			SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
			onComplete?.Invoke();
		}

		public static void LoadSceneAsync(string scenePath, Action onComplete)
		{
			LoadSceneAsync(scenePath, false, onComplete);
		}

		public static void LoadSceneAsync(string scenePath, bool additive, Action onComplete)
		{
			m_onComplete = onComplete;
			var sceneName = Path.GetFileNameWithoutExtension(scenePath);
			if (!IsSceneLoaded(sceneName)) {
				CheckSceneOrLoadAsset(scenePath, sceneName);
				SparkHelper.StartCoroutine(LoadSceneAsync(sceneName, additive ? LoadSceneMode.Additive : LoadSceneMode.Single, onComplete));
			} else {
				SparkHelper.StartCoroutine(SetActiveSceneAsync(sceneName, onComplete));
			}
		}

		private static IEnumerator LoadSceneAsync(string sceneName, LoadSceneMode mode, Action onComplete)
		{
			var asyncOption = SceneManager.LoadSceneAsync(sceneName, mode);
			asyncOption.allowSceneActivation = true;
			while (!asyncOption.isDone)
			{
				yield return null;
			}
			//yield return SetActiveSceneAsync(sceneName, onComplete);
		}

		private static void CheckSceneOrLoadAsset(string scenePath, string sceneName)
		{
			Assets.LoadScene(scenePath.EndsWith(".unity") ? scenePath : (scenePath + ".unity"));
		}

#if UNITY_5_5_OR_NEWER
		[Obsolete("Use SceneHelper.UnloadSceneAsync")]
#endif
		public static bool UnloadScene(string sceneName)
		{
			var isActive = SceneManager.GetActiveScene().name == sceneName;

			if (SceneManager.UnloadScene(sceneName)) {
				if (isActive && SceneManager.sceneCount > 0) {
					SceneManager.SetActiveScene(SceneManager.GetSceneAt(SceneManager.sceneCount - 1));
				}
				return true;
			}
			return false;
		}

#if UNITY_5_5_OR_NEWER
		public static void UnloadSceneAsync(string sceneName, Action onComplete)
		{
			var isActive = SceneManager.GetActiveScene().name == sceneName;
			SparkHelper.StartCoroutine(UnloadSceneAsync(sceneName, isActive, onComplete));
		}
		private static IEnumerator UnloadSceneAsync(string sceneName, bool isActive, Action onComplete)
		{
			yield return SceneManager.UnloadSceneAsync(sceneName);
			if (isActive && SceneManager.sceneCount > 0)
			{
				SceneManager.SetActiveScene(SceneManager.GetSceneAt(SceneManager.sceneCount - 1));
			}

			onComplete?.Invoke();
		}
#endif

		public static bool IsSceneLoaded(string sceneName)
		{
			for (var i = SceneManager.sceneCount - 1; i >= 0; i--) {
				var scene = SceneManager.GetSceneAt(i);
				if (scene.name == sceneName || scene.path == sceneName) {
					return true;
				}
			}
			return false;
		}
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using Spark;
using I2.Loc;

namespace SparkEditor
{
	namespace UI
	{
		public abstract class ScriptGenerator
		{
			public string extension
			{
				get;
				protected set;
			}

			public abstract void Generate(string file, List<KeyValuePair<string, Type>> properties, List<UIViewExporter.UISubViewCollection> tableViews, List<UIViewExporter.UISubViewCollection> viewStacks, string prefabPath);
		}
	}

	static public class UIViewExporter
	{
		public class UISubViewCollection
		{
			public class View
			{
				public string name;
				public string identifier;
				public List<KeyValuePair<string, Type>> properties = new List<KeyValuePair<string, Type>>();
			}
			public string name;
			public string objectName;
			public Type typeName;
			public List<View> views = new List<View>();
		}

		static private List<Type> m_ComponentTypes = new List<Type>() {
			typeof(Spark.IControl),
			typeof(Button), typeof(Dropdown), typeof(InputField), typeof(Scrollbar), typeof(ScrollRect), typeof(Slider), typeof(Toggle), typeof(ToggleGroup)
		};
		static private List<Type> m_SpecificPropertyTypes = new List<Type>() {
			typeof(Spark.IComponent),
			typeof(CanvasGroup), typeof(Image), typeof(RawImage), typeof(Text), typeof(LayoutGroup), typeof(RectTransform), typeof(Transform)
		};

		//static private string m_ScriptPath;
		//static private string m_PrefabPath;
		//static private string m_RawPrefabPath;
		static private UI.ScriptGenerator m_ScriptGenerator;
		// containers
		static private List<Transform> m_ViewStack = new List<Transform>();
		static private List<Transform> m_TransformStack = new List<Transform>();
		static private Dictionary<Transform, List<string>> m_ViewPropertyNames = new Dictionary<Transform, List<string>>();
		static private Dictionary<Transform, List<string>> m_ViewExportedAssets = new Dictionary<Transform, List<string>>();
		static private Dictionary<Transform, List<KeyValuePair<Component, string>>> m_ViewComponents = new Dictionary<Transform, List<KeyValuePair<Component, string>>>();
		static private List<KeyValuePair<string, KeyValuePair<Transform, int>>> m_RestorePrefabs = new List<KeyValuePair<string, KeyValuePair<Transform, int>>>();
		static private Dictionary<Transform, List<UISubViewCollection>> m_ViewTableCollections = new Dictionary<Transform, List<UISubViewCollection>>();
		static private Dictionary<Transform, List<UISubViewCollection>> m_ViewStackCollections = new Dictionary<Transform, List<UISubViewCollection>>();

		static private Func<string, string> m_GetRawPrefabPath;
		static private Func<string, string> m_GetViewPath;
		static private Func<string, string> m_GetScriptPath;

		static public void Export(Func<string,string> getRawPrefabPath, Func<string, string> getViewPath, Func<string, string> getScriptPath, UI.ScriptGenerator generator)
        {
			UIViewRoot[] array = GameObject.FindObjectsOfType<UIViewRoot>();
			if (array.Length == 0)
			{
				return;
			}

			// reset
			m_ViewStack.Clear();
			m_TransformStack.Clear();
			m_ViewComponents.Clear();
			m_ViewPropertyNames.Clear();
			m_ViewExportedAssets.Clear();
			m_ViewTableCollections.Clear();
			m_ViewStackCollections.Clear();
			m_RestorePrefabs.Clear();

			//m_ScriptPath = scriptPath;
			//m_PrefabPath = prefabPath;
			//m_RawPrefabPath = rawPrefabPath;
			m_GetRawPrefabPath = getRawPrefabPath;
			m_GetViewPath = getViewPath;
			m_GetScriptPath = getScriptPath;
			m_ScriptGenerator = generator;

			// Find Top-Level's UIViewRoot
			List<UIViewRoot> list = new List<UIViewRoot>();
			for (int i = 0; i < array.Length; i++)
			{
				var root = array[i];
				if (list.Contains(root))
					continue;
				while (true)
				{
					var parent = root.transform.parent;
					if (parent == null)
						break;
					var comp = parent.GetComponentInParent<UIViewRoot>();
					if (comp == null)
						break;
					root = comp;
				}
				if (!list.Contains(root))
					list.Add(root);
			}

			// Parse UIView
			//AssetDatabase.StartAssetEditing();
			foreach (var root in list)
			{
				try
				{
					ParseViewRoot(root, false);
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
				finally
				{
					m_ViewStack.Clear();
					m_TransformStack.Clear();
					m_ViewComponents.Clear();
				}
			}
			//AssetDatabase.StopAssetEditing();
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			// Restore
			try
			{
				for (int i = 0; i < m_RestorePrefabs.Count; i++)
				{
					var entry = m_RestorePrefabs[i];
					var cloneObject = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(entry.Key), entry.Value.Key) as GameObject;
					
					// cloneObject.SetParent(entry.Value.Key, false);
					cloneObject.transform.SetSiblingIndex(entry.Value.Value);
				}
			}
			finally
			{
				m_ScriptGenerator = null;
				m_RestorePrefabs.Clear();
			}
		}

		static private string GetPrefabPath(string name)
		{
			return m_GetViewPath(name) + "/" + name.ToString() + ".prefab";
			//return m_PrefabPath + "/" + name.ToString() + ".prefab";
		}
		static private string GetPrefabRefPath(string name)
		{
			return m_GetViewPath(name) + "/References/" + name.ToString() + ".prefab";
			//return m_PrefabPath + "/References/" + name.ToString() + ".prefab";
		}
		static private string GetRawPrefabPath(string name)
		{
			return m_GetRawPrefabPath(name) + "/" + name + ".prefab";
			//return m_RawPrefabPath + "/" + name + ".prefab";
		}
		static private string GetScriptPath(string name)
        {
			return m_GetScriptPath(name) + "/Base" + name.Replace("+", "") + "." + m_ScriptGenerator.extension;
			//return m_ScriptPath + "/Base" + name.Replace("+", "") + "." + m_ScriptGenerator.extension;
		}
		static private List<KeyValuePair<string, Type>> ExtractProperties(Transform view, Transform root)
		{
			// parse components
			List<GameObject> ignoreList = new List<GameObject>();
			ParseViewComponents(view, root, root, ignoreList);

			for (int i = ignoreList.Count - 1; i >= 0; i--) {
				GameObject.DestroyImmediate(ignoreList[i]);
			}

			// Save Runtime-Prefab
			List<KeyValuePair<Component, string>> components;
			UIComponentCollection componentCollection = root.GetComponent<UIComponentCollection>();
			if (componentCollection == null) {
				componentCollection = root.gameObject.AddComponent<UIComponentCollection>();
			}
			FieldInfo fieldInfo = typeof(UIComponentCollection).GetField("components", BindingFlags.NonPublic | BindingFlags.Instance);
			var fieldValue = fieldInfo.GetValue(componentCollection) as List<Component>;
			fieldValue.Clear();
			if (m_ViewComponents.TryGetValue(root, out components)) {
				foreach (var kv in components) {
					fieldValue.Add(kv.Key);
				}
			}

			// generate script
			List<KeyValuePair<string, Type>> properties = new List<KeyValuePair<string, Type>>();
			if (components != null) {
				foreach (var kv in components) {
					properties.Add(new KeyValuePair<string, Type>(kv.Value, kv.Key.GetType()));//GetPropertyName(comp.name, view)
				}
			}
			return properties;
		}

		static private void PreprocessLocaleConfig(Transform t, bool isRoot, bool flip) {
            var cf = false;
            var cmt = "";
            var cst = "";
            var config = t.GetComponent<UILocaleConfig>();
			if (config == null) {
                if (isRoot) cf = true;
                else if (t.GetComponent<UIViewRoot>() != null) cf = false;
				else if (t.GetComponent<Text>() != null || t.GetComponent<Image>() != null || t.GetComponent<RawImage>() != null || t.GetComponent<UIImage>() != null) cf = false;
				else cf = flip;
            } else {
				// 忽略多语言
				if (config.ignore) return;
                cf = config.flip;
                cmt = config.mainTerm;
                cst = config.secondaryTerm;
                Component.DestroyImmediate(config);
            }

			// 添加翻转组件
			if (flip ^ cf) {
                var localize = t.gameObject.AddComponent<Localize>();
                localize.mLocalizeTargetName = "I2.Loc.LocalizeTarget_UnityUI_RectTransform";
                localize.FindTarget();
                localize.RectTranformFlip = true;
                localize.RectTranformCoordinate = false;
                flip = !flip;
            }

			// 文本
			if (t.GetComponent<Text>() != null) {
                var comp = t.GetComponent<Text>();
                var text = comp.text;
				if (!string.IsNullOrEmpty(text)) {
					// 从文本中获取Key，避免添加组件
					if (string.IsNullOrEmpty(cmt)) {
                        var match = new Regex("<T:([^>]+)>").Match(text);
						if (match != null && match.Captures.Count > 0) {
                            cmt = match.Groups[1].Value;
							// mTermSecondary
							match = new Regex("<ST:([^>]+)>").Match(text);
							if (match != null && match.Captures.Count > 0) {
								cst = match.Groups[1].Value;
							}
							// 清空
                            comp.text = "";
                        }
                    }
				}

				// 修复文本问题
                //comp.verticalOverflow = VerticalWrapMode.Overflow;

				// 添加多语言组件
                var localize = t.gameObject.AddComponent<Localize>();
                localize.mLocalizeTargetName = "I2.Loc.LocalizeTarget_UnityUI_Text";
                localize.mTerm = string.IsNullOrEmpty(cmt) ? "-" : cmt;
                localize.mTermSecondary = string.IsNullOrEmpty(cst) ? "drawguess_font" : cst;
            }

            isRoot = t.GetComponent<UIViewRoot>() ? true : false;
            // 子节点
            for(int i = t.childCount - 1; i >= 0; i--) {
                var child = t.GetChild(i);
                if (child.CompareTag(SparkMenu.UIIgnoreTagName))
                    continue;
                PreprocessLocaleConfig(child, isRoot && child.CompareTag(SparkMenu.UIViewTagName), flip);
            }
        }

		static private void ParseViewRoot(UIViewRoot root, bool isSubView)
		{
			for (int i = root.transform.childCount - 1; i >= 0; i--) {
				Transform child = root.transform.GetChild(i);
				if (isSubView) {
					if (child.CompareTag(SparkMenu.UIIgnoreTagName)) {
						GameObject.DestroyImmediate(child.gameObject);
						continue;
					}
				} else {
					if (!child.gameObject.activeSelf || !child.CompareTag(SparkMenu.UIViewTagName))
						continue;
				}

				// push view
				m_ViewStack.Add(child);
				m_TransformStack.Add(child);

				var viewName = GetCurrentViewName();

				if (!isSubView) {
					m_ViewExportedAssets[m_ViewStack[0]] = new List<string>();

					string userData = "";
					var rawPath = GetRawPrefabPath(viewName);
#if UNITY_2018_1_OR_NEWER
                    var prefab = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
                    var isPrefab = prefab != null && PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.Regular;
#else
                    var prefab = PrefabUtility.GetPrefabParent(child);
                    var isPrefab = prefab != null && PrefabUtility.GetPrefabType(child) == PrefabType.PrefabInstance;
#endif
                    if (isPrefab)
                    {
                        var assetPath = AssetDatabase.GetAssetPath(prefab);
                        if (assetPath.StartsWith(m_GetRawPrefabPath(viewName) + "/"))
                        {
                            rawPath = assetPath;
                            userData = AssetImporter.GetAtPath(rawPath).userData;
                            if (prefab.name != viewName)
                            {
                                AssetDatabase.DeleteAsset(rawPath);
                                rawPath = Path.GetDirectoryName(rawPath) + "/" + viewName + ".prefab";
                            }
                        }
                    }

                    // Save Raw-Prefab
                    m_ViewExportedAssets[m_ViewStack[0]].Add(rawPath);
					SparkEditorHelper.CreateOrReplacePrefab(child.gameObject, rawPath);

					var importer = AssetImporter.GetAtPath(rawPath);
					importer.userData = userData;
					importer.SaveAndReimport();

					// Restore
					m_RestorePrefabs.Add(new KeyValuePair<string, KeyValuePair<Transform, int>>(rawPath, new KeyValuePair<Transform, int>(child.parent, child.GetSiblingIndex())));

#if UNITY_2018_1_OR_NEWER
                    PrefabUtility.UnpackPrefabInstance(child.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
#endif

                    // Resettable
                    foreach (var resettable in child.GetComponentsInChildren<IResettable>(true)) {
						resettable.Reset();
					}

					// 预处理多语言，只有未挂载多语言组件的才处理
					if (child.GetComponentInChildren<Localize>() == null) {
						PreprocessLocaleConfig(child, true, false);
					} else {
						// 修复多语言组件没有挂载字体的bug
						foreach(var t in child.GetComponentsInChildren<Text>(true)) {
                            Localize localize = null;
                            var localizes = t.GetComponents<Localize>();
							foreach(var loc in localizes) {
								if (loc.mLocalizeTargetName == "I2.Loc.LocalizeTarget_UnityUI_Text"){
                                    localize = loc;
                                    break;
                                }
							}
							if (localize == null) {
                                localize = t.gameObject.AddComponent<Localize>();
                                localize.mTerm = "-";
                                localize.mLocalizeTargetName = "I2.Loc.LocalizeTarget_UnityUI_Text";
                            }
							if (string.IsNullOrEmpty(localize.mTermSecondary) || localize.mTermSecondary == "-") {
								localize.mTermSecondary = "drawguess_font";
							}
						}
					}
				}

                List<KeyValuePair<string, Type>> properties = ExtractProperties(child, child);

				if (child.CompareTag(SparkMenu.UIViewTagName)) {
					child.tag = "Untagged";
				}
				var prefabPath = GetPrefabPath(viewName);
				m_ViewExportedAssets[m_ViewStack[0]].Add(prefabPath);
				SparkEditorHelper.CreateOrReplacePrefab(child.gameObject, prefabPath);

				string scriptPath = GetScriptPath(viewName);
				m_ViewExportedAssets[m_ViewStack[0]].Add(scriptPath);
				m_ScriptGenerator.Generate(scriptPath, properties, m_ViewTableCollections.ContainsKey(child) ? m_ViewTableCollections[child] : null, m_ViewStackCollections.ContainsKey(child) ? m_ViewStackCollections[child] : null, SparkEditorHelper.GetAssetRelativePath(GetPrefabPath(viewName)));

				if (!isSubView) {
					var assets = m_ViewExportedAssets[m_ViewStack[0]];
					var importer = AssetImporter.GetAtPath(assets[0]);
					string userData = importer.userData;
					if (!string.IsNullOrEmpty(userData)) {
						Array.ForEach(userData.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries), (s) => {
							if (!assets.Contains(s)) {
								if (File.Exists(s))
									AssetDatabase.DeleteAsset(s);
							}
						});
					}
					importer.userData = string.Join(";", assets.ToArray());
					importer.SaveAndReimport();
				}

				// pop view
				m_ViewTableCollections.Remove(child);
				m_ViewStackCollections.Remove(child);
				m_TransformStack.Remove(child);
				m_ViewStack.Remove(child);
				GameObject.DestroyImmediate(child.gameObject);
			}
		}

		static private void ParseUIViewStack(UIViewStack viewStack)
		{
			Transform view = m_ViewStack[m_ViewStack.Count - 1];
			List<UISubViewCollection> collections;
			if (!m_ViewStackCollections.TryGetValue(view, out collections)) {
				collections = new List<UISubViewCollection>();
				m_ViewStackCollections[view] = collections;
			}
			UISubViewCollection collection = new UISubViewCollection() { name = GetPropertyName(viewStack, m_TransformStack[m_TransformStack.Count - 1]), typeName = typeof(UIViewStack) };
			while (viewStack.transform.childCount > 0) {
				var child = viewStack.transform.GetChild(0);
				if (!child.CompareTag(SparkMenu.UIIgnoreTagName)) {
					m_TransformStack.Add(child);
					var prefabPath = GetPrefabPath(GetCurrentViewName());
					collection.views.Add(new UISubViewCollection.View() {
						name = child.gameObject.name,
						identifier = prefabPath,
						properties = ExtractProperties(view, child)
					});
					m_ViewExportedAssets[m_ViewStack[0]].Add(prefabPath);
					SparkEditorHelper.CreateOrReplacePrefab(child.gameObject, prefabPath);
					viewStack.Push(child.gameObject.name, SparkEditorHelper.GetAssetRelativePath(prefabPath));
					m_TransformStack.Remove(child);
				}
				GameObject.DestroyImmediate(child.gameObject);
			}
			if (collection.views.Count > 0) {
				collections.Add(collection);
			}
		}

		static private void ParseUITableView(UITableView tableView)
		{
			Transform view = m_ViewStack[m_ViewStack.Count - 1];
			List<UISubViewCollection> collections;
			if (!m_ViewTableCollections.TryGetValue(view, out collections))
			{
				collections = new List<UISubViewCollection>();
				m_ViewTableCollections[view] = collections;
			}
			FieldInfo cellList = typeof(UITableView).GetField("m_CellList", BindingFlags.Instance | BindingFlags.NonPublic);
			List<UITableViewCell> cells = cellList.GetValue(tableView) as List<UITableViewCell>;
			if (cells != null)
			{
				UISubViewCollection collection = new UISubViewCollection() { objectName = tableView.name, name = GetPropertyName(tableView, m_TransformStack[m_TransformStack.Count - 1]), typeName = typeof(UITableView) }; // .gameObject.name.ToUpperFirst()
				for (int i = 0; i < cells.Count; i++)
				{
					var cell = cells[i];
					collection.views.Add(new UISubViewCollection.View()
					{
						//name = cell.gameObject.name.ToUpperFirst(),
						identifier = cell.identifier,
						properties = ExtractProperties(cell.transform, cell.transform)
					});

					var prefabPath = GetPrefabRefPath(GetCurrentViewName() + "+" + tableView.name.ToUpperFirst() + "+Cell" + (string.IsNullOrEmpty(cell.identifier) ? "" : "_" + cell.identifier));
					m_ViewExportedAssets[m_ViewStack[0]].Add(prefabPath);
					SparkEditorHelper.CreateOrReplacePrefab(cell.gameObject, prefabPath);
					cells[i] = AssetDatabase.LoadAssetAtPath<UITableViewCell>(prefabPath);
					GameObject.DestroyImmediate(cell.gameObject);
				}
				collections.Add(collection);
			}
		}

		static private void ParseViewComponents(Transform view, Transform root, Transform node, List<GameObject> ignoreList)
		{
			Component comp = null;
			bool found = false, parseChildren = true;
			do {
				if (node.CompareTag(SparkMenu.UIIgnoreTagName)) {
					found = false;
					parseChildren = false;
					ignoreList.Add(node.gameObject);
					break;
				}

				comp = node.GetComponent<UIViewRoot>();
				if (comp != null) {
					found = true;
					parseChildren = false;
					ParseViewRoot((UIViewRoot)comp, true);
					break;
				}

				comp = node.GetComponent<UIViewStack>();
				if (comp != null) {
					found = false;
					parseChildren = false;
					AddComponent(view, root, comp);
					ParseUIViewStack((UIViewStack)comp);
					break;
				}

				comp = node.GetComponent<UITableView>();
				if (comp != null)
				{
					found = false;
					parseChildren = false;
					AddComponent(view, root, comp);
					ParseUITableView((UITableView)comp);
					break;
				}

				comp = node.GetComponent<UIComponentGroup>();
				if (comp != null)
                {
					found = true;
					parseChildren = false;
					break;
                }

				for (int i = 0; i < m_ComponentTypes.Count; i++) {
					comp = node.GetComponent(m_ComponentTypes[i]);
					if (comp != null) {
						found = true;
						break;
					}
				}

				if (!found && node.CompareTag(SparkMenu.UIPropertyTagName)) {
					for (int i = 0; i < m_SpecificPropertyTypes.Count; i++) {
						comp = node.GetComponent(m_SpecificPropertyTypes[i]);
						if (comp != null) {
							found = true;
							break;
						}
					}
				}
			} while (false);

			if (found) {
				AddComponent(view, root, comp);
			}

			if (parseChildren) {
				foreach (Transform child in node) {
					ParseViewComponents(view, root, child, ignoreList);
				}
			}
		}

		static private string GetCurrentViewName()
		{
			var viewName = new StringBuilder();
			m_TransformStack.ForEach((t) => {
				if (viewName.Length != 0) {
					viewName.Append("+");
				}
				viewName.Append(t.name.ToUpperFirst());
			});
			return viewName.ToString();
		}

		static private void AddComponent(Transform view, Transform root, Component comp)
		{
			List<KeyValuePair<Component, string>> components;
			if (!m_ViewComponents.TryGetValue(root, out components)) {
				components = new List<KeyValuePair<Component, string>>();
				m_ViewComponents[root] = components;
			}
			components.Add(new KeyValuePair<Component, string>(comp, GetPropertyName(comp.name, view)));
		}

		static private string GetPropertyName(Component comp, Transform root)
		{
			List<KeyValuePair<Component, string>> components;
			if (m_ViewComponents.TryGetValue(root, out components)) {
				foreach (var kv in components) {
					if (kv.Key == comp) {
						return kv.Value;
					}
				}
			}
			return string.Empty;
		}

		static private string GetPropertyName(string name, Transform view)
		{
			string propName = Regex.Replace(name, "[^_0-9a-zA-Z]", "").ToUpperFirst();
			List<string> propertyNames;
			if (!m_ViewPropertyNames.TryGetValue(view, out propertyNames)) {
				propertyNames = new List<string>() { propName };
				m_ViewPropertyNames[view] = propertyNames;
			} else {
				int i = 1;
				name = propName;
				while (propertyNames.Contains(propName)) {
					propName = name + (i++);
				}
				propertyNames.Add(propName);
			}
			return propName;
		}

	}
}
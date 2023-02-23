using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.IO;
using Spark;
using UnityEditor.Build.Reporting;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SparkEditor
{
    [InitializeOnLoad]
    public static class SparkMenu
    {
        public static string UIViewTagName = "UIView";
        public static string UIIgnoreTagName = "UIIgnore";
        public static string UIPropertyTagName = "UIProperty";

        static SparkMenu()
        {
            // init tags
            TagHelper.RemoveTags("SparkView", "SparkProperty");
            TagHelper.AddTags(UIViewTagName, UIPropertyTagName, UIIgnoreTagName);

            // Create SparkAssets Folder.
            if (!Directory.Exists(SparkEditorTools.assetsPath))
            {
                Directory.CreateDirectory(SparkEditorTools.assetsPath);
                AssetDatabase.Refresh();
            }

            // EditorApplication.hierarchyWindowChanged

            EditorApplication.hierarchyChanged += delegate()
            {
                GameObject go = Selection.activeGameObject;
                if (go != null)
                {
                    SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        Canvas canvas = go.GetComponentInParent<Canvas>();
                        if (canvas != null && canvas.gameObject != go)
                        {
                            go.AddComponent<RectTransform>();
                            go.layer = canvas.gameObject.layer;
                            UnityEngine.UI.Image image = go.AddComponent<UnityEngine.UI.Image>();
                            Sprite sprite = renderer.sprite;
                            image.sprite = sprite;
                            image.rectTransform.localPosition = Vector3.zero;
                            image.rectTransform.localScale = Vector3.one;
                            if (sprite.border == Vector4.zero)
                            {
                                image.type = UnityEngine.UI.Image.Type.Simple;
                                image.SetNativeSize();
                            }
                            else
                            {
                                image.type = UnityEngine.UI.Image.Type.Sliced;
                                image.fillCenter = true;
                                image.rectTransform.sizeDelta = sprite.rect.max - sprite.rect.min;
                            }

                            UnityEngine.Object.DestroyImmediate(renderer);
                        }
                    }
                }
            };

            List<KeyCode> hotkeys = new List<KeyCode>()
            {
                KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow
            };
            // SceneView.onSceneGUIDelegate
            SceneView.duringSceneGui += delegate(SceneView sceneView)
            {
                if (UnityEditor.Tools.current == Tool.Rect)
                {
                    Event evt = Event.current;
                    if (evt != null && evt.type == EventType.KeyDown)
                    {
                        if (!hotkeys.Contains(evt.keyCode))
                            return;

                        GameObject[] list = Selection.gameObjects;
                        if (list == null || list.Length == 0)
                            return;

                        foreach (GameObject go in list)
                        {
                            bool valid = true;
                            Transform parent = go.transform;
                            while (valid && (parent = parent.parent) != null)
                            {
                                GameObject obj = parent.gameObject;
                                for (int i = 0; i < list.Length; i++)
                                {
                                    if (list[i] == obj)
                                    {
                                        valid = false;
                                        break;
                                    }
                                }
                            }

                            if (valid)
                            {
                                Vector3 position = go.transform.localPosition;
                                Vector3 offset = Vector3.zero;
                                if (evt.keyCode == KeyCode.LeftArrow)
                                {
                                    offset.x = -1;
                                }
                                else if (evt.keyCode == KeyCode.RightArrow)
                                {
                                    offset.x = 1;
                                }
                                else if (evt.keyCode == KeyCode.UpArrow)
                                {
                                    offset.y = 1;
                                }
                                else if (evt.keyCode == KeyCode.DownArrow)
                                {
                                    offset.y = -1;
                                }

                                offset = offset * (evt.shift ? 10 : (evt.control ? 5 : 1));
                                go.transform.localPosition = position + offset;
                            }
                        }

                        evt.Use();
                    }
                }
            };

            // other
            //Debug.Log(string.Join(",", EditorUserBuildSettings.activeScriptCompilationDefines));
        }

        [MenuItem("Spark/Create UIView", false, 0)]
        [MenuItem("GameObject/UI/Spark/UIView", false, 1969)]
        private static void CreateUIView(MenuCommand command)
        {
            Spark.UIViewRoot root = command.context == null ? null : ((GameObject) command.context).GetComponent<Spark.UIViewRoot>();
            if (root == null)
            {
                GameObject go;
                if (command.context == null)
                {
                    go = GetOrCreateCanvasGameObject();
                }
                else
                {
                    go = (GameObject) command.context;
                }

                root = go.GetComponent<Spark.UIViewRoot>();
                if (root == null)
                    root = go.AddComponent<Spark.UIViewRoot>();
            }

            GameObject view = new GameObject("NewUIView", typeof(RectTransform), typeof(Spark.UIViewSettings));
            view.layer = LayerMask.NameToLayer("UI");
            view.tag = UIViewTagName;
            string uniqueName = GameObjectUtility.GetUniqueNameForSibling(root.transform, view.name);
            view.name = uniqueName;
            Undo.RegisterCreatedObjectUndo(view, "Create " + view.name);
            Undo.SetTransformParent(view.transform, root.transform, "Parent " + view.name);
            GameObjectUtility.SetParentAndAlign(view, root.gameObject);

            RectTransform t = view.GetComponent<RectTransform>();
            t.offsetMax = t.offsetMin = t.anchorMin = Vector2.zero;
            t.anchorMax = Vector2.one;
            t.pivot = new Vector2(0.5f, 0.5f);

            Selection.activeGameObject = view;
        }

        #region Helper methods

        private static void SetPositionVisibleinSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
        {
            // Find the best scene view
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null && SceneView.sceneViews.Count > 0)
                sceneView = SceneView.sceneViews[0] as SceneView;

            // Couldn't find a SceneView. Don't set position.
            if (sceneView == null || sceneView.camera == null)
                return;

            // Create world space Plane from canvas position.
            Vector2 localPlanePosition;
            Camera camera = sceneView.camera;
            Vector3 position = Vector3.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera,
                out localPlanePosition))
            {
                // Adjust for canvas pivot
                localPlanePosition.x = localPlanePosition.x + canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
                localPlanePosition.y = localPlanePosition.y + canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;

                localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRTransform.sizeDelta.x);
                localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRTransform.sizeDelta.y);

                // Adjust for anchoring
                position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
                position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;

                Vector3 minLocalPosition;
                minLocalPosition.x = canvasRTransform.sizeDelta.x * (0 - canvasRTransform.pivot.x) + itemTransform.sizeDelta.x * itemTransform.pivot.x;
                minLocalPosition.y = canvasRTransform.sizeDelta.y * (0 - canvasRTransform.pivot.y) + itemTransform.sizeDelta.y * itemTransform.pivot.y;

                Vector3 maxLocalPosition;
                maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1 - canvasRTransform.pivot.x) - itemTransform.sizeDelta.x * itemTransform.pivot.x;
                maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1 - canvasRTransform.pivot.y) - itemTransform.sizeDelta.y * itemTransform.pivot.y;

                position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
                position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
            }

            itemTransform.anchoredPosition = position;
            itemTransform.localRotation = Quaternion.identity;
            itemTransform.localScale = Vector3.one;
        }

        private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                parent = GetOrCreateCanvasGameObject();
            }

            string uniqueName = GameObjectUtility.GetUniqueNameForSibling(parent.transform, element.name);
            element.name = uniqueName;
            Undo.RegisterCreatedObjectUndo(element, "Create " + element.name);
            Undo.SetTransformParent(element.transform, parent.transform, "Parent " + element.name);
            GameObjectUtility.SetParentAndAlign(element, parent);
            if (parent != menuCommand.context) // not a context click, so center in sceneview
                SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());

            Selection.activeGameObject = element;
        }

        public static GameObject CreateNewUI()
        {
            // Root for the UI
            var root = new GameObject("Canvas");
            root.layer = LayerMask.NameToLayer("UI");
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);

            // if there is no event system add one...
            CreateEventSystem(false, null);
            return root;
        }

        private static void CreateEventSystem(bool select, GameObject parent)
        {
            var esys = UnityEngine.Object.FindObjectOfType<EventSystem>();
            if (esys == null)
            {
                var eventSystem = new GameObject("EventSystem");
                GameObjectUtility.SetParentAndAlign(eventSystem, parent);
                esys = eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();

                Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
            }

            if (select && esys != null)
            {
                Selection.activeGameObject = esys.gameObject;
            }
        }

        // Helper function that returns a Canvas GameObject; preferably a parent of the selection, or other existing Canvas.
        public static GameObject GetOrCreateCanvasGameObject()
        {
            GameObject selectedGo = Selection.activeGameObject;

            // Try to find a gameobject that is the selected GO or one if its parents.
            Canvas canvas = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
            if (canvas != null && canvas.gameObject.activeInHierarchy)
                return canvas.gameObject;

            // No canvas in selection or its parents? Then use just any canvas..
            canvas = UnityEngine.Object.FindObjectOfType(typeof(Canvas)) as Canvas;
            if (canvas != null && canvas.gameObject.activeInHierarchy)
                return canvas.gameObject;

            // No canvas in the scene at all? Then create a new one.
            return CreateNewUI();
        }

        #endregion

        // 

        [MenuItem("Spark/Generate StaticAssets Index (Compressed)", false, 20)]
        private static void GenerateCompressedStaticAssetsIndex()
        {
            GenerateStaticAssetsIndex(true);
        }

        [MenuItem("Spark/Generate StaticAssets Index (Uncompressed)", false, 21)]
        private static void GenerateUncompressedStaticAssetsIndex()
        {
            GenerateStaticAssetsIndex(false);
        }

        public static void GenerateStaticAssetsIndex(bool compressed)
        {
            var staticAssetsPath = SparkHelper.staticAssetsPath;
            var staticIndexPath = SparkHelper.staticAssetsIndexPath;
            if (Directory.Exists(staticAssetsPath))
            {
                var files = new List<string>();
                var fullPath = Path.GetFullPath(staticAssetsPath).Replace("\\", "/");
                foreach (var file in Directory.GetFiles(staticAssetsPath, "*.*", SearchOption.AllDirectories))
                {
                    if (file.EndsWith(".meta") || Path.GetFileName(file) == "KlPkgAssets.dat")
                        continue;
                    files.Add(FileHelper.GetMD5Hash(file) + "," + Path.GetFullPath(file).Replace("\\", "/").Replace(fullPath + "/", ""));
                }

                FileHelper.WriteString(staticIndexPath, string.Join("\n", files.ToArray()));
            }
            else
            {
                if (File.Exists(staticIndexPath))
                {
                    File.Delete(staticIndexPath);
                }
            }

            UnityEditor.AssetDatabase.Refresh();
        }

        public static void GenerateStaticAssetsIndexV2()
        {
            //try
            //{
            Debug.Log("xdd:::GenerateStaticAssetsIndexV2 start");
            var files = Directory.GetFiles(SparkHelper.staticAssetsPath, "*.awb", SearchOption.AllDirectories)
                .Select(file => file.Replace("\\", "/").Replace(SparkHelper.staticAssetsPath + "/", "")).ToList();

            var luapath = Application.dataPath + "/Resources/Lua";
            files.AddRange(Directory.GetFiles(luapath, "*.lua.bytes", SearchOption.AllDirectories)
                .Select(file => "Lua/" + file.Replace("\\", "/").Replace(luapath + "/", "").Replace(".bytes", "")));

            FileHelper.WriteString(luapath + "/../PkgAssets.bytes", string.Join("\n", files.ToArray()));
            UnityEditor.AssetDatabase.Refresh();
            //} catch (Exception ex)
            //{
            //    Debug.Log("xdd:::GenerateStaticAssetsIndexV2 error:::" + ex.Message);
            //}
            Debug.Log("xdd:::GenerateStaticAssetsIndexV2 end");

        }

        //[MenuItem("Spark/Export View (C#)", false, 40)]
        //private static void ExportUIView()
        //{
        //	UIViewExporter.Export("Assets/Game/Resources", "Assets/Spark/Game/Raw", "Assets/Game/Scripts/ViewDesigner", new UI.CSharpScriptGenerator());
        //}

        [MenuItem("Spark/Export View (Lua)", false, 60)]
        private static void ExportUIViewToLua()
        {
            //foreach (var button in GameObject.FindObjectsOfType<UnityEngine.UI.Button>()) {
            //	var sound = button.GetComponent<UISound>();
            //	if (sound == null) {
            //		button.gameObject.AddComponent<UISound>();
            //	}
            //}
            //UIViewExporter.Export("Assets/SparkAssets/Game", "Assets/Spark/Game/Raw/GUI/Prefabs", "Assets/SparkAssets/Lua/Game/GUI/Views", new SparkEditor.UI.LuaScriptGenerator("UIGameView"));
            var dict = new Dictionary<string, bool>();
            Array.ConvertAll(Directory.GetDirectories("Assets/Raw/Game"), (name) => dict[Path.GetFileNameWithoutExtension(name)] = true);
            Array.ConvertAll(Directory.GetDirectories("Assets/SparkAssets/Game"), (name) => dict[Path.GetFileNameWithoutExtension(name)] = true);
            Array.ConvertAll(Directory.GetDirectories("Assets/SparkAssets/Lua/Game"), (name) => dict[Path.GetFileNameWithoutExtension(name)] = true);

            Func<string, string> getGameName = (name) =>
            {
                foreach (var kv in dict)
                {
                    if (name.StartsWith("UI" + kv.Key)) return kv.Key;
                }

                return "Common";
            };
            UIViewExporter.Export((name) => "Assets/Raw/GUI/Prefabs", (name) => "Assets/SparkAssets/Game/" + getGameName(name) + "/Views",
                (name) => "Assets/SparkAssets/Lua/Game/" + getGameName(name) + "/Views", new SparkEditor.UI.LuaScriptGenerator("UIGameView"));

            Debug.Log("ExportUIViewForLua Completed.");
        }

        //[MenuItem("Spark/Export Excel (CS)", false, 21)]
        //static public void ExportExcel() {
        //    ExcelExporter.Export(Application.dataPath + "/../../../../mobdw-others/data/trunk", Application.dataPath + "/Game/Resources/db", Application.dataPath + "/Game/Scripts/Excels", true, false);
        //}

        [MenuItem("Spark/Pack AssetBundles")]
        private static void BuildAssetBundles()
        {
            // throw new NotImplementedException("没有实现");
            //AssetBundleBuilder.BuildAssetBundles(Application.dataPath + "/Data", null, BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);
            //GameAssetBuilder.BuildAssets("0.1", "E:/StarX/abs", true);
            //FileHelper.CopyFile("E:/StarX/abs/SparkAssetIndex.awb", "E:/StarX/abs/Game/Common/SparkAssetIndex.awb", true);
            //var dirs = Directory.GetDirectories("E:/StarX/abs/Game");
            //foreach (var dir in dirs)
            //{
            //    var md5sb = new StringBuilder();
            //    var files = Directory.GetFiles(dir);
            //    foreach (var file in files)
            //    {
            //        md5sb.Append(FileHelper.GetMD5Hash(file));
            //    }

            //    var gameVer = FileHelper.GetMD5Hash(md5sb.ToString().GetASCIIBytes());

            //    var content = new StringBuilder().Append("{").Append("last_game_version:\"1.0.0\",").Append($"last_game_version:\"{gameVer}\"").Append("}");
            //    ;
            //    FileHelper.WriteString($"{dir}/version.txt", content.ToString());
            //}
            GameAssetBuilder.BuildAssets("dev", Application.dataPath + "/../../local-abs", true, false);
        }

        [MenuItem("Spark/Load AssetBundles")]
        private static void LoadAssetBundles()
        {
            //Spark.Assets.Initialize("");
            //var go = Spark.Assets.LoadAsset<GameObject>("Game/Ludo/Views/UILudoMain.prefab");
            //GameObject.Instantiate(go);
            //Debug.Log(go.name);
        }

        [MenuItem("Spark/Convert 3.1/3.2 To 3.3")]
        private static void ConvertTo33()
        {
            var luaViewPath = "/Game/GUI/Views";
            foreach (var path in Directory.GetFiles(luaViewPath, "*.lua", SearchOption.TopDirectoryOnly))
            {
                var text = File.ReadAllText(path);
                int index = text.IndexOf("UIScriptableView.Initialize(self, ");
                if (index > 0)
                {
                    int lastIndex = text.IndexOf(")", index + 1);
                    if (lastIndex > 0)
                    {
                        var begin = text.IndexOf("\"", index + 1);
                        var end = text.LastIndexOf("\"", lastIndex - 1);
                        if (begin < end)
                        {
                            var line = text.IndexOf(")");
                            var start = text.IndexOf(" ") + 1;
                            var name = text.Substring(start, text.IndexOf("=") - start - 1);
                            StringBuilder builder = new StringBuilder();
                            builder.Append(text.Substring(0, line + 1)).AppendLine();
                            builder.AppendLine();
                            builder.AppendLine("-- Properties");
                            builder.AppendFormat("{0}.prefabPath = \"{1}\"", name, text.Substring(begin + 1, end - begin - 1));
                            builder.Append(text.Substring(line + 1, index - line - 1));
                            builder.Append("UIScriptableView.GetComponents(self").Append(text.Substring(end + 1));
                            FileHelper.WriteString(path, builder.ToString());
                            Debug.Log("Process: " + path);
                        }
                    }
                }
            }
        }

        [MenuItem("Spark/Export PC Platform")]
        private static void ExportPC()
        {
            Directory.CreateDirectory("Assets/PCPlatform/Resources");
            var files = Directory.GetFiles("Assets/SparkAssets", "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var path = Path.GetDirectoryName(file)?.Substring("Assets/".Length);
                Directory.CreateDirectory(path!);
                File.Copy(file, Path.Combine("Resources", path, Path.GetFileName(file)));
            }
            File.Copy("Assets/Raw/GUI/Prefabs/UIDebugLogin.prefab", "Assets/PCPlatform/Resources/UIDebugLogin.prefab");
            AssetDatabase.Refresh();
            
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                target = EditorUserBuildSettings.activeBuildTarget, 
                options = BuildOptions.None
            });
            var summary = report.summary;
            switch (summary.result)
            {
                case BuildResult.Succeeded:
                    Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
                    break;
                case BuildResult.Failed:
                    Debug.Log("Build failed");
                    break;
                case BuildResult.Unknown:
                    break;
                case BuildResult.Cancelled:
                    break;
                // default:
                //     throw new ArgumentOutOfRangeException();
            }
            Directory.Delete("Assets/PCPlatform", true);
            AssetDatabase.Refresh();
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Spark;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace SparkEditor
{
    public static class GameAssetBuilder
    {
        //shell中调用
        public static void BuildAssets()
        {
            var args = GetArgs("BuildAssets", true);
            BuildAssets(args["version"], args["outPath"], Convert.ToBoolean(args["buildBundle"]), Convert.ToBoolean(args["buildAvatarRes"]));
        }

        public static void BuildAssets(string version, string outPath, bool buildBundle, bool buildAvatarRes)
        {
            var builder = new StringBuilder();
            var avatarbuilder = new StringBuilder();
            // phase 2: build assetbundles
            {
                if (buildBundle)
                {
                    BuildAssetBundles(version, outPath, buildAvatarRes);
                }
                else
                {
                    var cacheFile =
                        SparkEditorHelper.GetSparkAssetIndexCachePath(EditorUserBuildSettings.activeBuildTarget,
                            version);
                    if (!File.Exists(cacheFile))
                    {
                        throw new Exception("Can't found SparkIndexCacheFile: " + cacheFile);
                    }

                    FileHelper.CopyFile(cacheFile, SparkEditorHelper.GetSparkAssetIndexPath(), true);
                    AssetDatabase.Refresh();
                }

                BuildAssetBundlesManifest(builder, avatarbuilder, outPath, buildAvatarRes);
                
            }

            // phase 3: encrypt lua
            {
                var dstLuaPath = outPath + "/Lua";
                if (Directory.Exists(dstLuaPath))
                {
                    Directory.Delete(dstLuaPath, true);
                }

                CheckExistsBeforeBuild(GameConfig.projectLuaPath, (dir) =>
                {
                    EncryptLua(GameConfig.projectLuaPath, dstLuaPath, 20180014);
                    BuildAssetsManifest(builder, dstLuaPath, false);
                });
            }

            // phase 4: copy audios and movies
            // TODO

            // phase 5: write manifest file
            FileHelper.WriteString(outPath + "/AppAssetsManifest.txt", builder.ToString());

            //虚拟形象需要一个服装的MD5配置
            FileHelper.WriteString(outPath + "/AvatarManifest.txt", avatarbuilder.ToString());

            // phase 6: write version file
            // var commonPath = $"{outPath}/Game/Common";
            // FileHelper.MakeDirs(commonPath);
            // File.Move(outPath + "/SparkAssetIndex.awb", commonPath + "/SparkAssetIndex.awb");
            // var dirs = Directory.GetDirectories(outPath);
            // foreach (var dir in dirs)
            // {
            //     var md5sb = new StringBuilder();
            //     var files= Directory.GetFiles(dir);
            //     foreach (var file in files)
            //     {
            //         md5sb.Append(FileHelper.GetMD5Hash(file));
            //     }
            //
            //     var gameVer = FileHelper.GetMD5Hash(md5sb.ToString().GetASCIIBytes());
            //
            //     var content = new StringBuilder().Append("{").Append("last_game_version:\"1.0.0\",").Append($"last_game_version:\"{gameVer}\"").Append("}");;
            //     FileHelper.WriteString($"{dir}/version.txt", content.ToString());
            // }
        }

        private static void EncryptLua(string srcPath, string dstPath, int key)
        {
            Debug.LogFormat("Encrypt files: {0} => {1}", srcPath, dstPath);
            byte[] header = {0x53, 0x58, 0x47, 0x4D};
            foreach (var file in Directory.GetFiles(srcPath, "*.lua", SearchOption.AllDirectories))
            {
                var data = FileHelper.ReadBytes(file);
                // data = Spark.Compression.Compress(data, Spark.Compression.Algorithm.ZLIB);
                data = XLua.LuaDLL.Lua.EncryptData(data);
                var result = new byte[4 + data.Length];
                System.Buffer.BlockCopy(header, 0, result, 0, 4);
                System.Buffer.BlockCopy(data, 0, result, 4, data.Length);
                var path = $"{dstPath}/{file.Replace(srcPath, "")}";
                FileHelper.WriteBytes(path, result);
                // FileHelper.CopyFile(file, path, true);
            }
        }

        private static bool IsBuiltin(string file)
        {
            // if (file.EndsWith(SparkHelper.SparkAssetIndexName)) return true;
            // if (file.EndsWith(".lua")) return true;
            return true;
        }

        private static void BuildAssetsManifest(StringBuilder builder, string outPath, bool genRes)
        {
            var postion = outPath.Replace("\\", "/").LastIndexOf("/", StringComparison.Ordinal) + 1;
            var files = Directory.GetFiles(outPath, "*.*", SearchOption.AllDirectories);
            Array.Sort(files);
            foreach (var file in files)
            {
                var dstFile = file.Replace("\\", "/").Substring(postion);
                AppendFile(builder, file, dstFile, IsBuiltin(dstFile)).AppendLine();
                if (genRes)
                {
                    builder.Append(">").Append(dstFile).AppendLine();
                }
            }
        }

        private static void BuildAssetBundlesManifest(StringBuilder builder, StringBuilder avatarBuilder, string outPath, bool buildAvatarRes)
        {
            var manifest =
                AssetDatabase.LoadAssetAtPath<Spark.AssetsManifest>(SparkEditorHelper.GetSparkAssetIndexPath());
            if (manifest == null)
            {
                throw new Exception("Can't found '" + SparkEditorHelper.GetSparkAssetIndexPath() + "'");
            }

            foreach (var path in manifest.directories)
            {
                builder.Append("#").Append(path).AppendLine();
            }

            foreach (var bundle in manifest.bundles)
            {
                if (!buildAvatarRes && bundle.name.Contains("config_resource"))
                {
                    continue;
                }
                AppendFile(builder, $"{outPath}/{bundle.name}", bundle.name, IsBuiltin(bundle.name)).Append("|1");
                for (var i = 0; i < bundle.depends.Length; i++)
                {
                    builder.Append(i == 0 ? "|" : ",").Append(bundle.depends[i]);
                }

                builder.AppendLine();
                foreach (var asset in bundle.assets)
                {
                    builder.Append(">").Append(asset.index).Append("|").Append(asset.path).AppendLine();
                }


                if (bundle.name.Contains("config_resource"))
                {
                    avatarBuilder.Append(Spark.FileHelper.GetMD5Hash($"{outPath}/{bundle.name}")).Append("|")
                    .Append("|").Append(bundle.name);
                    avatarBuilder.AppendLine();
                }
            
            }
        }

        private static StringBuilder AppendFile(StringBuilder builder, string file, string name, bool important)
        {
            if (important)
            {
                builder.Append("*");
            }

            return builder.Append(Spark.FileHelper.GetMD5Hash(file)).Append(Path.GetExtension(file)).Append("|")
                .Append(new FileInfo(file).Length)
                .Append("|").Append(name);
        }

        private static bool CheckExistsBeforeBuild(string path, Action<string> callback)
        {
            if (Directory.Exists(path) || File.Exists(path))
            {
                callback(path);
                return true;
            }

            return false;
        }

        private static void BuildAssetBundles(string version, string outPath, bool buildAvatarRes)
        {
            var packageGroups = new List<AssetBundleBuilder.Batch>();
            //var realPackageMap = new Dictionary<string, string>();

            var defaultOptions = new AssetBundleBuilder.BatchOptions()
            {
                legacy = false,
                combineShaderes = false,
            };

            Func<string, List<AssetBundleBuilder.Package>> NewPack = (name) =>
            {
                var batch = new AssetBundleBuilder.Batch(new AssetBundleBuilder.BatchOptions()
                {
                    legacy = false,
                    combineShaderes = false,
                    sharedDirectoryName = name ?? "SharedAssets",
                    sharedMode = AssetBundleBuilder.BatchOptions.SharedMode.Single,
                });
                packageGroups.Add(batch);
                return batch.packages;
            };
            Action<AssetBundleBuilder.Package, AssetBundleBuilder.BatchOptions> NewBatch = (package, options) =>
            {
                var batch = new AssetBundleBuilder.Batch(package, options ?? defaultOptions);
                packageGroups.Add(batch);
            };

            var games = Directory.GetDirectories("Assets/SparkAssets/Game", "*", SearchOption.TopDirectoryOnly);
            foreach (var game in games)
            {
                var gameName = Path.GetFileNameWithoutExtension(game);
                var gamePath = $"Assets/SparkAssets/Game/{gameName}";
                var packages = NewPack($"Game/{gameName}");
                packages.Add(new AssetBundleBuilder.Package($"Game/{gameName}/{gameName}.asset", Directory.GetFiles(
                    gamePath, "*.prefab", SearchOption.AllDirectories), true));
                packages.Add(new AssetBundleBuilder.Package($"Game/{gameName}/{gameName}Audio.asset",
                    Directory.GetFiles(gamePath, "*.mp3", SearchOption.AllDirectories), true));
                packages.Add(new AssetBundleBuilder.Package($"Game/{gameName}/{gameName}AnimatorController.asset", Directory.GetFiles(
                    gamePath, "*.controller", SearchOption.AllDirectories), true));
                if (gameName == "AmongUS")
                {
                    var amongTaskPics = Directory.GetFiles("Assets/Raw/Game/AmongUS/UI/CommonTask", "*.png", SearchOption.AllDirectories);
                    packages.Add(new AssetBundleBuilder.Package($"Game/{gameName}/{gameName}Pic.asset", amongTaskPics, true));
                }
                // var spines = new List<string>();
                // CheckExistsBeforeBuild($"Assets/Raw/Game/{gameName}/Spine", (path) =>
                // {
                //     spines.AddRange(Directory.GetFiles(path, "*", SearchOption.AllDirectories).Where((p) => !p.EndsWith(".meta"))); 
                // });
                // CheckExistsBeforeBuild($"Assets/Raw/Game/{gameName}/Spines", (path) =>
                // {
                //     spines.AddRange(Directory.GetFiles(path, "*", SearchOption.AllDirectories).Where((p) => !p.EndsWith(".meta")));
                // });
                // if (spines.Count > 0)
                // {
                //     packages.Add(new AssetBundleBuilder.Package($"Game/{gameName}/{gameName}Spine.asset", spines, true));
                // }
            }

            //打包scene
            var scenes = Directory.GetFiles("Assets/SparkAssets/Scenes", "*.unity", SearchOption.AllDirectories);
            foreach (var scene in scenes)
            {
                var name = Path.GetFileNameWithoutExtension(scene);
                if (name != "Startup")
                {
                    if (name.Contains("KTV"))
                    {
                        NewBatch(new AssetBundleBuilder.Package($"Game/KTV/Scenes/{name}.asset", scene)
                        {
                            combined = true
                        }, defaultOptions);
                    } else if (name.Contains("AUS"))
                    {
                        NewBatch(new AssetBundleBuilder.Package($"Game/AmongUS/Scenes/{name}.asset", scene)
                        {
                            combined = true
                        }, defaultOptions);
                    }
                }
            }

            // KTV actor
            // var ktvActors = Directory.GetFiles("Assets/Raw/Game/AvatarX/Templates", "*.prefab", SearchOption.AllDirectories);
            // foreach (var actor in ktvActors)
            // {
            //     NewBatch(new AssetBundleBuilder.Package($"Game/KTV/Actors/{Path.GetFileNameWithoutExtension(actor)}.asset", actor), defaultOptions);
            // }

            // KTV effects
            var ktvEfs = Directory.GetDirectories("Assets/Raw/Game/LiveStageX/Effect/Prefab", "*", SearchOption.TopDirectoryOnly);
            foreach (var ef in ktvEfs)
            {
                var name = Path.GetFileNameWithoutExtension(ef);
                var pack = NewPack($"Game/KTV/Effects/{name}");
                pack.Add(new AssetBundleBuilder.Package($"Game/KTV/Effects/{name}.asset",
                    Directory.GetFiles(ef, "*.prefab", SearchOption.AllDirectories), true));
            } 

            // scene
            NewBatch(new AssetBundleBuilder.Package($"Game/PersonalShow/Scenes/PersonalShow.asset", "Assets/SparkAssets/Game/PersonalShow/Scenes/PersonalShow.unity")
            {
                combined = true
            }, defaultOptions);

            // // // actor
            Debug.Log("xdd:::buildAvatarRes:::" + buildAvatarRes);
            var actorOptions = defaultOptions;
            if (!buildAvatarRes)
            {
                actorOptions = new AssetBundleBuilder.BatchOptions()
                {
                    legacy = false,
                    combineShaderes = false,
                    realBuild = false,
                };
            }

            var Actors = Directory.GetFiles("Assets/PersonalShow", "*.prefab", SearchOption.AllDirectories);
            foreach (var actor in Actors)
            {
                NewBatch(new AssetBundleBuilder.Package($"Game/AvatarRes/config_resource/{Path.GetFileNameWithoutExtension(actor)}.asset", actor)
                {
                    combined = true
                }, actorOptions);
            }

            var pics = Directory.GetFiles("Assets/PersonalShow", "*.png", SearchOption.AllDirectories);
            foreach (var pic in pics)
            {
                NewBatch(new AssetBundleBuilder.Package($"Game/AvatarRes/config_resource/{Path.GetFileNameWithoutExtension(pic)}.asset", pic)
                {
                    combined = true
                }, actorOptions);
            }

            //打包脸部资源数据
            var f_faceDatas = Directory.GetFiles("Assets/PersonalShow/AvatarX/F_Shape_Data", "*.txt", SearchOption.AllDirectories);
            foreach (var file in f_faceDatas)
            {
                NewBatch(new AssetBundleBuilder.Package($"Game/AvatarRes/config_resource/{Path.GetFileNameWithoutExtension(file)}.asset", file)
                {
                    combined = true
                }, actorOptions);
            }
            //打包男生脸部资源数据
            var m_faceDatas = Directory.GetFiles("Assets/PersonalShow/AvatarX/M_Shape_Data", "*.txt", SearchOption.AllDirectories);
            foreach (var file in m_faceDatas)
            {
                NewBatch(new AssetBundleBuilder.Package($"Game/AvatarRes/config_resource/{Path.GetFileNameWithoutExtension(file)}.asset", file)
                {
                    combined = true
                }, actorOptions);
            }

            AssetBundleBuilder.BuildAssetBundles(version, outPath, packageGroups,
                BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
        }

        public static void ExportAndroid()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                throw new Exception("目标平台与当前平台不一致，请确定要平台转换！");
            }

            var args = GetArgs("ExportAndroid", true);
            if (!args.ContainsKey("exportProjectPath"))
            {
                Debug.LogError("Cannot get 【exportProjectPath】 from args");
                EditorApplication.Exit(1);
            }

            UpdatePlayerSettings(args);

            if (args.ContainsKey("bundleVersionCode") && !string.IsNullOrEmpty(args["bundleVersionCode"]))
            {
                PlayerSettings.Android.bundleVersionCode = Convert.ToInt32(args["bundleVersionCode"]);
            }

            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Disabled);
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel21;

            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
            BuildPipeline.BuildPlayer(GetBuildScenes(), args["exportProjectPath"], BuildTarget.Android,
                BuildOptions.None);
        }

        public static void ExportXcode()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
            {
                throw new Exception("目标平台与当前平台不一致，请确定要平台转换！");
            }

            var args = GetArgs("ExportXcode", true);
            if (!args.ContainsKey("exportProjectPath"))
            {
                Debug.LogError("Cannot get 【exportProjectPath】 from args");
                EditorApplication.Exit(1);
            }

            UpdatePlayerSettings(args);

            if (args.ContainsKey("bundleBuildNumber") && !string.IsNullOrEmpty(args["bundleBuildNumber"]))
            {
                PlayerSettings.iOS.buildNumber = args["bundleBuildNumber"];
            }

            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.iOS.appInBackgroundBehavior = iOSAppInBackgroundBehavior.Custom;
            PlayerSettings.iOS.appleEnableAutomaticSigning = false;
            PlayerSettings.iOS.prerenderedIcon = true;
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS,
                new[] {GraphicsDeviceType.Metal, GraphicsDeviceType.OpenGLES3});
            // PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 2);

            BuildPipeline.BuildPlayer(GetBuildScenes(), args["exportProjectPath"], BuildTarget.iOS, BuildOptions.None);
        }

        private static void UpdatePlayerSettings(IReadOnlyDictionary<string, string> args)
        {
            if (args.ContainsKey("bundleName") && !string.IsNullOrEmpty(args["bundleName"]))
            {
                PlayerSettings.productName = args["bundleName"];
            }

            if (args.ContainsKey("bundleIdentifier") && !string.IsNullOrEmpty(args["bundleIdentifier"]))
            {
                PlayerSettings.applicationIdentifier = args["bundleIdentifier"];
            }

            if (args.ContainsKey("bundleVersion") && !string.IsNullOrEmpty(args["bundleVersion"]))
            {
                PlayerSettings.bundleVersion = args["bundleVersion"];
            }

            PlayerSettings.stripEngineCode = false;
//            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.SplashScreen.show = false;

#if UNITY_ANDROID
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.All;
#elif UNITY_IOS
			PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
			PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 2); // 0 - None, 1 - ARM64, 2 - Universal.
#endif
        }

        private static string[] GetBuildScenes()
        {
            return (from e in EditorBuildSettings.scenes where e != null && e.enabled select e.path).ToArray();
        }

        //shell中调用
        public static void SetScriptingDefineSymbols()
        {
            var defines = string.Empty;
            var args = GetArgs("SetScriptingDefineSymbols", true);
            if (args.ContainsKey("defines"))
            {
                defines = args["defines"];
            }

#if UNITY_IOS
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, defines);
#elif UNITY_ANDROID
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defines);
#elif UNITY_STANDALONE
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
#endif
        }

        private static Dictionary<string, string> GetArgs(string methodName, bool native)
        {
            if (native)
            {
                methodName = "SparkEditor.GameAssetBuilder." + methodName;
            }

            var isArg = false;
            var args = new Dictionary<string, string>();
            foreach (var arg in Environment.GetCommandLineArgs())
            {
                if (isArg)
                {
                    if (!arg.StartsWith("--")) continue;
                    var splitIndex = arg.IndexOf("=", StringComparison.Ordinal);
                    if (splitIndex > 0)
                    {
                        args.Add(arg.Substring(2, splitIndex - 2), arg.Substring(splitIndex + 1));
                    }
                    else
                    {
                        args.Add(arg.Substring(2), "true");
                    }
                }
                else if (arg == methodName)
                {
                    isArg = true;
                }
            }

            return args;
        }
    }
}
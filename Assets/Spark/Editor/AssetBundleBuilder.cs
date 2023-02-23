using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Spark;
using UnityEditor;
using UnityEngine;

namespace SparkEditor
{
    public static class AssetBundleBuilder
    {
        public static void BuildAssetBundles(string version, string outPath, IEnumerable<Batch> packageGroups,
            BuildAssetBundleOptions options, BuildTarget target)
        {
            if (EditorUserBuildSettings.activeBuildTarget != target)
            {
                Debug.LogErrorFormat("当前平台为[{0}]，请切换到目标平台[{1}]", EditorUserBuildSettings.activeBuildTarget, target);
                return;
            }

            var cacheData = new PackageData() {packageInfos = new List<PackageData.PackageInfo>()};

            var cacheFile = $"{SparkEditorHelper.temporaryCachePath}/ABBuilderCache_{target}_{version}";
            Debug.Log(cacheFile);
            if (File.Exists(cacheFile))
            {
                var data = JsonUtility.FromJson<PackageData>(FileHelper.ReadString(cacheFile));
                if (data != null)
                {
                    cacheData = data;
                }
            }

            // 清空AB的临时文件夹
            var oPath = SparkEditorHelper.temporaryCachePath + "/AssetBundles";
            if (Directory.Exists(oPath))
            {
                Directory.Delete(oPath, true);
            }

            Directory.CreateDirectory(oPath);

            var manifestChanged = false;
            var manifests = new List<AssetsManifestProxy>();
            var packageDatas = new List<PackageData>();

            var cacheMapData = cacheData.ToDictionary();

            var exceptions = new List<Exception>();

            foreach (var data in packageGroups)
            {
                // 生成依赖包
                var optimizer = new PackageOptimizer(data.packages, data.options);
                var packages = optimizer.packages;
                var packageData = optimizer.GeneratePackageData();
                packages = packageData.GetModifiesPackages(packages, cacheMapData);

                try
                {
                    if (packages != null && packages.Count > 0)
                    {
                        manifestChanged = true;
                        Debug.LogFormat("Total:{0}, Build:{1}", packageData.packageInfos.Count, packages.Count);
                        Debug.LogFormat("------------BuildAssetBundleOptions {0} ---------", data.bundleOptions);

                        if (data.options.realBuild)
                        {
                            BuildPipeline.BuildAssetBundles(oPath, packages.ConvertAll((p) =>
                            {
                                var list = new List<string>(p.assets);
                                return new AssetBundleBuild()
                                {
                                    assetBundleName = p.name,
                                    assetNames = list.Exists((s) => s.ToLower().EndsWith(".unity"))
                                        ? list.FindAll((s) => s.ToLower().EndsWith(".unity")).ToArray()
                                        : list.ToArray()
                                };
                            }).ToArray(), BuildAssetBundleOptions.DeterministicAssetBundle | data.bundleOptions, target);

                            foreach (var package in packages)
                            {
                                FileHelper.CopyFile(oPath + "/" + package.name.ToLower(), outPath + "/" + package.name, true);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
                finally
                {
                    packageDatas.Add(packageData);
                    manifests.Add(packageData.GenerateAssetManifest());
                }
            }

            var manifestFilePath = SparkEditorHelper.GetSparkAssetIndexPath();
            if (manifestChanged || !File.Exists(manifestFilePath))
            {
                // 合并所有文件的manifest列表
                var manifest = manifests[0];
                for (var i = 1; i < manifests.Count; i++)
                {
                    manifest.CopyFrom(manifests[i]);
                }

                Debug.Log("manifest 1:" + manifest);

                if (File.Exists(manifestFilePath))
                {
                    File.Delete(manifestFilePath);
                }

                Debug.Log("manifest 2:" + manifest);
                AssetDatabase.CreateAsset(manifest.ToManifest(), manifestFilePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                BuildPipeline.BuildAssetBundles(oPath, new[]
                {
                    new AssetBundleBuild()
                    {
                        assetBundleName = SparkHelper.SparkAssetIndexName,
                        assetNames = new[] {manifestFilePath},
                    }
                }, BuildAssetBundleOptions.DeterministicAssetBundle | options, target);
                FileHelper.CopyFile(oPath + "/" + SparkHelper.SparkAssetIndexName.ToLower(),
                    outPath + "/" + SparkHelper.SparkAssetIndexName, true);

                // add SparkHelper.SparkAssetIndexName
                manifest.bundles.Add(new AssetsManifest.Bundle()
                {
                    name = SparkHelper.SparkAssetIndexName,
                    assets = new[]
                    {
                        new AssetsManifest.Asset()
                        {
                            index = 0, path = SparkHelper.SparkAssetIndexName + ".asset"
                        }
                    },
                    scenes = new string[0],
                    depends = new int[0],
                    dependents = 0,
                });
                AssetDatabase.CreateAsset(manifest.ToManifest(), manifestFilePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // 删除过期的文件
                foreach (var path in Directory.GetFiles(outPath, "*.*", SearchOption.AllDirectories))
                {
                    var name = path.Replace("\\", "/").Substring(outPath.Length + 1);
                    if (name == SparkHelper.SparkAssetIndexName)
                        continue;

                    if (!manifest.bundles.Exists((b) => b.name == name))
                    {
                        Debug.Log("Deleting (" + path + ") ...");
                        File.Delete(path);
                    }
                }

                // 保存manifest文件
                FileHelper.CopyFile(manifestFilePath, SparkEditorHelper.GetSparkAssetIndexCachePath(target, version),
                    true);
            }

            // Save CacheData
            var pd = packageDatas[0];
            for (var i = 1; i < packageDatas.Count; i++)
            {
                pd.CopyFrom(packageDatas[i]);
            }

            FileHelper.WriteString(cacheFile, JsonUtility.ToJson(pd));

            if (exceptions.Count > 0)
            {
                // throw new Exception("There are some errors during build assetbundles.");
                foreach (var exception in exceptions)
                {
                    throw exception;
                }
            }
        }

        public class Package
        {
            public string name;
            public string group;
            public bool combined;
            public HashSet<string> assets;

            public Package(string name) : this(name, "", "")
            {
            }

            public Package(string name, string asset) : this(name, Path.GetDirectoryName(name), asset)
            {
            }

            public Package(string name, string group, string asset)
            {
                this.name = Path.ChangeExtension(name, SparkHelper.SparkAssetExtension);
                this.group = @group ?? string.Empty;
                combined = false;
                assets = new HashSet<string>();
                if (!string.IsNullOrEmpty(asset))
                {
                    assets.Add(asset.Replace("\\", "/"));
                }
            }

            public Package(string name, IEnumerable<string> collection, bool combined) : this(name,
                Path.GetDirectoryName(name), collection, combined)
            {
            }

            public Package(string name, string group, IEnumerable<string> collection, bool combined)
            {
                this.name = Path.ChangeExtension(name, SparkHelper.SparkAssetExtension);
                this.combined = combined;
                this.group = @group ?? string.Empty;
                var hashSet = new HashSet<string>();
                using (var it = collection.GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        hashSet.Add(it.Current?.Replace("\\", "/"));
                    }
                }

                assets = hashSet;
            }

            public string[] GetAssetsArray()
            {
                var arr = new string[assets.Count];
                assets.CopyTo(arr);
                return arr;
            }
        }

        public class BatchOptions
        {
            public enum SharedMode
            {
                Single,
                All,
            }

            public bool legacy = true;
            public bool combineShaderes = true;
            public SharedMode sharedMode = SharedMode.All;
            public string sharedDirectoryName = "SharedAssets";
            public bool realBuild = true; //为了解决有些资源不想每次都真正build，但是又无法加入assets里面的问题
        }

        public class Batch
        {
            public BatchOptions options;
            public List<Package> packages;
            public BuildAssetBundleOptions bundleOptions;

            public Batch()
            {
                this.packages = new List<Package>();
            }

            public Batch(BatchOptions options, BuildAssetBundleOptions buildOption = BuildAssetBundleOptions.ChunkBasedCompression) : this()
            {
                this.options = options;
                this.bundleOptions = buildOption;
            }

            public Batch(List<Package> packages, BatchOptions options = null)
            {
                this.options = options;
                this.packages = packages;
            }

            public Batch(Package package, BatchOptions options = null, BuildAssetBundleOptions buildOption = BuildAssetBundleOptions.ChunkBasedCompression)
            {
                this.options = options;
                this.packages = new List<Package>() {package};
                this.bundleOptions = buildOption;
            }
        }

        [Serializable]
        public class PackageData
        {
            [Serializable]
            public class AssetInfo
            {
                [SerializeField] public string name;
                [SerializeField] public string md5;
                [SerializeField] public bool exportable;
            }

            [Serializable]
            public class PackageInfo
            {
                [SerializeField] public string name;
                [SerializeField] public List<AssetInfo> assetInfos;
                [SerializeField] public List<string> depends;
                [SerializeField] public List<string> reverseDepends;
            }

            [SerializeField] public List<PackageInfo> packageInfos;

            public void CopyFrom(PackageData other)
            {
                packageInfos.AddRange(other.packageInfos);
            }

            public Dictionary<string, PackageInfo> ToDictionary()
            {
                var packageInfoMap = new Dictionary<string, PackageInfo>();
                foreach (var packageInfo in packageInfos)
                {
                    if (packageInfoMap.ContainsKey(packageInfo.name))
                    {
                        throw new Exception("This file is exist! => " + packageInfo.name);
                    }

                    packageInfoMap.Add(packageInfo.name, packageInfo);
                }

                return packageInfoMap;
            }

            public List<Package> GetModifiesPackages(List<Package> packages, Dictionary<string, PackageInfo> cacheDataMap)
            {
                var dataMap = this.ToDictionary();

                var modifies = new HashSet<string>();
                foreach (var packageInfo in packageInfos)
                {
                    if (modifies.Contains(packageInfo.name))
                        continue;
                    var modified = !cacheDataMap.TryGetValue(packageInfo.name, out var cacheInfo);
                    if (!modified)
                    {
                        if (cacheInfo.depends.Count != packageInfo.depends.Count)
                        {
                            modified = true;
                            Debug.LogWarning($"[MOD]: depends count.('{packageInfo.name}': {cacheInfo.depends.Count} => {packageInfo.depends.Count})");
                        }
                        else if (!packageInfo.depends.TrueForAll((s) => cacheInfo.depends.Contains(s)))
                        {
                            modified = true;
                            Debug.LogWarning(
                                $"[MOD]: depends list.('{packageInfo.name}': {string.Join("_", cacheInfo.depends.ToArray())} => {string.Join("_", packageInfo.depends.ToArray())})");
                        }
                        else if (cacheInfo.assetInfos.Count != packageInfo.assetInfos.Count)
                        {
                            modified = true;
                            Debug.LogWarning($"[MOD]: assetInfos count.('{packageInfo.name}': {cacheInfo.assetInfos.Count} => {packageInfo.assetInfos.Count})");
                        }
                        else
                        {
                            var cacheAssetMap = new Dictionary<string, string>();
                            foreach (var assetInfo in cacheInfo.assetInfos)
                            {
                                cacheAssetMap[assetInfo.name] = assetInfo.md5;
                            }

                            foreach (var assetInfo in packageInfo.assetInfos)
                            {
                                if (!cacheAssetMap.TryGetValue(assetInfo.name, out var md5) || md5 != assetInfo.md5)
                                {
                                    modified = true;
                                    Debug.LogWarning($"[MOD]: assetInfos list.('{packageInfo.name}': {assetInfo.name})");
                                    break;
                                }
                            }
                        }
                    }

                    if (modified)
                    {
                        var depends = new Queue<PackageInfo>();
                        depends.Enqueue(packageInfo);
                        while (depends.Count > 0)
                        {
                            var p = depends.Dequeue();
                            modifies.Add(p.name);
                            foreach (var depend in p.depends)
                            {
                                if (modifies.Contains(depend))
                                    continue;
                                depends.Enqueue(dataMap[depend]);
                            }
                        }
                    }
                }

                if (modifies.Count > 0)
                {
                    return packages.Count == modifies.Count
                        ? packages
                        : packages.FindAll((p) => modifies.Contains(p.name));
                }

                return null;
            }

            public Spark.AssetsManifestProxy GenerateAssetManifest()
            {
                var bundles = new List<Spark.AssetsManifest.Bundle>();
                var directories = new List<string>();
                var assets = new List<Spark.AssetsManifest.Asset>();
                var depends = new List<int>();
                var scenes = new List<string>();

                var dict = new Dictionary<string, int>();
                for (var i = 0; i < packageInfos.Count; i++)
                {
                    dict[packageInfos[i].name] = i;
                }

                var token = "/SparkAssets/";
                var tokenLength = token.Length;
                for (var i = 0; i < packageInfos.Count; i++)
                {
                    assets.Clear();
                    scenes.Clear();
                    depends.Clear();

                    var packageInfo = packageInfos[i];
                    var bundle = new Spark.AssetsManifest.Bundle()
                    {
                        name = packageInfo.name,
                        dependents = packageInfo.reverseDepends.Count
                    };

                    foreach (var depend in packageInfo.depends)
                    {
                        depends.Add(dict[depend]);
                    }

                    foreach (var assetInfo in packageInfo.assetInfos)
                    {
                        if (assetInfo.exportable)
                        {
                            var index = 0;
                            var assetName = assetInfo.name;
                            var p = assetName.LastIndexOf(token, StringComparison.Ordinal);
                            if (p >= 0)
                            {
                                var dir = assetName.Substring(0, p + tokenLength);
                                assetName = assetName.Substring(p + tokenLength);
                                index = directories.IndexOf(dir) + 1;
                                if (index <= 0)
                                {
                                    directories.Add(dir);
                                    index = directories.Count;
                                }
                            }

                            assets.Add(new AssetsManifest.Asset() {path = assetName, index = index});

                            // Scenes
                            if (assetInfo.name.ToLower().EndsWith(".unity"))
                            {
                                scenes.Add(Path.GetFileNameWithoutExtension(assetInfo.name));
                            }
                        }
                    }

                    bundle.assets = assets.ToArray();
                    bundle.scenes = scenes.ToArray();
                    bundle.depends = depends.ToArray();
                    bundles.Add(bundle);
                }

                return new Spark.AssetsManifestProxy() {bundles = bundles, directories = directories};
            }
        }

        private class PackageOptimizer
        {
            public abstract class Asset
            {
                public Asset(string name)
                {
                    this.name = name;
                }

                public string name { get; }
                private HashSet<Package> m_Packages;

                public virtual HashSet<Package> packages => m_Packages ??= new HashSet<Package>();

                public virtual Package package { get; set; }
            }

            public class SingleAsset : Asset
            {
                public SingleAsset(string name) : base(name)
                {
                }
            }

            public class CombineAsset : Asset
            {
                public CombineAsset(string name) : this(name, null)
                {
                }

                public CombineAsset(string name, HashSet<string> assets) : base(name)
                {
                    this.assets = assets ?? new HashSet<string>();
                }

                public HashSet<string> assets { get; }
            }

            public class ReferenceAsset : Asset
            {
                private Asset m_Asset;

                public ReferenceAsset(string name, Asset asset) : base(name)
                {
                    m_Asset = asset;
                }

                public override HashSet<Package> packages => m_Asset.packages;

                public override Package package => m_Asset.package;
            }

            private Dictionary<string, string> m_ExportableAssets = new Dictionary<string, string>();
            private HashSet<string> m_NonTaggedTextures = new HashSet<string>();
            private Dictionary<string, Asset> m_AssetInfos = new Dictionary<string, Asset>();

            private Dictionary<string, HashSet<string>> m_CombinedAssets = new Dictionary<string, HashSet<string>>();

            // 资源的反向依赖列表
            private Dictionary<string, HashSet<string>> m_AssetReverseDependencies = new Dictionary<string, HashSet<string>>();

            public List<Package> packages { get; private set; }

            private static readonly BatchOptions defaultOptions = new BatchOptions();

            private BatchOptions m_Options;

            public PackageOptimizer(List<Package> packages, BatchOptions options = null)
            {
                m_Options = options ?? defaultOptions;

                foreach (var p in packages)
                {
                    if (p.assets.Count == 0)
                    {
                        continue;
                    }
                    if (p.combined)
                    {
                        m_CombinedAssets.Add("[CP]" + p.name, new HashSet<string>(p.assets));
                    }

                    foreach (var asset in p.assets)
                    {
                        CollectDependencies(asset, p, true);
                    }
                }
#if UNITY_5_4_OR_NEWER // || UNITY_5_5 || UNITY_5_6 || UNITY_2017
                // FixedUnitySpriteTextureBug();
#endif
                GenerateCombinedAssets();
                GenerateRawPackages();
                // TODO: 预测包大小
                ConvertCombinedToAssets();
            }

            public PackageData GeneratePackageData()
            {
                var allReverseDepends = new Dictionary<string, List<string>>();

                var packageData = new PackageData() {packageInfos = new List<PackageData.PackageInfo>()};
                foreach (var package in packages)
                {
                    if (!allReverseDepends.TryGetValue(package.name, out var reverseDepends))
                    {
                        reverseDepends = new List<string>();
                        allReverseDepends[package.name] = reverseDepends;
                    }

                    var packageInfo = new PackageData.PackageInfo()
                    {
                        name = package.name,
                        assetInfos = new List<PackageData.AssetInfo>(),
                        depends = new List<string>(),
                        reverseDepends = reverseDepends
                    };
                    packageData.packageInfos.Add(packageInfo);
                    var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                    foreach (var asset in package.assets)
                    {
                        var bytes = FileHelper.ReadBytes(asset);
                        if (FileHelper.ExistsFile(asset + ".meta"))
                        {
                            ArrayUtility.AddRange(ref bytes, FileHelper.ReadBytes(asset + ".meta"));
                        }

                        packageInfo.assetInfos.Add(new PackageData.AssetInfo()
                        {
                            name = asset,
                            exportable = m_ExportableAssets.ContainsKey(asset),
                            md5 = BitConverter.ToString(md5.ComputeHash(bytes)).Replace("-", "").ToLower()
                        });
                        foreach (var path in GetDependencies(asset, false))
                        {
                            if (package.assets.Contains(path))
                                continue;
                            Asset assetInfo = null;
                            if (m_AssetInfos.TryGetValue(path, out assetInfo))
                            {
                                var name = assetInfo.package.name;
                                if (!packageInfo.depends.Contains(name))
                                {
                                    packageInfo.depends.Add(name);
                                    if (!allReverseDepends.TryGetValue(name, out reverseDepends))
                                    {
                                        reverseDepends = new List<string>();
                                        allReverseDepends[name] = reverseDepends;
                                    }

                                    reverseDepends.Add(packageInfo.name);
                                }
                            }
                        }
                    }
                }

                return packageData;
            }

            // 修复当Texture类型为Sprite时，多种类型（Material、RawImage、...）和Texture不能分包的bug
            private void FixedUnitySpriteTextureBug()
            {
                var queue = new Queue<string>();
                foreach (var tex in m_NonTaggedTextures)
                {
                    var list = new HashSet<string>();
                    // 循环查找依赖关系
                    queue.Clear();
                    queue.Enqueue(tex);
                    while (queue.Count > 0)
                    {
                        var str = queue.Dequeue();
                        if (str.ToLower().EndsWith(".unity")) // 场景文件不可与Texture一起
                            continue;
                        list.Add(str);
                        if (m_AssetReverseDependencies.TryGetValue(str, out var reverseDepends))
                        {
                            foreach (var depend in reverseDepends)
                            {
                                if (queue.Contains(depend))
                                    continue;
                                queue.Enqueue(depend);
                            }
                        }
                    }

                    if (list.Count > 0)
                    {
                        m_CombinedAssets.Add("[TP]" + m_CombinedAssets.Count, list);
                    }
                }
            }

            private void GenerateCombinedAssets()
            {
                var array = new HashSet<string>[m_CombinedAssets.Count];
                m_CombinedAssets.Values.CopyTo(array, 0);

                for (var i = array.Length - 1; i > 0; i--)
                {
                    var a = array[i];
                    for (var j = 0; j < i; j++)
                    {
                        var b = array[j];
                        if (a.IsIntersectOf(b))
                        {
                            b.UnionWith(a);
                            array[i] = null;
                            break;
                        }
                    }
                }

                for (var i = 0; i < array.Length; i++)
                {
                    var arr = array[i];
                    if (arr != null)
                    {
                        var name = "[PP]" + i;
                        var combineAsset = new CombineAsset(name);
                        foreach (var v in arr)
                        {
                            if (m_AssetInfos.TryGetValue(v, out var info))
                            {
                                combineAsset.packages.UnionWith(info.packages);
                                m_AssetInfos[v] = new ReferenceAsset(v, combineAsset);
                            }

                            combineAsset.assets.Add(v);
                        }

                        m_AssetInfos.Add(name, combineAsset);
                    }
                }
            }

            private void GenerateRawPackages()
            {
                var names = new List<string>();
                var packages = new Dictionary<string, Package>();
                var scenePackage = new Package("##Scene##");
                var shaderPackage = new Package(SparkHelper.SparkUnityShadersName); // 特定名称，同SparkHelper.SparkUnityShadersName
                foreach (var kv in m_AssetInfos)
                {
                    var assetInfo = kv.Value;
                    if (assetInfo is ReferenceAsset)
                        continue;

                    if (assetInfo.GetType() == typeof(SingleAsset))
                    {
                        if (assetInfo.name.ToLower().EndsWith(".unity"))
                        {
                            if (m_Options.legacy)
                            {
                                assetInfo.packages.Add(scenePackage);
                            }
                        }
                        else if (m_Options.combineShaderes && assetInfo.name.ToLower().EndsWith(".shader"))
                        {
                            // 所有shader放到一个package里
                            assetInfo.packages.Clear();
                            assetInfo.packages.Add(shaderPackage);
                        }
                    }

                    var packageName = string.Empty;
                    if (assetInfo.packages.Count == 1)
                    {
                        foreach (var pkg in assetInfo.packages)
                        {
                            packageName = pkg.name;
                            break;
                        }
                    }
                    else
                    {
                        if (m_Options.legacy)
                        {
                            names.Clear();
                            foreach (var pkg in assetInfo.packages)
                            {
                                names.Add(pkg.name);
                            }

                            names.Sort();
                            packageName = string.Join("_", names.ToArray());
                        }
                        else
                        {
                            packageName = "Pack_" + assetInfo.name + "_" + assetInfo.GetHashCode();
                        }

                        if (m_Options.sharedMode == BatchOptions.SharedMode.Single)
                        {
                            packageName = packageName + "_" + assetInfo.name;
                        }
                    }

                    if (!packages.TryGetValue(packageName, out var package))
                    {
                        package = new Package(assetInfo.packages.Count == 1
                            ? packageName
                            : $"SharedAssets/{packageName}{SparkHelper.SparkAssetExtension}");
                        packages.Add(packageName, package);
                    }

                    package.assets.Add(assetInfo.name);
                    assetInfo.package = package;
                }

                this.packages = new List<Package>(packages.Values);
            }

            private string[] GetDependencies(string pathName, bool recursive)
            {
                //过滤目录
                //var depends = AssetDatabase.GetDependencies(pathName, recursive).Where((file)=>!Directory.Exists(file));
                var depends = AssetDatabase.GetDependencies(pathName, recursive);
                if (pathName.ToLower().EndsWith(".unity"))
                {
                    var list = new List<string>(depends);
                    for (var i = 0; i < list.Count; i++)
                    {
                        var path = list[i];
                        if (path.EndsWith("LightingData.asset"))
                        {
                            // Editor only objects cannot be included in AssetBundles (LightingData)
                            if (!recursive)
                            {
                                foreach (var p in AssetDatabase.GetDependencies(path, false))
                                {
                                    if (p == path || p == pathName)
                                        continue;
                                    list.Add(p);
                                }
                            }

                            list.RemoveAt(i);
                            break;
                        }
                    }

                    // depends = list;
                    depends = list.ToArray();
                }

                // return depends.ToArray();
                return depends;
            }

            private void ConvertCombinedToAssets()
            {
                var usedNames = new HashSet<string>();
                foreach (var package in packages)
                {
                    usedNames.Add(package.name);
                    var arr = new string[package.assets.Count];
                    package.assets.CopyTo(arr);
                    foreach (var asset in arr)
                    {
                        Asset assetInfo = null;
                        if (m_AssetInfos.TryGetValue(asset, out assetInfo))
                        {
                            if (assetInfo is CombineAsset)
                            {
                                package.assets.Remove(asset);
                                package.assets.UnionWith(((CombineAsset) assetInfo).assets);
                            }
                        }
                    }
                }

                // 优化Package的名字
                var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                foreach (var package in packages)
                {
                    if (package.name.StartsWith("SharedAssets/"))
                    {
                        // 使用资源列表的md5作为SharedAssets的名称
                        var assetList = new List<string>(package.assets);
                        if (assetList.Count == 1)
                        {
                            package.name = m_Options.sharedDirectoryName +
                                           assetList[0].Substring(6).Replace("\\", "/").Replace(" ", "_") +
                                           SparkHelper.SparkAssetExtension;
                        }
                        else
                        {
                            assetList.Sort();
                            package.name =
                                $"{m_Options.sharedDirectoryName}/{BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(string.Join("_", assetList.ToArray())))).Replace("-", "").ToLower()}{SparkHelper.SparkAssetExtension}";
                        }

                        var pkgName = "";
                        var hasScene = false;
                        foreach (var s in package.assets)
                        {
                            if (m_ExportableAssets.TryGetValue(s, out var name))
                            {
                                if (!hasScene)
                                {
                                    hasScene = s.ToLower().EndsWith(".unity");
                                }

                                if (string.IsNullOrEmpty(pkgName))
                                {
                                    pkgName = name;
                                }
                                else if (pkgName != name)
                                {
                                    pkgName = string.Empty;
                                    break;
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(pkgName))
                        {
                            if (usedNames.Contains(pkgName))
                            {
                                if (!hasScene)
                                    continue;
                                var p = packages.Find((pkg) => pkg.name == pkgName);
                                if (p != null)
                                {
                                    p.name = package.name;
                                }
                            }

                            Debug.LogFormat("Pkg Name: {0} => {1}", package.name, pkgName);
                            usedNames.Add(pkgName);
                            package.name = pkgName;
                        }
                    }
                }
            }

            private void CollectDependencies(string assetPath, Package package, bool exportable)
            {
                if (exportable)
                {
                    if (m_ExportableAssets.TryGetValue(assetPath, out var oldName))
                    {
                        if (oldName != package.name)
                        {
                            m_ExportableAssets[assetPath] = string.Empty;
                        }
                    }
                    else
                    {
                        m_ExportableAssets.Add(assetPath, package.name);
                    }
                }

                // if (CollectAssetPath(assetPath, package))
                // {
                //     foreach (var path in GetDependencies(assetPath, false))
                //     {
                //         if (path != assetPath)
                //         {
                //             var lpath = path.ToLower();
                //             if (lpath.EndsWith(".cs") || lpath.EndsWith(".js") || lpath.EndsWith(".dll"))
                //             {
                //                 // 忽略脚本文件
                //                 continue;
                //             }
                //
                //             // 记录依赖关系
                //             if (!m_AssetReverseDependencies.TryGetValue(path, out var hashSet))
                //             {
                //                 hashSet = new HashSet<string>();
                //                 m_AssetReverseDependencies[path] = hashSet;
                //             }
                //
                //             hashSet.Add(assetPath);
                //             // 递归查找依赖
                //             CollectDependencies(path, package, false);
                //         }
                //     }
                // }
            }

            private bool CollectAssetPath(string assetPath, Package package)
            {
                if (!m_AssetInfos.TryGetValue(assetPath, out var assetInfo))
                {
                    // var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                    // if (importer != null)
                    // {
                    //     if (importer.textureType == TextureImporterType.Sprite)
                    //     {
                    //         var tag = importer.spritePackingTag;
                    //         if (!string.IsNullOrEmpty(tag))
                    //         {
                    //             tag = "[SP]" + tag;
                    //             if (!m_CombinedAssets.TryGetValue(tag, out var combined))
                    //             {
                    //                 combined = new HashSet<string>();
                    //                 m_CombinedAssets.Add(tag, combined);
                    //             }
                    //
                    //             combined.Add(assetPath);
                    //         }
                    //         else
                    //         {
                    //             m_NonTaggedTextures.Add(assetPath);
                    //         }
                    //     }
                    // }

                    assetInfo = new SingleAsset(assetPath);
                    m_AssetInfos.Add(assetPath, assetInfo);
                }

                return assetInfo.packages.Add(package);
            }
        }
    }

    static class EnumerableExtension
    {
        public static bool IsIntersectOf<T>(this ICollection<T> first, ICollection<T> second)
        {
            ICollection<T> a, b;
            if (first.Count > second.Count)
            {
                b = first;
                a = second;
            }
            else
            {
                a = first;
                b = second;
            }

            foreach (var o in a)
            {
                if (b.Contains(o))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
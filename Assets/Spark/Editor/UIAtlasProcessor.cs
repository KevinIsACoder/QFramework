using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Spark;
using UnityEngine.U2D;
using UnityEditor.U2D;

namespace SparkEditor
{
	public class UIAtlasProcessor : AssetPostprocessor
	{
		public enum TagMode
		{
			None,
			Auto,
			//Custom,
			Single,
			Group,
		}

		public struct CompressionSettings
		{
			public int compressionQuality;
#if UNITY_5_5_OR_NEWER
			public bool crunchedCompression;
			public TextureImporterCompression textureCompression;
#endif
		}

		public struct PlatformSettings
		{
			public int maxTextureSize;
			public TextureImporterFormat textureFormat;
			public HashSet<TextureImporterFormat> allowedTextureFormats;
			public bool? allowsAlphaSplit;
			public CompressionSettings? compression;
		}

		public struct SpriteSettings
		{
			public string tag;
			public TagMode tagMode;
			public bool mipmapEnabled;
			public bool isNonSprite;
			public PlatformSettings? android;
			public PlatformSettings? ios;
			public PlatformSettings? standalone;
			public CompressionSettings compression;
#if !UNITY_5_5_OR_NEWER
			public TextureImporterFormat format;
#endif
		}

		public struct AtlasSettings
		{
			public string name;
			public string atlasPath;
			public bool isRoot;
			public bool recursive;
			public bool deleteIfEmpty;
			public List<string> excludes;
			public List<string> includes;
		}

		public struct Rule
		{
			public string assetPath;
			public AtlasSettings? atlasSettings;
			public SpriteSettings spriteSettings;
		}

		static private List<Rule> m_Rules = new List<Rule>();
		static private HashSet<Rule> m_RuleDirtyHashSet = new HashSet<Rule>();
		static private Dictionary<string, Rule> m_AssetDirtyDict = new Dictionary<string, Rule>();

		public static void AddRule(Rule rule)
		{
			m_Rules.Add(rule);
		}

		static void UpdateSpriteAtlasPlatformSettings(SpriteAtlas spriteAtlas, string platform, PlatformSettings? settings)
        {
			var platformSettings = spriteAtlas.GetPlatformSettings(platform);
			if (settings.HasValue)
            {
				var value = settings.Value;
				platformSettings.overridden = true;
				platformSettings.maxTextureSize = 2048; //value.maxTextureSize == 0 ? 1024 : value.maxTextureSize;
				var format = platformSettings.format;
				var allowedTextureFormats = value.allowedTextureFormats;
				if (allowedTextureFormats == null || allowedTextureFormats.Count == 0 || !allowedTextureFormats.Contains(format))
				{
					platformSettings.format = value.textureFormat;
				}
				if (value.allowsAlphaSplit.HasValue)
                {
					platformSettings.allowsAlphaSplitting = value.allowsAlphaSplit.Value;
                }
				if (value.compression.HasValue)
				{
					var compression = value.compression.Value;
					platformSettings.compressionQuality = compression.compressionQuality;
					platformSettings.crunchedCompression = compression.crunchedCompression;
					platformSettings.textureCompression = compression.textureCompression;
                }
			}
			else
            {
				platformSettings.overridden = false;
            }
			spriteAtlas.SetPlatformSettings(platformSettings);
		}

		static void UpdatePlatformTextureSettings(TextureImporter importer, string platform, PlatformSettings? settings)
		{
			if (settings.HasValue) {
				var platformSettings = settings.Value;
				int maxTextureSize = platformSettings.maxTextureSize;
				if (maxTextureSize == 0) {
					maxTextureSize = importer.maxTextureSize;
				}
#if UNITY_5_5_OR_NEWER
				var importerSettings = importer.GetPlatformTextureSettings(platform);
				importerSettings.overridden = true;
				importerSettings.maxTextureSize = maxTextureSize;
				var format = importerSettings.format;
				var allowedTextureFormats = platformSettings.allowedTextureFormats;
				if (allowedTextureFormats == null || allowedTextureFormats.Count == 0 || !allowedTextureFormats.Contains(format)) {
					importerSettings.format = platformSettings.textureFormat;
				}
				if (platformSettings.compression.HasValue) {
					var compression = platformSettings.compression.Value;
					importerSettings.compressionQuality = compression.compressionQuality;
					importerSettings.crunchedCompression = compression.crunchedCompression;
					importerSettings.textureCompression = compression.textureCompression;
				} else {
					importerSettings.compressionQuality = importer.compressionQuality;
					importerSettings.crunchedCompression = importer.crunchedCompression;
					importerSettings.textureCompression = importer.textureCompression;
				}
				if (platformSettings.allowsAlphaSplit.HasValue) {
					importerSettings.allowsAlphaSplitting = platformSettings.allowsAlphaSplit.Value;
				}
				importer.SetPlatformTextureSettings(importerSettings);
#else
				importer.SetPlatformTextureSettings(platform, maxTextureSize, platformSettings.textureFormat, platformSettings.compression.HasValue ? platformSettings.compression.Value.compressionQuality : 100, platformSettings.allowsAlphaSplit.HasValue ? platformSettings.allowsAlphaSplit.Value : false);
#endif
			} else {
				importer.ClearPlatformTextureSettings(platform);
			}
		}

        static SpriteAtlas GetOrCreateSpriteAtlasAsset<T>(string assetPath) where T:UnityEngine.Object
        {
			var dir = "Assets/_GEN/SpriteAtlases";
			if (!Directory.Exists(dir))
            {
				Directory.CreateDirectory(dir);
            }
			var file = dir + "/" + assetPath.Replace("/", "_").Replace(".", "_") + ".spriteatlas";
			var spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(file);
			if (spriteAtlas == null)
            {
				spriteAtlas = new SpriteAtlas();
				AssetDatabase.CreateAsset(spriteAtlas, file);
			} else {
				var objects = spriteAtlas.GetPackables();
				if (objects != null && objects.Length > 0)
                {
					spriteAtlas.Remove(objects);
                }
			}
			var obj = AssetDatabase.LoadAssetAtPath<T>(assetPath);
			if (obj != null)
			{
				spriteAtlas.Add(new UnityEngine.Object[] { obj });
			}
			return spriteAtlas;
        }

        static void UpdateTextureImporter(string assetPath, TextureImporter importer, bool reimport)
		{
			foreach (var rule in m_Rules) {
				var relativePath = assetPath.Replace(rule.assetPath + "/", "");
				if (relativePath == assetPath)
					continue;

				var atlasPath = rule.assetPath;
				if (rule.atlasSettings.HasValue) {
					AtlasSettings atlasSettings = rule.atlasSettings.Value;
					int index = relativePath.IndexOf("/");
					if (atlasSettings.isRoot) {
						if (index < 0)
							continue;
						if (atlasSettings.excludes != null && atlasSettings.excludes.Contains(relativePath.Substring(0, index)))
							continue;
						if (atlasSettings.includes != null && !atlasSettings.includes.Contains(relativePath.Substring(0, index)))
							continue;
					}
					if (!atlasSettings.recursive && relativePath.IndexOf("/", index + 1) >= 0)
						continue;
					atlasPath = atlasSettings.isRoot ? (rule.assetPath + "/" + relativePath.Substring(0, index)) : rule.assetPath;
				}

				var packingTag = string.Empty;
				var settings = rule.spriteSettings;
				if (settings.tagMode == TagMode.Single) {
					packingTag = "~" + assetPath.Replace('/', '.');
				} else if (settings.tagMode == TagMode.Group) {
					packingTag = atlasPath.Replace('/', '.');
				} else if (settings.tagMode == TagMode.Auto) {
					int width, height;
					GetOriginalTextureSize(importer, out width, out height);
					int textureSize = Mathf.Max(width, height);
					if (textureSize > importer.maxTextureSize) {
						float rate = importer.maxTextureSize * 1.0f / textureSize;
						width = Mathf.CeilToInt(width * rate);
						height = Mathf.CeilToInt(height * rate);
					}
					if (width == 1024 && height == 1024) {
						packingTag = string.Empty;
					} else {
						if (width % 4 == 0 && height % 4 == 0) {
							packingTag = string.Empty;
						} else {
							packingTag = "~" + assetPath.Replace('/', '.');
						}
					}
				}

				// Sprite默认为FullRect
				var ti = new TextureImporterSettings();
				importer.ReadTextureSettings(ti);
				ti.textureType = TextureImporterType.Sprite;
				ti.spriteMeshType = SpriteMeshType.FullRect;
				ti.spriteGenerateFallbackPhysicsShape = false;
				importer.SetTextureSettings(ti);
				
				if (!string.IsNullOrEmpty(packingTag) && settings.tagMode != TagMode.None)
				{
					//SpriteAtlas atlas;
                    // 不规则的尺寸，需要生成atlas
                    if (packingTag.StartsWith("~"))
                    {
                        // 单个贴图生成atlas
                        if (!m_AssetDirtyDict.ContainsKey(assetPath))
							m_AssetDirtyDict.Add(assetPath, rule);
					}
                    else
                    {
						// 大图atlas
						if (!m_AssetDirtyDict.ContainsKey(atlasPath))
							m_AssetDirtyDict.Add(atlasPath, rule);
					}
					importer.spritePackingTag = packingTag;
				}
                else
                {
					importer.spriteImportMode = settings.isNonSprite ? SpriteImportMode.None : SpriteImportMode.Single;
					importer.mipmapEnabled = settings.mipmapEnabled;
					importer.spritePackingTag = packingTag;
#if UNITY_5_5_OR_NEWER
                    importer.textureCompression = settings.compression.textureCompression;
					importer.crunchedCompression = settings.compression.crunchedCompression;
					importer.compressionQuality = settings.compression.compressionQuality;
#else
					importer.textureFormat = settings.format;
#endif

					UpdatePlatformTextureSettings(importer, "Android", settings.android);
					UpdatePlatformTextureSettings(importer, "iPhone", settings.ios);
					UpdatePlatformTextureSettings(importer, "Standalone", settings.standalone);
					importer.SaveAndReimport();
				}

				if (reimport)
				{
					importer.SaveAndReimport();
				}

				break;
			}

			// 
		}
		private static void GetOriginalTextureSize(TextureImporter importer, out int width, out int height)
		{
			object[] args = new object[2] { 0, 0 };
			System.Reflection.MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			mi.Invoke(importer, args);

			width = (int)args[0];
			height = (int)args[1];
		}

#if !SPARK_AUTO_BUILD
		void OnPreprocessTexture()
		{
			UpdateTextureImporter(assetPath, (TextureImporter)assetImporter, false);
        }

		private static void DeleteUnusedAtlases()
        {
			var dir = "Assets/_GEN/SpriteAtlases";
			if (Directory.Exists(dir))
            {
				var files = Directory.GetFiles(dir, "*.spriteatlas", SearchOption.TopDirectoryOnly);
				foreach (var file in files)
				{
					var importer = AssetImporter.GetAtPath(file);
					var assetPath = importer.userData;
					if (Directory.Exists(assetPath) || File.Exists(assetPath))
					{
						if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) != null)
						{
							var ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
							// 如果贴图未设置了tag，认为是符合压缩规范的，不用生成atlas
							if (ti == null || !string.IsNullOrEmpty(ti.spritePackingTag))
								continue;
						}
					}
					AssetDatabase.DeleteAsset(file);
				}
			}
		}

		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
		{
			bool dirty = false;
			if (movedAssets.Length > 0) {
				AssetDatabase.StartAssetEditing();
				foreach (var path in movedAssets) {
					var importer = TextureImporter.GetAtPath(path) as TextureImporter;
					if (importer != null) {
						UpdateTextureImporter(path, importer, true);
					}
				}
				AssetDatabase.StopAssetEditing();
			}
			foreach (var rule in m_Rules) {
				if (rule.atlasSettings.HasValue) {
					if (rule.atlasSettings.Value.isRoot) {
						foreach (var childPath in AssetDatabase.GetSubFolders(rule.assetPath)) {
							AtlasSettings atlasSettings = rule.atlasSettings.Value;
							var dirName = Path.GetFileNameWithoutExtension(childPath);
							if (atlasSettings.excludes == null || !atlasSettings.excludes.Contains(dirName)) {
								bool changed = CheckAssetsChanged(childPath, importedAssets, deletedAssets, movedAssets, movedFromPath);
								if (changed) {
									var childRule = rule;
									if (!string.IsNullOrEmpty(atlasSettings.name)) {
										atlasSettings.name = atlasSettings.name + dirName;//.ToUpperFirst();
									} else {
										atlasSettings.name = dirName;//.ToUpperFirst();
									}
									atlasSettings.excludes = null;
									childRule.assetPath = childPath;
									childRule.atlasSettings = atlasSettings;
									m_RuleDirtyHashSet.Add(childRule);
								}
							}
						}
					} else {
						bool changed = CheckAssetsChanged(rule.assetPath, importedAssets, deletedAssets, movedAssets, movedFromPath);
						if (changed) {
							m_RuleDirtyHashSet.Add(rule);
						}
					}
                }
                else
				{
					bool changed = CheckAssetsChanged(rule.assetPath, importedAssets, deletedAssets, movedAssets, movedFromPath);
					if (changed)
					{
						dirty = true;
					}
				}
			}
			if (m_RuleDirtyHashSet.Count > 0 || m_AssetDirtyDict.Count > 0 || dirty) {
				EditorApplication.delayCall = () => {
					if (m_AssetDirtyDict.Count > 0)
                    {
						foreach(var kv in m_AssetDirtyDict)
                        {
							var atlas = GetOrCreateSpriteAtlasAsset<UnityEngine.Object>(kv.Key);
							atlas.SetIncludeInBuild(true);

							var textureSettings = atlas.GetTextureSettings();
							textureSettings.generateMipMaps = false;
							atlas.SetTextureSettings(textureSettings);

							var packingsettings = atlas.GetPackingSettings();
							packingsettings.enableTightPacking = false;
							packingsettings.enableRotation = false;
							atlas.SetPackingSettings(packingsettings);

							UpdateSpriteAtlasPlatformSettings(atlas, "Android", kv.Value.spriteSettings.android);
							UpdateSpriteAtlasPlatformSettings(atlas, "iPhone", kv.Value.spriteSettings.ios);
							UpdateSpriteAtlasPlatformSettings(atlas, "Standalone", kv.Value.spriteSettings.standalone);

							var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(atlas));
							importer.userData = kv.Key;
							importer.SaveAndReimport();
						}
						m_AssetDirtyDict.Clear();
					}

					//if (dirty)
     //               {
					//	dirty = false;
					// 清除无用的atlas
					DeleteUnusedAtlases();
					//}

					if (m_RuleDirtyHashSet.Count > 0)
                    {
						Debug.Log("<color=olive>[UIAtlas-AutoGenerated] Detect some assets changed, repacking ...</color>");
						Rule[] rules = m_RuleDirtyHashSet.ToArray();
						m_RuleDirtyHashSet.Clear();
						foreach (var rule in rules)
						{
							AtlasSettings atlasSettings = rule.atlasSettings.Value;
							var atlasPath = atlasSettings.atlasPath + "/" + atlasSettings.name + ".prefab";
							if (Directory.Exists(rule.assetPath))
							{
								var sprites = SparkEditorHelper.GetAssetsAtPath<Sprite>(rule.assetPath, atlasSettings.recursive);
								if (sprites.Length > 0 || !atlasSettings.deleteIfEmpty)
								{
									UIAtlasBuilder.Build<Spark.UIAtlas>(sprites, atlasPath);
									continue;
								}
							}
							AssetDatabase.DeleteAsset(atlasPath);
						}
					}
                };
			}
		}
#endif

		private static bool CheckAssetsChanged(string assetPath, string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
		{
			assetPath = assetPath + "/";
			foreach (var p in importedAssets) {
				if (p.Contains(assetPath)) {
					return true;
				}
			}
			foreach (var p in deletedAssets) {
				if (p.Contains(assetPath)) {
					return true;
				}
			}
			foreach (var p in movedAssets) {
				if (p.Contains(assetPath)) {
					return true;
				}
			}
			foreach (var p in movedFromPath) {
				if (p.Contains(assetPath)) {
					return true;
				}
			}
			return false;
		}
	}
}

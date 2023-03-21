using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

public class AutoSetAtlasContent : Editor
{
    //private static string _atlasPath = "Assets/Atlas/Atlas.spriteatlas";
    private static string[] _texturePath = new string[] { "Assets/Image/Image_1", "Assets/Image/Image_2" };

    [MenuItem("AbBuilder/AutoSetAtlas")]
    static void AutoSetAtlasContents()
    {
        for (int i = 0; i < _texturePath.Length; i++)
        {
            SpriteAtlas atlas = new SpriteAtlas();
            // 设置参数 可根据项目具体情况进行设置
            SpriteAtlasPackingSettings packSetting = new SpriteAtlasPackingSettings()
            {
                blockOffset = 1,
                enableRotation = false,
                enableTightPacking = false,
                padding = 2,
            };
            atlas.SetPackingSettings(packSetting);

            SpriteAtlasTextureSettings textureSetting = new SpriteAtlasTextureSettings()
            {
                readable = false,
                generateMipMaps = false,
                sRGB = true,
                filterMode = FilterMode.Bilinear,
            };
            atlas.SetTextureSettings(textureSetting);

            TextureImporterPlatformSettings platformSetting = new TextureImporterPlatformSettings()
            {
                maxTextureSize = 2048,
                format = TextureImporterFormat.Automatic,
                crunchedCompression = true,
                textureCompression = TextureImporterCompression.Compressed,
                compressionQuality = 50,
            };
            atlas.SetPlatformSettings(platformSetting);

            atlas.SetIncludeInBuild(true);

            var _atlasPath = "Assets/Atlas/" + _texturePath[i].Substring(_texturePath[i].LastIndexOf("/") + 1) + ".spriteatlas";
            AssetDatabase.CreateAsset(atlas, _atlasPath);

            // 1、添加文件
            // DirectoryInfo dir = new DirectoryInfo(_texturePath[i]);
            // // 这里我使用的是png图片，已经生成Sprite精灵了
            // FileInfo[] files = dir.GetFiles("*.png");
            // foreach (FileInfo file in files)
            // {
            //     atlas.Add(new[] {AssetDatabase.LoadAssetAtPath<Sprite>($"{_texturePath}/{file.Name}")});
            // }

            // 2、添加文件夹
            Object obj = AssetDatabase.LoadAssetAtPath(_texturePath[i], typeof(Object));
            atlas.Add(new[] { obj });

            AssetDatabase.SaveAssets();

        }
    }
}
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SparkEditor
{

    //******************************************
    // TextureSpritePackingTagCleaner
    //
    // @Author: Kakashi
    // @Email: john.cha@qq.com
    // @Date: 2023-01-09 23:46
    //******************************************
    public class TextureSpritePackingTagCleaner
    {
        [MenuItem("StarX/Sprite Atlas/Clean selections")]
        public static void Clean()
        {
            var objs = Selection.objects;
            if (objs == null)
            {
                Debug.Log("Clean nothing. Please select textures or folders to clean");
                return;
            }

            foreach (var obj in objs)
            {
                _Clean(obj);
            }
        }

        private static void _Clean(Object o)
        {
            var path = AssetDatabase.GetAssetPath(o);
            
            var t = o as Texture;
            if (t != null)
            {
                TextureImporter importer = TextureImporter.GetAtPath(path) as TextureImporter;
                if (!string.IsNullOrEmpty(importer.spritePackingTag))
                {
                    importer.spritePackingTag = "";
                    importer.SaveAndReimport();    
                }
                return;
            }

            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path);
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        _Clean(AssetDatabase.LoadAssetAtPath<Object>(file));
                    }
                }
                
                var directories = Directory.GetDirectories(path);
                if (directories != null)
                {
                    foreach (var directory in directories)
                    {
                        _Clean(AssetDatabase.LoadAssetAtPath<Object>(directory));
                    }
                }
            }
        }
    }
}
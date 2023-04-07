using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
public class AbTest : MonoBehaviour
{
    // Start is called before the first frame update
    private AssetBundle bundle;
    public GameObject canvas;
    private Image _image;
    private void OnEnable()
    {
        SpriteAtlasManager.atlasRequested += OnRequest;
    }

    void Start()
    {
        bundle = AssetBundle.LoadFromFile("Assets/StreamingAssets/prefabs");
        var imgObj = LoadAssets("", "Assets/GameAssets/Prefabs/Image.prefab");
        _image = imgObj.GetComponent<Image>();
        // bundle = AssetBundle.LoadFromFile("Assets/StreamingAssets/atlas");
        // bundle.LoadAsset<SpriteAtlas>("Assets/GameAssets/Atlas/Image_1.spriteatlas");
    }

    void OnRequest(string tag, Action<SpriteAtlas> cb)
    {
        // var atlasbundle = AssetBundle.LoadFromFile("Assets/StreamingAssets/atlas");
        //  //bundle.LoadAsset<SpriteAtlas>("Assets/GameAssets/Atlas/Image_1.spriteatlas");
        // Debug.Log("OnRequest++++++++++++++++");
        // var atlas = atlasbundle.LoadAsset<SpriteAtlas>("Assets/GameAssets/Atlas/Image_1.spriteatlas");
        SpriteAtlas atlas = null;
        var atlasbundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/atlas");
        atlas = atlasbundle.LoadAsset<SpriteAtlas>("Assets/GameAssets/Atlas/Image_1.spriteatlas");
// #if UNITY_EDITOR
//        // atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>("Assets/GameAssets/Atlas/Image_1.spriteatlas");
//        Debug.Log(atlas);
// #else
//         var atlasbundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/atlas");
//         atlas = atlasbundle.LoadAsset<SpriteAtlas>("Assets/GameAssets/Atlas/Image_1.spriteatlas");
// #endif
        cb(atlas);
    }
    

    GameObject LoadAssets(string bundlePath, string assetPath)
    {
        if(!string.IsNullOrEmpty(assetPath))
        {
            var obj = bundle.LoadAsset<GameObject>(assetPath);
            var UIPanel = Instantiate(obj);
            UIPanel.transform.SetParent(canvas.transform, false);
            UIPanel.transform.localPosition = Vector3.zero;
            UIPanel.transform.localScale = Vector3.one;
            UIPanel.transform.localRotation = Quaternion.identity;
            UIPanel.name = "Image";
            return UIPanel;
        }

        return null;
    }
}

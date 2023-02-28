using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        LoadAssets("Assets/StreamingAssets/image", "");
        LoadAssets("Assets/StreamingAssets/prefabs", "Assets/Prefabs/Canvas.prefab");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LoadAssets(string bundlePath, string assetPath)
    {
        var bundle = AssetBundle.LoadFromFile(bundlePath);
        if(!string.IsNullOrEmpty(assetPath))
        {
            var obj = bundle.LoadAsset<GameObject>(assetPath);
            var UIPanel = Instantiate(obj);
        }
    }
}

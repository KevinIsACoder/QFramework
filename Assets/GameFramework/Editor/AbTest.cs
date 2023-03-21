using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //LoadAssets("Assets/StreamingAssets/image", "");
        LoadAssets("Assets/StreamingAssets/prefabs", "Assets/Prefabs/UIPanel.prefab");
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
            for (int i = 0; i < 10; i++)
            {
                var UIPanel = Instantiate(obj);   
                UIPanel.name = "UIPanel_" + i;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AbTest : MonoBehaviour
{
    // Start is called before the first frame update
    private AssetBundle bundle;
    void Start()
    {
        bundle = AssetBundle.LoadFromFile("Assets/StreamingAssets/prefabs");
        //LoadAssets("Assets/StreamingAssets/image", "");

        LoadAssets("", "Assets/Prefabs/UIPanel_1.prefab");
        LoadAssets("", "Assets/Prefabs/UIPanel_2.prefab");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LoadAssets(string bundlePath, string assetPath)
    {
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

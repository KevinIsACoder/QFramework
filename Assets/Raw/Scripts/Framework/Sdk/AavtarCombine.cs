using System.Collections.Generic;
using KLFramework.Avatar;
using UnityEngine;

public class AavtarCombine : MonoBehaviour
{
    public Transform skeleton;

    void Awake()
    {
        var parts = new List<Transform>();
        for (var i = 0; i < transform.childCount; i++)
        {
            var part = transform.GetChild(i);
            parts.Add(part);
        }
        
        var avatar = new BasicAvatar<int>();
        avatar.GetGameObject().transform.SetParent(transform, false);
        avatar.SetSkeleton(new BasicSkeleton(skeleton));
        var partType = 0;
        foreach (var part in parts)
        {
            //part.SetParent(avatar.GetTransform(), false);
            if (part == skeleton || part.name == "LookAt") continue;
            avatar.AddPart(partType++, new SkinnedMeshRendererPart<int>(0, part.gameObject));
        }
        avatar.ApplyChange();
    }
    
}

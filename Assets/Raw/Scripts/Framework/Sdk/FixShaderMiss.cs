using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FixShaderMiss : MonoBehaviour
{
    void Start()
    {
        var pss = GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in pss)
        {
            var com = ps.GetComponent<Renderer>();
            if (com == null || !com.enabled) continue;
            var mat = com.material;
            if (mat == null) continue;
            var target = Shader.Find(mat.shader.name);
            if (target)
            {
                mat.shader = target;    
            }
            else
            {
                Debug.LogError(gameObject.name +  " not found the shader: " + mat.shader.name);
            }
        }
        
        var meshes = new List<Renderer>();
        meshes.AddRange(GetComponentsInChildren<MeshRenderer>(true));
        meshes.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>(true));
        foreach (var mat in meshes.Select(mesh => mesh.material).Where(mat => mat != null))
        {
            var target = Shader.Find(mat.shader.name);
            if (target)
            {
                mat.shader = target;    
            }
            else
            {
                Debug.LogError(gameObject.name +  " not found the shader: " + mat.shader.name);
            }
        }
    }
}

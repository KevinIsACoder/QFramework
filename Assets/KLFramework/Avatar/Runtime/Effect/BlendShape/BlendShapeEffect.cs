using UnityEngine;

namespace KLFramework.Avatar
{

    //******************************************
    // BlendShapeEffect
    //
    // @Author: Kakashi
    // @Email: john.cha@qq.com
    // @Date: 2022-07-30 16:30
    //******************************************
    public class BlendShapeEffect : IEffect
    {
        private SkinnedMeshRenderer[] _Renderers;
        
        public BlendShapeEffect(GameObject go) : this(go.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
        }
        public BlendShapeEffect(SkinnedMeshRenderer[] renderers)
        {
            _Renderers = renderers;
        }

        public void SetBlendShapeWeight(int index, float value)
        {
            for (int i = 0; i < _Renderers.Length; i++)
            {
                _Renderers[i].SetBlendShapeWeight(index, value);
            }
        }
        
        public float GetBlendShapeWeight(int index)
        {
            return _Renderers[0].GetBlendShapeWeight(index);
        }

        public void Apply()
        {
        }
    }
}
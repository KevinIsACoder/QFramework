using System;
using System.Collections.Generic;
using UnityEngine;

namespace KLFramework.Avatar
{

    //******************************************
    // UniformTransformEffect
    //
    // @Author: Kakashi
    // @Email: john.cha@qq.com
    // @Date: 2022-07-28 17:09
    //******************************************
    public class MaterialEffect : IEffect
    {
        private Renderer[] _Renderers;
        private List<IMaterialArg> _MaterialArgs = new List<IMaterialArg>(); 
        private List<Material> _Materials = new List<Material>();
        private bool _Dirty;
        
        public MaterialEffect(GameObject go) : this(go.GetComponentsInChildren<Renderer>())
        {
        }
        
        public MaterialEffect(Renderer[] renderers)
        {
            _Renderers = renderers;
        }

        public List<IMaterialArg> GetMaterialArgs()
        {
            return _MaterialArgs;
        }

        public MaterialEffect AddMaterialArg(IMaterialArg materialArg)
        {
            _Dirty = true;
            _MaterialArgs.Add(materialArg);
            return this;
        }

        public MaterialEffect AddMaterialArgs(IMaterialArg[] materialArgs)
        {
            _Dirty = true;
            foreach (var materialArg in materialArgs)
            {
                _MaterialArgs.Add(materialArg);
            }
            return this;
        }

        public MaterialEffect RemoveMaterialArg(IMaterialArg materialArg)
        {
            _Dirty = true;
            _MaterialArgs.Remove(materialArg);
            return this;
        }

        public MaterialEffect RemoveMaterialArgs(IMaterialArg[] materialArgs)
        {
            _Dirty = true;
            foreach (var materialArg in materialArgs)
            {
                _MaterialArgs.Remove(materialArg);
            }
            return this;
        }

        public MaterialEffect ClearMaterialArg()
        {
            _Dirty = true;
            _MaterialArgs.Clear();
            return this;
        }

        private void _TryFilterMaterials()
        {
            if (!_Dirty || _Renderers == null)
            {
                return;
            }
            _Dirty = false;

            _Materials.Clear();
            for (int i = 0; i < _Renderers.Length; i++)
            {
                var materials = _Renderers[i].materials;
                for (int j = 0; j < materials.Length; j++)
                {
                    var mat = materials[j];
                    if (mat == null || !_HasProperty(mat))
                    {
                        continue;
                    }
                    _Materials.Add(mat);
                }
            }
        }

        private bool _HasProperty(Material material)
        {
            for (int i = 0; i < _MaterialArgs.Count; i++)
            {
                if (material.HasProperty(_MaterialArgs[i].PropertyID))
                {
                    return true;
                }
            }
            return false;
        }

        public void Apply()
        {
            _TryFilterMaterials();
            for (int i = 0; i < _Materials.Count; i++)
            {
                var mat = _Materials[i];
                for (int j = 0; j < _MaterialArgs.Count; j++)
                {
                    _MaterialArgs[j].Apply(mat);
                }
            }
        }
    }
}
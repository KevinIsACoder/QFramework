using System;
using System.Collections.Generic;
using UnityEngine;

namespace KLFramework.Avatar
{
    public class MaterialItem
    {
        private List<IMaterialArg> _MaterialArgs = new List<IMaterialArg>();
        private Dictionary<int, IMaterialArg> _MaterialArgsMap = new Dictionary<int, IMaterialArg>();
        private Material[] _materials;

        public MaterialItem(Material[] mat)
        {
            _materials = mat;
        }

        public void AddMaterialArgs(int propertyId, IMaterialArg materialArg)
        {
            if(_MaterialArgsMap.ContainsKey(propertyId))
            {
                var oldValue = _MaterialArgsMap[propertyId].Value;
                var newValue = materialArg.Value;
                if(materialArg is MaterialTextureArgs)
                {
                    if(oldValue != newValue)
                        materialArg.Dirty = true;
                }
                else if(materialArg is MaterialFloatArg)
                {
                    if(!Mathf.Approximately((float)newValue, (float)oldValue))
                        materialArg.Dirty = true;
                }
                else if(materialArg is MaterialColorArg)
                {   
                    Color oldColor = (Color)oldValue;
                    Color newColor = (Color)newValue;
                    if(!(Mathf.Approximately(newColor.r, oldColor.r) && Mathf.Approximately(newColor.g, oldColor.g) && Mathf.Approximately(newColor.b, oldColor.b)))
                      materialArg.Dirty = true;
                }
            }
            else
            {
                materialArg.Dirty = true;
            }
            _MaterialArgsMap[propertyId] = materialArg;
            Apply();
        }

        public void Apply()
        {
            foreach (var item in _MaterialArgsMap)
            {
                if(item.Value.Dirty)
                {
                    foreach (var material in _materials)
                    {
                        item.Value.Apply(material);   
                    }
                }
            }
        }
    }

    public class MultiMaterialEffect : IEffect
    {
        private Renderer[] _Renderers;
        private Material[] _Materials;

        private List<IMaterialArg> _MaterialArgs = new List<IMaterialArg>();
        
        Dictionary<int, MaterialItem> m_materialEffects = new Dictionary<int, MaterialItem>();

        private bool _Dirty;
        
        private GameObject _Go;
    
        public MultiMaterialEffect(GameObject go) : this(go.GetComponentsInChildren<Renderer>())
        {
            _Go = go;
        }

        public MultiMaterialEffect(Renderer[] renderer)
        {
            _Renderers = renderer;
            var materials = new List<Material>();
            for (int i = 0; i < _Renderers.Length; i++)
            {
                materials.AddRange(_Renderers[i].materials);
            }
            _Materials = materials.ToArray();
        }
        
        public List<IMaterialArg> GetMaterialArgs()
        {
            return _MaterialArgs;
        }

        public GameObject GetMaterialObj()
        {
            return _Go;
        }

        public MultiMaterialEffect AddMaterialArg(int partId, IMaterialArg materialArg, string materialName = null)
        {
            MaterialItem materiaItem;
            if(m_materialEffects.TryGetValue(partId, out materiaItem) == false)
            {
                var materials = _TryFilterMaterials(materialArg, materialName);
                materiaItem = new MaterialItem(materials);
                m_materialEffects[partId] = materiaItem;
            }
            m_materialEffects[partId].AddMaterialArgs(materialArg.PropertyID, materialArg);
            return this;
        }

        public MultiMaterialEffect AddMaterialArgs(IMaterialArg[] materialArgs)
        {
            foreach (var materialArg in materialArgs)
            {
                _MaterialArgs.Add(materialArg);
            }
            return this;
        }

        public MultiMaterialEffect RemoveMaterialArg(IMaterialArg materialArg)
        {
            _MaterialArgs.Remove(materialArg);
            return this;
        }

        public MultiMaterialEffect RemoveMaterialArgs(IMaterialArg[] materialArgs)
        {
            foreach (var materialArg in materialArgs)
            {
                _MaterialArgs.Remove(materialArg);
            }
            return this;
        }

        public MultiMaterialEffect ClearMaterialArg()
        {
            for (int i = 0; i < _Materials.Length; i++)
            {
                GameObject.Destroy(_Materials[i]);
            }
            _MaterialArgs.Clear();
            m_materialEffects.Clear();
            return this;
        }

        private Material[] _TryFilterMaterials(IMaterialArg materialArg, string materialName = null)
        {
            var materials = new List<Material>();
            foreach (var item in _Materials)
            {
                if(item.HasProperty(materialArg.PropertyID))
                {
                    if(!string.IsNullOrEmpty(materialName))
                    {   
                        if(item.name.Contains(materialName))
                        {
                            materials.Add(item);
                        }
                    }
                    else
                    {
                        materials.Add(item);
                    }
                }
            }
            return materials.ToArray();
        }

        public void Apply()
        {

        }
    }
}
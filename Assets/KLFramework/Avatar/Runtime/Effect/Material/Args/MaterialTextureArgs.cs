using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KLFramework.Avatar
{
    public class MaterialTextureArgs:AbstractMaterialArg
    {
        public MaterialTextureArgs(string propertyName) : base(propertyName)
        {

        }

        public override object Value { get; set;}

        public override void Apply(Material material)
        {
            material.SetTexture(PropertyID, (Texture)Value);
        }
    }
}

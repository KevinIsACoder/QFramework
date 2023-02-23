using UnityEngine;
using System.Collections.Generic;
namespace KLFramework.Avatar
{

    //******************************************
    // MaterialColorArg
    //
    // @Author: Kakashi
    // @Email: john.cha@qq.com
    // @Date: 2022-07-30 16:00
    //******************************************
    public class MaterialColorArgGeneric : AbstractMaterialArgGeneric<Color>
    {
        public MaterialColorArgGeneric(string propertyName) : base(propertyName)
        {
        }

        public override Color Value { get; set; }
        public override void Apply(Material material)
        {
            material.SetColor(PropertyID, Value);
        }
    }
    
    public class MaterialColorArg : AbstractMaterialArg
    {
        public MaterialColorArg(string propertyName) : base(propertyName)
        {
        }

        public override object Value { get; set; }
        public override void Apply(Material material)
        {
            var color = (Color)Value;
            material.SetVector(PropertyID, new Vector4(color.r, color.g, color.b, color.a));
        }
    }
}
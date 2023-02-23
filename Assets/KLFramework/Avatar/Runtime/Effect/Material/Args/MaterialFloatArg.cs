using UnityEngine;

namespace KLFramework.Avatar
{

    //******************************************
    // MaterialFloatArg
    //
    // @Author: Kakashi
    // @Email: john.cha@qq.com
    // @Date: 2022-07-29 11:21
    //******************************************
    public class MaterialFloatArgGeneric : AbstractMaterialArgGeneric<float>
    {
        public MaterialFloatArgGeneric(string propertyName) : base(propertyName)
        {
        }
        public override float Value { get; set; }
        public override void Apply(Material material)
        {
            material.SetFloat(PropertyID, Value);
        }
    }
    
    public class MaterialFloatArg : AbstractMaterialArg
    {
        public MaterialFloatArg(string propertyName) : base(propertyName)
        {
        }

        public override object Value { get; set; }
        public override void Apply(Material material)
        {
            material.SetFloat(PropertyID, (float)Value);
        }
    }
}
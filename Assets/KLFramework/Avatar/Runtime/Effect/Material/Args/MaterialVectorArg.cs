using UnityEngine;

namespace KLFramework.Avatar
{

    //******************************************
    // MaterialVectorArg
    //
    // @Author: Kakashi
    // @Email: john.cha@qq.com
    // @Date: 2022-07-30 15:50
    //******************************************
    public class MaterialVectorArgGeneric : AbstractMaterialArgGeneric<Vector4>
    {
        public MaterialVectorArgGeneric(string propertyName) : base(propertyName)
        {
        }

        public override Vector4 Value { get; set; }
        public override void Apply(Material material)
        {
            material.SetVector(PropertyID, Value);
        }
    }
    
    public class MaterialVectorArg : AbstractMaterialArg
    {
        public MaterialVectorArg(string propertyName) : base(propertyName)
        {
        }

        public override object Value { get; set; }
        public override void Apply(Material material)
        {
            if (Value is Vector4)
            {
                material.SetVector(PropertyID, (Vector4)Value);    
            }
            else if (Value is Vector3)
            {
                material.SetVector(PropertyID, (Vector3)Value);
            }
            else if (Value is Vector2)
            {
                material.SetVector(PropertyID, (Vector2)Value);
            }
        }
    }
}
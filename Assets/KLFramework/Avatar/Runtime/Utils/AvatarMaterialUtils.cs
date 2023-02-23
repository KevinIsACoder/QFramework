using System.Collections.Generic;
using UnityEngine;

namespace KLFramework.Avatar
{

    //******************************************
    // AvatarMaterialUtils
    //
    // @Author: Kakashi
    // @Email: john.cha@qq.com
    // @Date: 2022-07-29 09:59
    //******************************************
    public class AvatarMaterialUtils
    {
        public static void SetVector(List<Material> materials, int propertyID, Vector4 vector)
        {
            if (materials == null)
            {
                return;
            }

            for (int i = 0; i < materials.Count; i++)
            {
                materials[i].SetVector(propertyID, vector);
            }
        }
        
        public static void SetFloat(List<Material> materials, int propertyID, float f)
        {
            if (materials == null)
            {
                return;
            }

            for (int i = 0; i < materials.Count; i++)
            {
                materials[i].SetFloat(propertyID, f);
            }
        }
        
        public static void SetColor(List<Material> materials, int propertyID, Color c)
        {
            if (materials == null)
            {
                return;
            }

            for (int i = 0; i < materials.Count; i++)
            {
                materials[i].SetColor(propertyID, c);
            }
        }
    }
}
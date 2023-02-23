
using UnityEngine;

namespace KLFramework.Avatar
{
    //******************************************
    // IMaterialArg
    //
    // @Author: Kakashi
    // @Email: john.cha@qq.com
    // @Date: 2022-07-29 11:17
    //******************************************
    public interface IMaterialArgGeneric<T>
    {
        string PropertyName { get; }
        int PropertyID { get; }
        T Value { get; set; }
        void Apply(Material material);
        bool Dirty{get; set;}
    }
    
    public interface IMaterialArg : IMaterialArgGeneric<object>
    {
    }
}
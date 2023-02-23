using UnityEngine;

namespace KLFramework.Avatar
{
    //******************************************
    // ISkeleton
    //
    // @Author: Kakashi
    // @Email: john.cha@qq.com
    // @Date: 2022-07-25 17:09
    //******************************************
    public interface ISkeleton
    {
        Transform Find(string boneName);
        Transform GetSkeleton();
    }
}
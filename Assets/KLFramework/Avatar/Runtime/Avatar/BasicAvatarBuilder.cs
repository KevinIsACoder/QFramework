using UnityEngine;

namespace KLFramework.Avatar
{

    //******************************************
    // BasicAvatarBuilder
    //
    // @Author: Kakashi
    // @Email: john.cha@qq.com
    // @Date: 2022-07-28 11:42
    //******************************************
    public class BasicAvatarBuilder<C>
    {
        private IPartManager<C> _PartManager;
        private Transform _Parent;
        private string _Name;
        
        public BasicAvatarBuilder<C> SetPartManage(IPartManager<C> partManager)
        {
            _PartManager = partManager;
            return this;
        }
        public BasicAvatarBuilder<C> SetParent(Transform parent)
        {
            _Parent = parent;
            return this;
        }
        public BasicAvatarBuilder<C> SetName(string name)
        {
            _Name = name;
            return this;
        }
        public BasicAvatar<C> Build()
        {
            return new BasicAvatar<C>(_PartManager, _Parent, _Name);
        }
    }
}